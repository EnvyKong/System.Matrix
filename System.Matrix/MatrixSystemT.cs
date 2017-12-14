using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.Matrix
{
    class MatrixSystemT : IMatrixSystem, ICalibrate, IDeviceMember
    {
        public MatrixSystemT(IEntryData data)
        {
            EntryData = data;
        }

        public IEntryData EntryData { get; }

        public IVectorNetworkAnalyzer VNA { get; set; }
        public ICalibratable Matrix { get; set; }
        public ICalibratable[] Vertexs { get; set; }
        public ICalibratable CalBoxToMatrix { get; set; }
        public ICalibratable CalBoxToVertex { get; set; }
        public SwitchAdapter SwitchAdapter { get; set; }

        public void Calibrate()
        {
            try
            {
                SwitchAdapter = new SwitchAdapter(CalBoxToMatrix as ISwitch, CalBoxToVertex as ISwitch);
                var _signalPaths = new List<SignalPath>();
                Thread getCalBoxDatasThread = new Thread((calBoxToMatrix) =>
                {
                    (calBoxToMatrix as CalBoxToMatrix).GetCalBoxData();
                })
                {
                    CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                    Name = "子线程：获取校准盒子数据。",
                    IsBackground = true
                };
                //获取校准盒子数据
                getCalBoxDatasThread.Start(CalBoxToMatrix);

                Thread resetMatrixThread = new Thread((matrix) =>
                {
                    var m = matrix as Matrix;
                    m.ResetAttAndPha();
                })
                {
                    CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                    Name = "子线程：设置MCS衰减相位归零。",
                    IsBackground = true
                };
                //相位衰减置零
                resetMatrixThread.Start(Matrix);

                //等待子线程完成任务
                getCalBoxDatasThread.Join();
                resetMatrixThread.Join();

                int attCalFre = (Matrix as Matrix).AttCalFre;
                int phaCalFre = (Matrix as Matrix).PhaCalFre;
                int maxCalCount = attCalFre > phaCalFre ? attCalFre : phaCalFre;
                for (int i = 1; i <= maxCalCount; i++)
                {
                    if (i <= attCalFre)
                    {
                        //开始获取通道衰减
                        _signalPaths = GetAllSignalPathData();
                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("通道总数量为{0}。Vertex台数为{1}。", _signalPaths.Count, Vertexs.Length);
                        }

                        //找到衰减最小值
                        SignalPath.ExpectAttStandard = _signalPaths.Select(s => s.Attenuation).Min();

                        Thread setMatrixAttThread = new Thread((matrix) =>
                        {
                            var m = matrix as Matrix;
                            m.SetAtt(_signalPaths.Select(s => s.ChannelToMatrix).ToList(), true);
                        })
                        {
                            CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                            Name = "子线程：设置MCS补偿衰减。",
                            IsBackground = true
                        };
                        setMatrixAttThread.Start(Matrix);
                        setMatrixAttThread.Join();

                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("第{0}次衰减校准完成。", i);
                        }

                    }
                    if (i <= phaCalFre)
                    {
                        //取相位
                        for (int b = 1; b <= Matrix.BPortConnectNum; b++)
                        {
                            //下行
                            var vertexID = (b - 1) / Vertexs[0].APortConnectNum;
                            var inPortID = (b - 1) % Vertexs[0].APortConnectNum + 1;
                            var outPortID = 1;

                            (Vertexs[vertexID] as Vertex).OpenChannel(inPortID, outPortID, UpDown.DOWN);

                            if (Log.log.IsInfoEnabled)
                            {
                                Log.log.InfoFormat("第{0}台Vertex响应。打开通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                            }

                            for (int a = 1; a <= Matrix.APortConnectNum; a++)
                            {
                                var calBoxAPortID = a;
                                var calBoxBPortID = ((b - 1) / Vertexs[0].APortConnectNum) * Vertexs[0].BPortConnectNum + 1;
                                //(CalBoxToMatrix as CalBoxToMatrix).Set64B16Switch(calBoxAPortID, calBoxBPortID, 1, 1);
                                //Switch.DoSwitch(calBoxAPortID, calBoxBPortID);
                                SwitchAdapter.DoSwitch(calBoxAPortID, calBoxBPortID);
                                if (Log.log.IsInfoEnabled)
                                {
                                    Log.log.InfoFormat("相位校准阶段切开关 {0}{1} OK。", calBoxAPortID, calBoxBPortID);
                                }
                                _signalPaths.Find(s => s.Index.Equals($"{a}:{b}:1")).Phase = VNA.GetMarkerY(VNA.PhaMarkPoint);
                            }
                            (Vertexs[vertexID] as Vertex).CloseChannel(inPortID, outPortID, UpDown.DOWN);

                            if (Log.log.IsInfoEnabled)
                            {
                                Log.log.InfoFormat("第{0}台Vertex响应。关闭通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                            }
                        }
                        Thread setMatrixPhaThread = new Thread((matrix) =>
                        {
                            var m = matrix as Matrix;
                            m.SetPha(_signalPaths.Select(s => s.ChannelToMatrix).ToList(), true);
                        })
                        {
                            CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                            Name = "子线程：设置MCS补偿相位。",
                            IsBackground = true,
                        };

                        setMatrixPhaThread.Start(Matrix);

                        setMatrixPhaThread.Join();

                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("第{0}次相位校准完成。", i);
                        }
                    }
                }
                OutputResult(Device.CALIBRATE_OFFSET_DATA_PATH);
                Log.log.Info("MCS Calibrate Successfully! Please Start Vertex Self Calibrate!");
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        private List<SignalPath> GetAllSignalPathData()
        {
            var signalPaths = new List<SignalPath>();
            Log.log.Info("Start The Calibration.");

            VNA.SetMarkerActive();
            VNA.SetMarkerX((Matrix as Matrix).Frequency * 1000000);
            //关闭Vertex所有通道，后面用哪个打开哪个
            foreach (var v in Vertexs)
            {
                (v as Vertex).CloseAllChannel(v.APortNum, v.BPortNum);
            }
            for (int b = 1; b <= Matrix.BPortConnectNum; b++)
            {
                //下行
                var vertexID = (b - 1) / Vertexs[0].APortConnectNum;
                var inPortID = (b - 1) % Vertexs[0].APortConnectNum + 1;
                var outPortID = 1;

                (Vertexs[vertexID] as Vertex).OpenChannel(inPortID, outPortID, UpDown.DOWN);

                if (Log.log.IsInfoEnabled)
                {
                    Log.log.InfoFormat("第{0}台Vertex响应。打开通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                }

                for (int a = 1; a <= Matrix.APortConnectNum; a++)
                {
                    var calBoxAPortID = a;
                    var calBoxBPortID = ((b - 1) / Vertexs[0].APortConnectNum) * Vertexs[0].BPortConnectNum + 1;

                    //(CalBoxToMatrix as CalBoxToMatrix).Set64B16Switch(calBoxAPortID, calBoxBPortID, 1, 1);
                    //Switch.DoSwitch(calBoxAPortID, calBoxBPortID);
                    SwitchAdapter.DoSwitch(calBoxAPortID, calBoxBPortID);
                    if (Log.log.IsInfoEnabled)
                    {
                        Log.log.InfoFormat("衰减校准阶段切开关 {0}{1} OK。", calBoxAPortID, calBoxBPortID);
                    }
                    //(CalBoxToMatrix as CalBoxToMatrix).SetSwitch(a);
                    //calBoxToVertex .SetSwitch(c);
                    var signalPath = new SignalPath((CalBoxToMatrix as CalBoxToMatrix).CalBoxData, EntryData)
                    {
                        APortID = a,
                        BPortID = b,
                        CPortID = 1,
                        Attenuation = VNA.GetMarkerY(VNA.AttMarkPoint),
                    };
                    signalPaths.Add(signalPath);
                }
                (Vertexs[vertexID] as Vertex).CloseChannel(inPortID, outPortID, UpDown.DOWN);

                if (Log.log.IsInfoEnabled)
                {
                    Log.log.InfoFormat("第{0}台Vertex响应。关闭通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                }
            }
            return signalPaths;
        }

        public void CalibrateByFile(string path)
        {
            throw new NotImplementedException();
        }

        public void ConnectAll()
        {
            try
            {
                Matrix = new Matrix(EntryData);
                Vertexs = new Vertex[Vertexs[0].Quantity];
                for (int i = 1; i <= Vertexs[0].Quantity; i++)
                {
                    var dataType = EntryData.GetType();
                    var ip = dataType.GetProperty("IPToVertex" + i).ToString();
                    Vertexs[i] = new Vertex(EntryData);
                }
                CalBoxToMatrix = new CalBoxToMatrix(EntryData);
                CalBoxToVertex = new CalBoxToVertex(EntryData);
                VNA = VNAFactory.GetVNA(EntryData);

                Matrix.Connect();
                Vertexs.ForEach(D => D.Connect());
                CalBoxToMatrix.Connect();
                CalBoxToVertex.Connect();
                VNA.Connect();
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        public void DisConnectAll()
        {
            try
            {
                Matrix.Close();
                Vertexs.ForEach(D => D.Close());
                CalBoxToMatrix.Close();
                CalBoxToVertex.Close();
                VNA.Close();
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void OutputResult(string savePath)
        {
            throw new NotImplementedException();
        }

        public void SetValueDynamic(string path)
        {
            throw new NotImplementedException();
        }

        public void SetValueStatic(string path)
        {
            throw new NotImplementedException();
        }
    }
}
