using FluentFTP;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TopYoung.MV.Core;

namespace System.Matrix
{
    public class Operation
    {
        Matrix matrix;
        Vertex[] vertexs = new Vertex[AppConfigInfo.VertexQuantity];
        CalBoxToMatrix calBoxToMatrix;
        CalBoxToVertex calBoxToVertex;
        IVectorNetworkAnalyzer VNA;
        List<SignalPath> signalPaths;

        private List<IConnected> Devices
        {
            get
            {
                var devices = new List<IConnected>
                {
                    matrix,
                    calBoxToMatrix,
                    calBoxToVertex,
                    VNA
                };
                devices.AddRange(vertexs);
                return devices;
            }
        }

        private void DoCalibrate()
        {
            throw new NotImplementedException();
        }

        private void ConnectAll(IEntryData data)
        {
            try
            {
                matrix = new Matrix(data.IPToMatrix, 3000);
                for (int i = 1; i <= AppConfigInfo.VertexQuantity; i++)
                {
                    var dataType = data.GetType();
                    var ip = dataType.GetProperty("IPToVertex" + i).ToString();
                    vertexs[i] = new Vertex(ip, 3000);
                }
                calBoxToMatrix = new CalBoxToMatrix(data.IPToCalBoxForMatrix, 3000);
                calBoxToVertex = new CalBoxToVertex(data.IPToCalBoxForVertex, 3000);
                VNA = VNAFactory.GetVNA(data.IPToVNA, -1);

                foreach (var device in Devices)
                {
                    device.Connect();
                }
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
            }
        }

        private void DisConnectAll()
        {
            try
            {
                foreach (var device in Devices)
                {
                    device?.Close();
                }
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
            }
        }

