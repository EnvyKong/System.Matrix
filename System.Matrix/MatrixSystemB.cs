using FluentFTP;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace System.Matrix
{
    public class MatrixSystemB : MatrixSystem
    {
        public MatrixSystemB(List<DeviceData> deviceDatas) : base(deviceDatas)
        {

        }
        private Vertex _vertex1;
        private Vertex _vertex2;

        private List<SignalPath> _signalPaths;

        public override void Initialize()
        {
            Matrix = new Matrix(DeviceData.Find(d => d.TypeName.ToLower().Contains("matrix")));
            _vertex1 = new Vertex(DeviceData.Find(d => d.TypeName.ToLower().Contains("vertex1")));
            _vertex2 = new Vertex(DeviceData.Find(d => d.TypeName.ToLower().Contains("vertex2")));
            Vertexs = new List<Vertex> { _vertex1, _vertex2 };
            CalBoxToMatrix = new CalBoxToMatrix(DeviceData.Find(d => d.TypeName.ToLower().Contains("calboxtomatrix")));
            CalBoxToVertex = new CalBoxToVertex(DeviceData.Find(d => d.TypeName.ToLower().Contains("calboxtovertex")));
            CalBoxWhole = new CalBoxWhole(DeviceData.Find(d => d.TypeName.ToLower().Contains("calboxwhole")));
            VNA = VNAFactory.GetVNA(DeviceData.Find(d => d.TypeName.ToLower().Contains("vna")));
            SwitchAdapter = new SwitchAdapter<ISwitch>(CalBoxToMatrix, CalBoxToVertex, CalBoxWhole);
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

        private List<SignalPath> GetAllSignalPathData()
        {
            var signalPaths = new List<SignalPath>();
            Log.log.Info("Start The Calibration.");

            VNA.SetMarkerActive();
            VNA.SetMarkerX(Matrix.Frequency * 1000000);
            //关闭Vertex所有通道，后面用哪个打开哪个
            foreach (var v in Vertexs)
            {
                v.CloseAllChannel(v.APortNum, v.BPortNum);
            }
            //for (int c = 1; c <= vertex.BPortNum; c++)
            //{
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

                for (int a = 1; a <= Matrix.APortConnectNum; a++)
                {
                    var calBoxAPortID = a;
                    var calBoxBPortID = ((b - 1) / Vertexs[0].APortConnectNum) * Vertexs[0].BPortConnectNum + 1;
                    SwitchAdapter.DoSwitch(calBoxAPortID, calBoxBPortID);
                    //_calBoxToMatrix.Set64B16Switch(calBoxAPortID, calBoxBPortID, 1, 1);
                    //_calBoxToMatrix.SetSwitch(calBoxAPortID);
                    //_calBoxToVertex.SetSwitch(calBoxBPortID);
                    if (Log.log.IsInfoEnabled)
                    {
                        Log.log.InfoFormat("衰减校准阶段切开关 {0}{1} OK。", calBoxAPortID, calBoxBPortID);
                    }

                    var signalPath = new SignalPath(SwitchAdapter.CalBoxData, Matrix.DeviceData)
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

        public void OutputResultWithForm()
        {
            try
            {
                SaveFileDialog saveFile = new SaveFileDialog()
                {
                    Filter = "Text File (*.txt)|*.txt|CSV File (*.csv)|*.csv",
                    Title = "Export Calibration Offset",
                    RestoreDirectory = true,
                    FilterIndex = 1,
                    FileName = "CalibrationData"
                };
                if (saveFile.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                string savePath = saveFile.FileName;
                OutputResult(savePath);
                Log.log.Info("Output Completed！");
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

        public override void Calibrate()
        {
            try
            {
                TaskFactory taskFactory = new TaskFactory();
                Task[] tasks = new Task[]
                {
                    taskFactory.StartNew(SwitchAdapter.GetCalBoxData),
                    taskFactory.StartNew(Matrix.ResetAttAndPha)
                };
                taskFactory.ContinueWhenAll(tasks, TasksEnded);
                #region used Thread
                //Thread getCalBoxDatasThread = new Thread((calBox) =>
                //{
                //    (calBox as SwitchAdapter<ISwitch>).GetCalBoxData();
                //})
                //{
                //    CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                //    Name = "子线程：获取校准盒子数据。",
                //    IsBackground = true
                //};
                ////获取校准盒子数据
                //getCalBoxDatasThread.Start(_switchAdapter);

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
                //resetMatrixThread.Start(_matrix);

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
                        _signalPaths = GetAllSignalPathData();
                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("通道总数量为{0}。Vertex台数为{1}。", _signalPaths.Count, Vertexs.Count);
                        }

                        //找到衰减最小值
                        SignalPath.ExpectAttStandard = _signalPaths.Select(s => s.Attenuation).Min();

                        Task taskSetMatrixAtt = new Task(() =>
                        {
                            Matrix.SetAtt(_signalPaths.Select(s => s.ChannelToMatrix).ToList(), true);
                        });
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
                        //setMatrixAttThread.Start(_matrix);
                        //setMatrixAttThread.Join();

                        #endregion

                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("第{0}次衰减校准完成。", i);
                        }

                    }
                    if (i <= phaCalFre)
                    {
                        //matrix.Channels = new List<Channel>();
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

                            for (int a = 1; a <= Matrix.APortConnectNum; a++)
                            {
                                var calBoxAPortID = a;
                                var calBoxBPortID = ((b - 1) / Vertexs[0].APortConnectNum) * Vertexs[0].BPortConnectNum + 1;
                                //_calBoxToMatrix.Set64B16Switch(calBoxAPortID, calBoxBPortID, 1, 1);
                                //_switch.DoSwitch(calBoxAPortID, calBoxBPortID);
                                SwitchAdapter.DoSwitch(calBoxAPortID, calBoxBPortID);
                                if (Log.log.IsInfoEnabled)
                                {
                                    Log.log.InfoFormat("相位校准阶段切开关 {0}{1} OK。", calBoxAPortID, calBoxBPortID);
                                }
                                _signalPaths.Find(s => s.Index.Equals($"{a}:{b}:1")).Phase = VNA.GetMarkerY(VNA.PhaMarkPoint);
                            }
                            Vertexs[vertexID].CloseChannel(inPortID, outPortID, UpDown.DOWN);

                            if (Log.log.IsInfoEnabled)
                            {
                                Log.log.InfoFormat("第{0}台Vertex响应。关闭通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                            }
                        }
                        #region 上下行
                        //foreach (var signalPath in signalPaths)
                        //{

                        //上行
                        //vertex.OpenChannel(signalPath.CPortID, signalPath.BPortID, UpDown.UP);
                        //signalPath.Phase = vNAE5061B.GetMarkerY(int.Parse(ConfigurationManager.AppSettings["Pha Mark Point"]));
                        //signalPath.GetOffsetPhaToMatrix();
                        //signalPath.GetOffsetPhaToVertex(UpDown.UP);
                        //vertex.CloseChannel(signalPath.CPortID, signalPath.BPortID, UpDown.UP);

                        //Thread.Sleep(10);

                        //signalPath.Attenuation = vNARSZVB8.GetMarkerY(int.Parse(ConfigurationManager.AppSettings["Att Mark Point"]));
                        //if (signalPath.Attenuation < -100)
                        //{
                        //    MessageBox.Show("Error.");
                        //}
                        //signalPath.GetOffsetPhaToMatrix();
                        //signalPath.GetOffsetPhaToVertex(UpDown.DOWN);
                        //vertex.CloseChannel(signalPath.BPortID, signalPath.CPortID, UpDown.DOWN);
                        //Thread.Sleep(10);
                        //}
                        #endregion

                        Task taskSetMatrixPha = new Task(() =>
                        {
                            Matrix.SetPha(_signalPaths.Select(s => s.ChannelToMatrix).ToList(), true);
                        });
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

                        //setMatrixPhaThread.Start(_matrix);

                        //setMatrixPhaThread.Join();

                        #endregion

                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("第{0}次相位校准完成。", i);
                        }
                    }
                }
                OutputResult(Device.CALIBRATE_OFFSET_DATA_PATH);
                Log.log.Info("MCS calibrate successfully! Please start Vertex self calibrate!");
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        private void TasksEnded(Task[] tasks)
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

        public void CalibrateByFileWithForm()
        {
            try
            {
                OpenFileDialog loadPhaseOffset = new OpenFileDialog()
                {
                    Filter = "Text File (*.txt)|*.txt|CSV File (*.csv)|*.csv",
                    Title = "Import Calibration Offset",
                    RestoreDirectory = true,
                    FilterIndex = 1
                };
                if (loadPhaseOffset.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                CalibrateByFile(loadPhaseOffset.FileName);
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        public override void CalibrateByFile(string path)
        {
            try
            {
                var offsets = File.ReadAllLines(path);
                for (int i = 0; i < offsets.Length; i++)
                {
                    var offset = offsets[i].Split(':');
                    int id = int.Parse(offset[0]);
                    int pha = int.Parse(offset[3]);
                    int att = int.Parse(offset[4]);
                    Matrix.SetPhaAndAtt(id, pha, att);
                }
                MessageBox.Show("Calibrate successfully!");
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        public void SetValueStaticWithForm()
        {
            try
            {
                OpenFileDialog loadPhase = new OpenFileDialog()
                {
                    Filter = "Text File (*.txt)|*.txt|CSV File (*.csv)|*.csv",
                    Title = "Load Phase Value",
                    RestoreDirectory = true,
                    FilterIndex = 1
                };
                if (loadPhase.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                SetValueStatic(loadPhase.FileName);
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
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

        public void SetValueDynamicWithForm()
        {
            try
            {
                OpenFileDialog loadScene = new OpenFileDialog()
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    Title = "Load Scene",
                    RestoreDirectory = true,
                    FilterIndex = 1
                };
                if (loadScene.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                SetValueDynamic(loadScene.FileName);
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }

        public override void SetValueDynamic(string path)
        {
            try
            {
                Scene scene = new Scene();
                scene.LoadSceneData(path, Matrix);

                FtpClient ftpClient = new FtpClient(Matrix.IP, "root", "123456");
                var dbFileName = $"ASP{Matrix.APortNum}B{Matrix.BPortNum}.db";
                var ftpPath = $"ftp://{Matrix.IP}/media/mmcblk0p1/{dbFileName}";

                ftpClient.DownloadFile($"{dbFileName}", $"/media/mmcblk0p1/{dbFileName}");

                ;
                var fileName = Path.GetFileNameWithoutExtension(path);
                string tableName = string.Format("S{0}_{1}", fileName, DateTime.Now.ToString("yyyy_M_d_H_m_s"));
                string timeCmd = DateTime.Now.ToString("yyyy/M/d H-m-s");

                string addInfoTableSQL = $"CREATE TABLE[{tableName}_INFO]([ID] INTEGER PRIMARY KEY AUTOINCREMENT, [Frame] int NULL, [Delay] int NULL, [Freq] int NULL)";
                string addFrameTableSQL = "";
                string sql1 = "";
                string insertFrameInfo = "";
                List<string> sqls = new List<string>();
                for (int i = 0; i < scene.Frames.Count; i++)
                {
                    addFrameTableSQL = string.Format("CREATE TABLE[{0}_F{1}]([Id] INT IDENTITY(1, 1) PRIMARY KEY NOT NULL, [Value] int NULL, [Ch] int NULL)", tableName, i + 1);
                    sqls.Add(addFrameTableSQL);
                    sql1 = $"INSERT INTO {tableName}_INFO ([Frame],[Delay],[Freq]) VALUES({i + 1},{scene.Frames[i]},{i + 1}) ";
                    sqls.Add(sql1);
                    for (int j = 0; j < scene.Frames[i].ChannelToMatrixCollection.Count; j++)
                    {
                        insertFrameInfo = string.Format("INSERT INTO {0}_F{1} (Id,[Value],[Ch]) VALUES({2},{3},{4})", tableName, i + 1, j + 1, scene.Frames[i].ChannelToMatrixCollection[j].PhaCode, j + 1);
                        sqls.Add(insertFrameInfo);
                    }
                }
                //SQLiteHelper.ExecuteSqlTran(addInfoTableSQL);
                //SQLiteHelper.ExecuteInsertSqlTran(sqls);

                ftpClient.UploadFile($"{dbFileName}", ftpPath);

                string frameInfo = $"{fileName} {timeCmd} {scene.Frames.Count}";
                Matrix.AddFrameInfo(frameInfo);

                string checkInfo = $"{fileName} {timeCmd}";
                Matrix.CheckDB(checkInfo);
                string downLoadInfo = $"{fileName} {timeCmd}";
                Matrix.DownLoadDBToFirmware(downLoadInfo);
                string playInfo = $"1:{fileName} {timeCmd} 1";
                Matrix.PlayScene(playInfo);

                MessageBox.Show("Start playing.");
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
                throw ex;
            }
        }
    }
}