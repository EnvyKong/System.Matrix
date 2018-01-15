using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace System.Matrix
{
    public class MatrixSystemT : MatrixSystem
    {
        public MatrixSystemT(params DeviceData[] deviceDatas) : base(deviceDatas)
        {

        }

        public override void Calibrate()
        {
            try
            {
                var signalPaths = new List<SignalPath>();
                TaskFactory taskFactory = new TaskFactory();
                Task[] tasks = new Task[]
                {
                    taskFactory.StartNew(SwitchAdapter.GetCalBoxData),
                    taskFactory.StartNew(Matrix.ResetAttAndPha)
                };
                taskFactory.ContinueWhenAll(tasks, TasksEnded);

                #region used Thread
                //Thread getCalBoxDatasThread = new Thread((calBoxToMatrix) =>
                //{
                //    (calBoxToMatrix as CalBoxToMatrix).GetCalBoxData();
                //})
                //{
                //    CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                //    Name = "子线程：获取校准盒子数据。",
                //    IsBackground = true
                //};
                ////获取校准盒子数据
                //getCalBoxDatasThread.Start(CalBoxToMatrix);

                //Thread resetMatrixThread = new Thread((matrix) =>
                //{
                //    var m = matrix as Matrix;
                //    m.ResetAttAndPha();
                //})
                //{
                //    CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                //    Name = "子线程：设置MCS衰减相位归零。",
                //    IsBackground = true
                //};
                ////相位衰减置零
                //resetMatrixThread.Start(Matrix);

                ////等待子线程完成任务
                //getCalBoxDatasThread.Join();
                //resetMatrixThread.Join();
                #endregion

                int attCalFre = Matrix.AttCalFre;
                int phaCalFre = Matrix.PhaCalFre;
                int maxCalCount = attCalFre > phaCalFre ? attCalFre : phaCalFre;
                for (int i = 1; i <= maxCalCount; i++)
                {
                    if (i <= attCalFre)
                    {
                        //开始获取通道衰减
                        signalPaths = GetAllSignalPathData();
                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("通道总数量为{0}。Vertex台数为{1}。", signalPaths.Count, Vertex.Count);
                        }

                        //找到衰减最小值
                        SignalPath.ExpectAttStandard = signalPaths.Select(s => s.Attenuation).Min();

                        Task taskSetMatrixAtt = new Task(() => { Matrix.SetAtt(signalPaths.Select(s => s.ChannelToMatrix).ToList(), true); });
                        taskSetMatrixAtt.Start();
                        taskSetMatrixAtt.Wait();

                        #region used Thread
                        //Thread setMatrixAttThread = new Thread((matrix) =>
                        //{
                        //    var m = matrix as Matrix;
                        //    m.SetAtt(_signalPaths.Select(s => s.ChannelToMatrix).ToList(), true);
                        //})
                        //{
                        //    CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                        //    Name = "子线程：设置MCS补偿衰减。",
                        //    IsBackground = true
                        //};
                        //setMatrixAttThread.Start(Matrix);
                        //setMatrixAttThread.Join();
                        #endregion

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

                            Vertexs[vertexID].OpenChannel(inPortID, outPortID, UpDown.DOWN);

                            if (Log.log.IsInfoEnabled)
                            {
                                Log.log.InfoFormat("第{0}台Vertex响应。打开通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                            }

                            for (int a = 1 + (b - 1) * (Matrix.APortNum / Matrix.BPortNum); a <= 2 * (b - 1) * (Matrix.APortNum / Matrix.BPortNum); a++)
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
                                signalPaths.Find(s => s.Index.Equals($"{a}:{b}:1")).Phase = VNA.GetMarkerY(VNA.PhaMarkPoint);
                            }
                            Vertexs[vertexID].CloseChannel(inPortID, outPortID, UpDown.DOWN);

                            if (Log.log.IsInfoEnabled)
                            {
                                Log.log.InfoFormat("第{0}台Vertex响应。关闭通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                            }
                        }

                        Task taskSetMatrixPha = new Task(() => { Matrix.SetPha(signalPaths.Select(s => s.ChannelToMatrix).ToList(), true); });
                        taskSetMatrixPha.Start();
                        taskSetMatrixPha.Wait();

                        #region used Thread
                        //Thread setMatrixPhaThread = new Thread((matrix) =>
                        //{
                        //    var m = matrix as Matrix;
                        //    m.SetPha(_signalPaths.Select(s => s.ChannelToMatrix).ToList(), true);
                        //})
                        //{
                        //    CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                        //    Name = "子线程：设置MCS补偿相位。",
                        //    IsBackground = true,
                        //};

                        //setMatrixPhaThread.Start(Matrix);

                        //setMatrixPhaThread.Join();

                        #endregion

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


        void TasksEnded(Task[] tasks)
        {
            if (tasks[0].IsCompleted)
            {
                Log.log.Info("Get calbox data successfully.");
            }
            if (tasks[1].IsCompleted)
            {
                Log.log.Info("Reset Matrix's Attenuation and Phase successfully.");
            }
        }

        private List<SignalPath> GetAllSignalPathData()
        {
            var signalPaths = new List<SignalPath>();
            Log.log.Info("Start The Calibration.");

            VNA.SetMarkerActive();
            VNA.SetMarkerX((Matrix as Matrix).Frequency * 1000000);
            //关闭Vertex所有通道，后面用哪个打开哪个
            foreach (var vertex in Vertexs)
            {
                vertex.CloseAllChannel(vertex.APortNum, vertex.BPortNum);
            }
            for (int b = 1; b <= Matrix.BPortConnectNum; b++)
            {
                //下行
                var vertexID = (b - 1) / Vertexs[0].APortConnectNum;
                var inPortID = (b - 1) % Vertexs[0].APortConnectNum + 1;
                var outPortID = 1;

                Vertexs[vertexID].OpenChannel(inPortID, outPortID, UpDown.DOWN);

                if (Log.log.IsInfoEnabled)
                {
                    Log.log.InfoFormat("第{0}台Vertex响应。打开通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                }

                for (int a = 1 + (b - 1) * (Matrix.APortNum / Matrix.BPortNum); a <= b * (Matrix.APortNum / Matrix.BPortNum); a++)
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
                    var signalPath = new SignalPath(SwitchAdapter.CalBoxData, Matrix.PhaseStepShiftDirection)
                    {
                        APortID = a,
                        BPortID = b,
                        CPortID = 1,
                        Attenuation = VNA.GetMarkerY(VNA.AttMarkPoint),
                    };
                    signalPaths.Add(signalPath);
                }
                Vertexs[vertexID].CloseChannel(inPortID, outPortID, UpDown.DOWN);

                if (Log.log.IsInfoEnabled)
                {
                    Log.log.InfoFormat("第{0}台Vertex响应。关闭通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                }
            }
            return signalPaths;
        }

        public override void CalibrateByFile(string path)
        {
            throw new NotImplementedException();
        }

        public override void ConnectAll()
        {
            try
            {
                Matrix.Connect();
                Vertexs.ForEach(v => v.Connect());
                CalBoxToMatrix.Connect();
                CalBoxToVertex.Connect();
                CalBoxWhole.Connect();
                VNA.Connect();
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        public override void DisConnectAll()
        {
            try
            {
                Matrix.Close();
                Vertexs.ForEach(v => v.Close());
                CalBoxToMatrix.Close();
                CalBoxToVertex.Close();
                CalBoxWhole.Close();
                VNA.Close();
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        public override void OutputResult(string savePath)
        {
            try
            {
                for (int a = 1; a <= Matrix.APortConnectNum; a++)
                {
                    for (int b = 1; b <= Matrix.BPortConnectNum; b++)
                    {
                        string[] offset = new string[1];
                        offset[0] = $"{Matrix[a, b]}:{a}:{b}:{Matrix.CurrentPha(Matrix[a, b])}:{Matrix.CurrentAtt(Matrix[a, b])}";
                        File.AppendAllLines(savePath, offset);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        public override void SetValueDynamic(string path)
        {
            throw new NotImplementedException();
        }

        public override void SetValueStatic(string path)
        {
            try
            {
                var channels = new List<Channel>();
                if (path.ToLower().EndsWith(".txt"))
                {
                    Matrix.LoadOffsets(path, out channels);
                }
                else if (path.ToLower().EndsWith(".csv"))
                {
                    Scene scene = new Scene();
                    scene.LoadSceneData(path, Matrix);
                    foreach (var frame in scene.Frames)
                    {
                        foreach (var channel in frame.ChannelToMatrixCollection)
                        {
                            channels.Add(new Channel(channel.APortID, channel.BPortID)
                            {
                                PhaOffset = channel.PhaOffset
                            });
                        }
                    }
                }
                Matrix.SetPha(channels, false);
                Log.log.Info("Play successfully!");
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }
    }
}