        private List<SignalPath> GetAllSignalPathData(CalBoxToMatrix calBoxToMatrix, CalBoxToVertex calBoxToVertex, IVectorNetworkAnalyzer VNA, Matrix matrix, Vertex[] vertex)
        {
            var signalPaths = new List<SignalPath>();
            Log.log.Info("Start The Calibration.");

            VNA.SetMarkerActive();
            VNA.SetMarkerX(ViewConfigInfo.Frequency * 1000000);
            //关闭Vertex所有通道，后面用哪个打开哪个
            foreach (var v in vertex)
            {
                v.CloseAllChannel(v.APortNum, v.BPortNum);
            }
            //for (int c = 1; c <= vertex.BPortNum; c++)
            //{
            for (int b = 1; b <= matrix.BPortConnectNum; b++)
            {
                //下行
                //var groupSignalPaths = new List<SignalPath>();
                var vertexID = (b - 1) / AppConfigInfo.VertexAConnectNum;
                var inPortID = (b - 1) % AppConfigInfo.VertexAConnectNum + 1;
                var outPortID = 1;

                vertex[vertexID].OpenChannel(inPortID, outPortID, UpDown.DOWN);

                if (Log.log.IsInfoEnabled)
                {
                    Log.log.InfoFormat("第{0}台Vertex响应。打开通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                }

                for (int a = 1; a <= matrix.APortConnectNum; a++)
                {
                    var calBoxAPortID = a;
                    var calBoxBPortID = ((b - 1) / AppConfigInfo.VertexAConnectNum) * AppConfigInfo.VertexBConnectNum + 1;

                    calBoxToMatrix.Set64B16Switch(calBoxAPortID, calBoxBPortID, 1, 1);

                    if (Log.log.IsInfoEnabled)
                    {
                        Log.log.InfoFormat("衰减校准阶段切开关 {0}{1} OK。", calBoxAPortID, calBoxBPortID);
                    }
                    calBoxToMatrix.SetSwitch(a);
                    //calBoxToVertex.SetSwitch(c);
                    var signalPath = new SignalPath(calBoxToMatrix.CalBoxData)
                    {
                        APortID = a,
                        BPortID = b,
                        CPortID = 1,
                        Attenuation = VNA.GetMarkerY(AppConfigInfo.AttMarkPoint),
                    };
                    signalPaths.Add(signalPath);
                }
                vertex[vertexID].CloseChannel(inPortID, outPortID, UpDown.DOWN);

                if (Log.log.IsInfoEnabled)
                {
                    Log.log.InfoFormat("第{0}台Vertex响应。关闭通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                }
            }
            return signalPaths;
        }

        private void Output()
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
                SaveTxtOrCsv(savePath);
                Log.log.Info("Output Completed！");
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
            }
        }

        private void SaveTxtOrCsv(string savePath)
        {
            for (int a = 1; a <= matrix.APortConnectNum; a++)
            {
                for (int b = 1; b <= matrix.BPortConnectNum; b++)
                {
                    string[] offset = new string[1];
                    offset[0] = $"{matrix[a, b]}:{a}:{b}:{matrix.CurrentPha(matrix[a, b])}:{matrix.CurrentAtt(matrix[a, b])}";
                    File.AppendAllLines(savePath, offset);
                }
            }
        }

        private void Calibrate()
        {
            try
            {
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
                getCalBoxDatasThread.Start(calBoxToMatrix);

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
                resetMatrixThread.Start(matrix);

                //等待子线程完成任务
                getCalBoxDatasThread.Join();
                resetMatrixThread.Join();

                int attCalFre = AppConfigInfo.AttCalFre;
                int phaCalFre = AppConfigInfo.PhaCalFre;
                int maxCalCount = attCalFre > phaCalFre ? attCalFre : phaCalFre;
                for (int i = 1; i <= maxCalCount; i++)
                {
                    if (i <= attCalFre)
                    {
                        //开始获取通道衰减
                        signalPaths = GetAllSignalPathData(calBoxToMatrix, calBoxToVertex, VNA, matrix, vertexs);
                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("通道总数量为{0}。Vertex台数为{1}。", signalPaths.Count, vertexs.Length);
                        }

                        //找到衰减最小值
                        SignalPath.ExpectAttStandard = signalPaths.Select(s => s.Attenuation).Min();

                        Thread setMatrixAttThread = new Thread((matrix) =>
                        {
                            var m = matrix as Matrix;
                            m.SetAtt(signalPaths.Select(s => s.ChannelToMatrix).ToList(), true);
                        })
                        {
                            CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                            Name = "子线程：设置MCS补偿衰减。",
                            IsBackground = true
                        };
                        setMatrixAttThread.Start(matrix);
                        setMatrixAttThread.Join();

                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("第{0}次衰减校准完成。", i);
                        }

                    }
                    if (i <= phaCalFre)
                    {
                        //matrix.Channels = new List<Channel>();
                        //取相位
                        for (int b = 1; b <= matrix.BPortConnectNum; b++)
                        {
                            //下行
                            //vertex.OpenChannel(b, 1, UpDown.DOWN);
                            //vertexs[b / AppConfigInfo.VertexAConnectNum].OpenChannel(b % AppConfigInfo.VertexAConnectNum, 1, UpDown.DOWN);
                            var vertexID = (b - 1) / AppConfigInfo.VertexAConnectNum;
                            var inPortID = (b - 1) % AppConfigInfo.VertexAConnectNum + 1;
                            var outPortID = 1;

                            vertexs[vertexID].OpenChannel(inPortID, outPortID, UpDown.DOWN);

                            if (Log.log.IsInfoEnabled)
                            {
                                Log.log.InfoFormat("第{0}台Vertex响应。打开通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                            }

                            for (int a = 1; a <= matrix.APortConnectNum; a++)
                            {
                                var calBoxAPortID = a;
                                var calBoxBPortID = ((b - 1) / AppConfigInfo.VertexAConnectNum) * AppConfigInfo.VertexBConnectNum + 1;
                                calBoxToMatrix.Set64B16Switch(calBoxAPortID, calBoxBPortID, 1, 1);
                                if (Log.log.IsInfoEnabled)
                                {
                                    Log.log.InfoFormat("相位校准阶段切开关 {0}{1} OK。", calBoxAPortID, calBoxBPortID);
                                }
                                signalPaths.Find(s => s.Index.Equals($"{a}:{b}:1")).Phase = VNA.GetMarkerY(AppConfigInfo.PhaMarkPoint);
                            }
                            //vertex.CloseChannel(b, 1, UpDown.DOWN);
                            //vertexs[b / AppConfigInfo.VertexAConnectNum].CloseChannel(b % AppConfigInfo.VertexAConnectNum, 1, UpDown.DOWN);
                            vertexs[vertexID].CloseChannel(inPortID, outPortID, UpDown.DOWN);

                            if (Log.log.IsInfoEnabled)
                            {
                                Log.log.InfoFormat("第{0}台Vertex响应。关闭通道{1}{2}，方向{3}。", vertexID, inPortID, outPortID, UpDown.DOWN);
                            }
                        }
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
                        Thread setMatrixPhaThread = new Thread((matrix) =>
                        {
                            var m = matrix as Matrix;
                            m.SetPha(signalPaths.Select(s => s.ChannelToMatrix).ToList(), true);
                        })
                        {
                            CurrentCulture = Globalization.CultureInfo.InvariantCulture,
                            Name = "子线程：设置MCS补偿相位。",
                            IsBackground = true,
                        };

                        setMatrixPhaThread.Start(matrix);

                        setMatrixPhaThread.Join();

                        if (Log.log.IsInfoEnabled)
                        {
                            Log.log.InfoFormat("第{0}次相位校准完成。", i);
                        }
                    }
                }


                SaveTxtOrCsv(Device.CALIBRATE_OFFSET_DATA_PATH);
                Log.log.Info("MCS Calibrate Successfully! Please Start Vertex Self Calibrate!");
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
            }
        }

        private void CalibrateByFile()
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
                var offsets = File.ReadAllLines(loadPhaseOffset.FileName);
                for (int i = 0; i < offsets.Length; i++)
                {
                    var offset = offsets[i].Split(':');
                    int id = int.Parse(offset[0]);
                    int pha = int.Parse(offset[3]);
                    int att = int.Parse(offset[4]);
                    matrix.SetPhaAndAtt(id, pha, att);
                }
                MessageBox.Show("Calibrate successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Log.log.ErrorFormat("{0}", ex);
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
                throw ex;
            }
        }

        public void SetValueStatic(string path)
        {
            try
            {
                var channels = new List<Channel>();
                if (path.ToLower().EndsWith(".txt"))
                {
                    matrix.LoadOffsets(path, out channels);
                }
                else if (path.ToLower().EndsWith(".csv"))
                {
                    Scene scene = new Scene();
                    scene.LoadSceneData(path, matrix);
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
                matrix.SetPha(channels, false);
                Log.log.Info("Play successfully!");
            }
            catch (Exception ex)
            {
                Log.log.ErrorFormat("{0}", ex);
            }
        }

        public void SetValueDynamicWithForm()
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

        public void SetValueDynamic(string path)
        {
            try
            {
                Scene scene = new Scene();
                scene.LoadSceneData(path, matrix);

                FtpClient ftpClient = new FtpClient(matrix.IP, "root", "123456");
                var dbFileName = $"ASP{matrix.APortNum}B{matrix.BPortNum}.db";
                var ftpPath = $"ftp://{matrix.IP}/media/mmcblk0p1/{dbFileName}";

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
                SQLiteHelper.ExecuteSqlTran(addInfoTableSQL);
                SQLiteHelper.ExecuteInsertSqlTran(sqls);

                ftpClient.UploadFile($"{dbFileName}", ftpPath);

                string frameInfo = $"{fileName} {timeCmd} {scene.Frames.Count}";
                matrix.AddFrameInfo(frameInfo);

                string checkInfo = $"{fileName} {timeCmd}";
                matrix.CheckDB(checkInfo);
                string downLoadInfo = $"{fileName} {timeCmd}";
                matrix.DownLoadDBToFirmware(downLoadInfo);
                string playInfo = $"1:{fileName} {timeCmd} 1";
                matrix.PlayScene(playInfo);

                MessageBox.Show("Start playing.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}