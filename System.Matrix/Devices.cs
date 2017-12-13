﻿using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace System.Matrix
{
    public class Matrix : Device
    {
        public Matrix(string ip, int portNum, IEntryData data) : base(ip, portNum)
        {
            try
            {
                EntryData = data;
                ChannelList = new List<Channel>();
                var offsets = File.ReadAllLines(CALIBRATE_OFFSET_DATA_PATH);
                for (int i = 0; i < offsets.Length; i++)
                {
                    var offset = offsets[i].Split(':');
                    ChannelList.Add(new Channel(offset[1].ToInt32(), offset[2].ToInt32())
                    {
                        PhaStdCode = offset[3].ToInt32(),
                        AttStdCode = offset[4].ToInt32()
                    });
                }
            }
            catch (Exception ex)
            {
                if (!(ex is FileNotFoundException))
                {
                    throw ex;
                }
            }
        }

        public IEntryData EntryData { get; }

        public List<Channel> ChannelList { get; }

        private string SetAttCmd(int id, int value)
        {
            Cmd = $"ATT:SHIFt:{id},{value}";
            return Send(Cmd);
        }

        private string SetPhaCmd(int id, int value)
        {
            Cmd = $"PHASe:SHIFt:{id},{value}";
            return Send(Cmd);
        }

        private string SetPhaAndAttCmd(int id, int pha, int att)
        {
            Cmd = $"SETM:{id},{pha},{att}";
            return Send(Cmd);
        }

        private string ReadIDNCmd()
        {
            Cmd = "*IDN?";
            return Send(Cmd);
        }

        internal int CurrentPha(int id)
        {
            Cmd = $"READM:{id}";
            return Send(Cmd).Replace("\r\n", "").Split(',')[1].ToInt32();
        }

        internal int CurrentAtt(int id)
        {
            Cmd = $"READM:{id}";
            return Send(Cmd).Replace("\r\n", "").Split(',')[2].ToInt32();
        }

        internal string ReadIDN()
        {
            return ReadIDNCmd();
        }

        internal void LoadOffsets(string filePath, out List<Channel> channelList)
        {
            try
            {
                channelList = new List<Channel>();

                if (filePath.ToLower().EndsWith(".txt"))
                {
                    var rows = File.ReadAllLines(filePath);
                    for (int r = 0; r < rows.Length; r++)
                    {
                        var columns = rows[r].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int c = 0; c < columns.Length; c++)
                        {
                            channelList.Add(new Channel(r + 1, c + 1) { PhaOffset = columns[c].ToDouble() });
                        }
                    }
                }
                else if (filePath.ToLower().EndsWith(".csv"))
                {
                    var rows = File.ReadAllLines(filePath);
                    for (int r = 0; r < rows.Length; r++)
                    {
                        var columns = rows[r].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int c = 0; c < columns.Length; c++)
                        {
                            channelList.Add(new Channel(r + 1, c + 1) { PhaOffset = columns[c].ToDouble() });
                        }
                    }
                }
                else
                {
                    throw new Exception("文件格式错误！");
                }
            }
            catch (Exception ex)
            {
                channelList = null;
                throw ex;
            }
        }

        internal void SetPhaAndAtt(int id, int pha, int att)
        {
            SetPhaAndAttCmd(id, pha, att);
        }

        internal void SetPhaOffsets()
        {
            throw new NotImplementedException();
        }

        internal void SetPha(List<Channel> channelList, bool isCalibrating)
        {
            try
            {
                if (isCalibrating)
                {
                    foreach (var channel in channelList)
                    {
                        var currentPha = CurrentPha(this[channel.APortID, channel.BPortID]);
                        var offset = (int)Math.Round(channel.PhaOffset / EntryData.PhaseStep);
                        var x = SetPhaCmd(this[channel.APortID, channel.BPortID], (currentPha + offset) % (int)(360 / EntryData.PhaseStep));
                        if ((!x.Contains("OK")))
                        {
                            Log.log.ErrorFormat("Signal Path ID : A{0}B{1} Set Value Error!", channel.APortID, channel.BPortID);
                        }
                    }
                }
                else
                {
                    foreach (var channel in channelList)
                    {
                        var offset = (int)Math.Round(channel.PhaOffset / EntryData.PhaseStep);
                        var x = SetPhaCmd(this[channel.APortID, channel.BPortID], (channel.PhaStdCode + offset) % (int)(360 / EntryData.PhaseStep));
                        if ((!x.Contains("OK")))
                        {
                            Log.log.ErrorFormat("Signal Path ID : A{0}B{1} Set Value Error!", channel.APortID, channel.BPortID);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void ResetAttAndPha()
        {
            try
            {
                for (int b = 1; b <= BPortConnectNum; b++)
                {
                    for (int a = 1; a <= APortNum; a++)
                    {
                        var x = SetPhaAndAttCmd(this[a, b], 0, 0);
                        if (!x.Contains("OK"))
                        {
                            Log.log.ErrorFormat("Signal Path ID : {2} A{0}B{1} Reset Error!", a, b, this[a, b]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        internal void SetAtt(List<Channel> channelList, bool isCalibrating)
        {
            try
            {
                if (isCalibrating)
                {
                    foreach (var channel in channelList)
                    {
                        int currentAtt = CurrentAtt(this[channel.APortID, channel.BPortID]);
                        int offset = (int)(Math.Round(channel.AttOffset / EntryData.AttenuationStep) % 240);
                        var x = SetAttCmd(this[channel.APortID, channel.BPortID], (currentAtt + offset));
                        if (!x.Contains("OK"))
                        {
                            Log.log.ErrorFormat("Signal Path ID : A{0}B{1} Set Attenuation Value Error!", channel.APortID, channel.BPortID);
                        }
                    }
                }
                else
                {
                    foreach (var channel in channelList)
                    {
                        int offset = (int)(Math.Round(channel.AttOffset / EntryData.AttenuationStep) % 240);
                        var x = SetAttCmd(this[channel.APortID, channel.BPortID], (channel.AttStdCode + offset));
                        if (!x.Contains("OK"))
                        {
                            Log.log.ErrorFormat("Signal Path ID : A{0}B{1} Set Attenuation Value Error!", channel.APortID, channel.BPortID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void AddFrameInfo(string frameInfo)
        {
            string result = AddFrameInfoCmd(frameInfo);
            if (!result.Contains("OK"))
            {
                Log.log.ErrorFormat("新增场景文件错误。" + frameInfo);
            }
        }

        private string AddFrameInfoCmd(string frameInfo)
        {
            Cmd = $"SCENe:ADD:{frameInfo}";
            return Send(Cmd);
        }

        internal void CheckDB(string checkInfo)
        {
            string result = CheckDBCmd(checkInfo);
            if (!result.Contains("OK"))
            {
                Log.log.ErrorFormat("校验数据库错误。" + checkInfo);
            }
        }

        private string CheckDBCmd(string checkInfo)
        {
            Cmd = $"SCENe:VERIfy:{checkInfo}";
            return Send(Cmd);
        }

        internal void DownLoadDBToFirmware(string downLoadInfo)
        {
            string result = DownLoadDBToFirmwareCmd(downLoadInfo);
            if (!result.Contains("START"))
            {
                Log.log.ErrorFormat("向固件下载数据库错误。" + downLoadInfo);
            }
        }

        private string DownLoadDBToFirmwareCmd(string downLoadInfo)
        {
            Cmd = $"SCENe:DOWNload:{downLoadInfo}";
            return Send(Cmd);
        }

        internal void PlayScene(string playInfo)
        {
            string result = PlaySceneCmd(playInfo);
            if (!result.Contains("START"))
            {
                Log.log.ErrorFormat("播放场景错误。" + playInfo);
            }
        }

        private string PlaySceneCmd(string playInfo)
        {
            Cmd = $"SCENe:PLAY:{playInfo}";
            return Send(Cmd);
        }
    }

    public class Vertex : Device
    {
        internal void OpenChannel(int inPortID, int outPortID, UpDown linkUpDown)
        {
            try
            {
                switch (linkUpDown)
                {
                    case UpDown.UP:
                        Cmd = $"SYST:RLINK:BA{inPortID}{outPortID}:STATe ON";
                        break;
                    case UpDown.DOWN:
                        Cmd = $"SYST:RLINK:AB{inPortID}{outPortID}:STATe ON";
                        break;
                    default:
                        Cmd = "";
                        break;
                }
                Send(Cmd).WaitCompleted(Send, Cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Vertex(string ip, int portNum) : base(ip, portNum)
        {

        }

        internal void CloseChannel(int inPortID, int outPortID, UpDown linkUpDown)
        {
            try
            {
                switch (linkUpDown)
                {
                    case UpDown.UP:
                        Cmd = $"SYST:RLINK:BA{inPortID}{outPortID}:STATe OFF";
                        break;
                    case UpDown.DOWN:
                        Cmd = $"SYST:RLINK:AB{inPortID}{outPortID}:STATe OFF";
                        break;
                    default:
                        Cmd = "";
                        break;
                }
                Send(Cmd).WaitCompleted(Send, Cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        internal void CloseAllChannel(int aPortNum, int bPortNum)
        {
            try
            {
                for (int a = 1; a <= aPortNum; a++)
                {
                    for (int b = 1; b <= bPortNum; b++)
                    {
                        Cmd = $"SYST:RLINK:BA{b}{a}:STATe OFF";
                        Send(Cmd).WaitCompleted(Send, Cmd);
                        Cmd = $"SYST:RLINK:AB{a}{b}:STATe OFF";
                        Send(Cmd).WaitCompleted(Send, Cmd);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void ReSet()
        {
            try
            {
                Cmd = "*RST";
                Send(Cmd).WaitCompleted(Send, Cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void SetAtt()
        {
            throw new NotImplementedException();
        }

        internal void SetPha(String index, double upLinkValue, double downLinkValue)
        {
            try
            {
                var port = index.Split(':');
                Cmd = $"RLINK:BA{port[1]}{port[0]}:PHAse {upLinkValue}";
                Send(Cmd).WaitCompleted(Send, Cmd);
                Cmd = $"RLINK:AB{port[0]}{port[1]}:PHAse {downLinkValue}";
                Send(Cmd).WaitCompleted(Send, Cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        internal void SelfCalibrate()
        {
            Cmd = "CONnection:IPCAL:BEGin";
            Send(Cmd);
        }

        internal bool IsSelfCalibrateComplete()
        {
            Cmd = "CONnection:IPCAL:STATus?";
            return Send(Cmd).Contains("Completed");
        }
    }

    public abstract class CalBox : Device
    {
        public CalBox(string ip, int portNum, IEntryData data) : base(ip, portNum)
        {
            _data = data;
        }

        protected IEntryData _data;

        public CalBoxData CalBoxData { get; set; }

        public void SetSwitch(int portNumID)
        {
            var sw1 = (portNumID / 8) + 1;
            var swx = portNumID % 8;
            if (swx == 0)
            {
                swx = 8;
            }
            if (Connected)
            {
                CutSwitch(1, sw1);
                CutSwitch(sw1, swx);
            }
            else
            {
                throw new Exception("错误：开关盒未连接！");
            }
        }

        private string CutSwitch(int switchID, int pin)
        {
            //string[] strMsg = new string[1];
            //strMsg[0] = "SETSW:" + switchID.ToString() + ":" + pin.ToString();
            //SendCommand((int)UserCmd.SET, strMsg);
            ////Thread.Sleep(200);
            //if (_setValueInterval < 0)
            //{
            //    if (WaitCmdResponse((int)UserCmd.SET, 1000))
            //    {
            //        return CommonDataArr[(int)UserCmd.SET].GetData();
            //    }
            //    else
            //    {
            //        return "";
            //    }
            //}
            //else
            //{
            //    Thread.Sleep(_setValueInterval);
            return "";
            //}
        }

        internal string RouteChangeTo(int v1, int v2)
        {
            Cmd = $"SET:{v1}:{v2}";
            return Send(Cmd);
        }

        internal string GetCalBoxDataCmd(int frequency)
        {
            Cmd = $"READcb:{frequency}";
            return Send(Cmd);
        }

        public void GetCalBoxData()
        {
            CalBoxData = new CalBoxData();
            string result = GetCalBoxDataCmd((int)_data.Frequency * 1000);
            result.Replace("\r\n", "");
            string[] calBoxVal = result.Split(':')[2].Split(';');
            for (int n = 1; n <= calBoxVal.Length; n++)
            {
                if (n >= 1 && n <= 64)
                {
                    if (calBoxVal[n - 1].Contains(","))
                    {
                        CalBoxData.APortDataList.Add(new PortData()
                        {
                            Phase = calBoxVal[n - 1].Split(',')[0].ToDouble(),
                            Attenuation = calBoxVal[n - 1].Split(',')[1].ToDouble()
                        });
                    }
                    else
                    {
                        CalBoxData.APortDataList.Add(new PortData()
                        {
                            Phase = Convert.ToDouble(calBoxVal[n - 1]),
                            Attenuation = 0
                        });
                    }
                }
                else if (n >= 65 && n <= 80)
                {
                    if (calBoxVal[n - 1].Contains(","))
                    {
                        CalBoxData.BPortDataList.Add(new PortData()
                        {
                            Phase = calBoxVal[n - 1].Split(',')[0].ToDouble(),
                            Attenuation = calBoxVal[n - 1].Split(',')[1].ToDouble()
                        });
                    }
                    else
                    {
                        CalBoxData.BPortDataList.Add(new PortData()
                        {
                            Phase = Convert.ToDouble(calBoxVal[n - 1]),
                            Attenuation = 0
                        });
                    }
                }
            }
        }
        //result.TrimEnd(new char[] { '\'});
        //Random r = new Random();
        //for (int n = 1; n <= 80; n++)
        //{
        //    if (n >= 1 && n <= 64)
        //    {
        //        calBoxData.LeftPortDatas.Add(new PortData()
        //        {
        //            PortID = n,
        //            Phase = r.Next(-180, 180),
        //            Attenuation = -1
        //        });
        //    }
        //    else if (n >= 65 && n <= 80)
        //    {
        //        calBoxData.RightPortDatas.Add(new PortData()
        //        {
        //            PortID = n,
        //            Phase = r.Next(-180, 180),
        //            Attenuation = -1
        //        });
        //    }
        //}
        //return calBoxData;

        //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en", true);
        //calBoxToMatrix.ReadCB(out string result);
        //string result = File.ReadAllText("1.txt");
        //result.Replace("\r\n", "");
        #region MyRegion
        //if (result != "FAIL" && result != "")
        //{
        //    if (result.Contains(','))
        //    {
        //        //CalBoxIsOld = false;
        //        string[] calBoxVal = result.Split(';');
        //        for (int n = 1; n <= calBoxVal.Length; n++)
        //        {
        //            //calBoxData.Add(new CalBoxData
        //            //{
        //            //    Index = index,
        //            //    Phase = Convert.ToDouble(calBoxVal[i]),
        //            //    Attenuation = -1
        //            //});
        //            if (n >= 1 && n <= 16)
        //            {
        //                CalBoxData.APortsDatas.Add(new PortData()
        //                {
        //                    Phase = Convert.ToDouble(calBoxVal[n - 1].Split(',')[2]),
        //                    Attenuation = -1
        //                });
        //            }
        //            else if (n >= 17 && n <= 24)
        //            {
        //                CalBoxData.BPortsDatas.Add(new PortData()
        //                {
        //                    Phase = Convert.ToDouble(calBoxVal[n - 1].Split(',')[2]),
        //                    Attenuation = -1
        //                });
        //            }
        //        }
        //        //for (int i = 0; i < calBoxVal.Length; i++)
        //        //{
        //        //calBoxData.Add(new CalBoxData
        //        //{
        //        //    Index = index,
        //        //    Phase = Convert.ToDouble(calBoxVal[i].Split(',')[0]),
        //        //    Attenuation = -1
        //        //});
        //        //_calBoxData[i + portabNum].Data = Convert.ToDouble(calBoxVal[i].Split(',')[1]);
        //        //}
        //    }
        //    else
        //    {
        //        //CalBoxIsOld = true;
        //        string[] calBoxVal = result.Split(';');
        //        for (int n = 1; n <= calBoxVal.Length; n++)
        //        {
        //            //calBoxData.Add(new CalBoxData
        //            //{
        //            //    Index = index,
        //            //    Phase = Convert.ToDouble(calBoxVal[i]),
        //            //    Attenuation = -1
        //            //});
        //            if (n >= 1 && n <= 64)
        //            {
        //                CalBoxData.APortsDatas.Add(new PortData()
        //                {
        //                    Phase = Convert.ToDouble(calBoxVal[n]),
        //                    Attenuation = -1
        //                });
        //            }
        //            else if (n >= 65 && n <= 80)
        //            {
        //                CalBoxData.BPortsDatas.Add(new PortData()
        //                {
        //                    Phase = Convert.ToDouble(calBoxVal[n]),
        //                    Attenuation = -1
        //                });
        //            }
        //        }
        //    }
        //}
        #endregion
    }

    public class CalBoxToMatrix : CalBox
    {
        public CalBoxToMatrix(string ip, int portNum, IEntryData data) : base(ip, portNum, data)
        {

        }

        public void Set64B16Switch(int a, int b)
        {
            if (!RouteChangeTo(1, Convert.ToInt32(a)).Contains("OK"))
            {
                MessageBox.Show("失败");
            }
            if (!RouteChangeTo(2, Convert.ToInt32(b)).Contains("OK"))
            {
                MessageBox.Show("失败");
            }
        }

        //internal void Set64B16Switch(int aPortID, int cPortID, int v1, int v2)
        //{
        //    throw new NotImplementedException();
        //}

        public void Set64B16Switch(int portanum, int portbnum, int switchD, int switchB)
        {
            //此处为什么强行切  开关9，和，10  ==》开关示意图如下
            /*
            ---
            |1|
            ---                                            ----
            ---                                            |11|
            |2|                                            ----
            ---
            ---                   -----       ----
            |3|                   | 9 |       |10|
            ---                   -----       ----
             :
             :                                             ----
            ---                                            |12|
            |8|                                            ----
            ---

             */
            #region 选择单刀8开关
            if (portanum > 8 && portanum <= 16)
            {
                switchD = 2;
            }
            if (portanum > 16 && portanum <= 24)
            {
                switchD = 3;
            }
            if (portanum > 24 && portanum <= 32)
            {
                switchD = 4;
            }
            if (portanum > 32 && portanum <= 40)
            {
                switchD = 5;
            }
            if (portanum > 40 && portanum <= 48)
            {
                switchD = 6;
            }
            if (portanum > 48 && portanum <= 56)
            {
                switchD = 7;
            }
            if (portanum > 56 && portanum <= 64)
            {
                switchD = 8;
            }
            if (portbnum > 8)
            {
                switchB = 2;
            }
            #endregion 选择单刀8开关
            #region 切单刀8开关
            if (switchD == 1)
            {
                if (!RouteChangeTo(switchD, Convert.ToInt32(portanum)).Contains("OK"))
                {
                    MessageBox.Show("Set Value Failed!");
                    return;
                }
                //两个八选一开关选择
            }
            if (switchD == 2)
            {
                //RouteChangeTo(switchD, Convert.ToInt32(portanum - 8));
                if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 8)).Contains("OK"))
                {
                    MessageBox.Show("Set Value Failed!");
                    return;
                }
                //两个八选一开关选择
            }
            if (switchD == 3)
            {
                //RouteChangeTo(switchD, Convert.ToInt32(portanum - 16));
                if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 16)).Contains("OK"))
                {
                    MessageBox.Show("Set Value Failed!");
                    return;
                }
                //两个八选一开关选择
            }
            if (switchD == 4)
            {
                //RouteChangeTo(switchD, Convert.ToInt32(portanum - 24));
                if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 24)).Contains("OK"))
                {
                    MessageBox.Show("Set Value Failed!");
                    return;
                }
                //两个八选一开关选择
            }
            if (switchD == 5)
            {
                //RouteChangeTo(switchD, Convert.ToInt32(portanum - 32));
                if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 32)).Contains("OK"))
                {
                    if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 32)).Contains("OK"))
                    {
                        if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 32)).Contains("OK"))
                        {
                            MessageBox.Show("Set Value Failed!");
                            return;
                        }
                    }
                }
                //两个八选一开关选择
            }
            if (switchD == 6)
            {
                //RouteChangeTo(switchD, Convert.ToInt32(portanum - 40));
                if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 40)).Contains("OK"))
                {
                    if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 40)).Contains("OK"))
                    {
                        if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 40)).Contains("OK"))
                        {
                            MessageBox.Show("Set Value Failed!");
                            return;
                        }
                    }
                }
                //两个八选一开关选择
            }
            if (switchD == 7)
            {
                //RouteChangeTo(switchD, Convert.ToInt32(portanum - 48));
                if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 48)).Contains("OK"))
                {
                    if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 48)).Contains("OK"))
                    {
                        if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 48)).Contains("OK"))
                        {
                            MessageBox.Show("Set Value Failed!");
                            return;
                        }
                    }
                }
                //两个八选一开关选择
            }
            if (switchD == 8)
            {
                //RouteChangeTo(switchD, Convert.ToInt32(portanum - 56));
                if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 56)).Contains("OK"))
                {
                    if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 56)).Contains("OK"))
                    {
                        if (!RouteChangeTo(switchD, Convert.ToInt32(portanum - 56)).Contains("OK"))
                        {
                            MessageBox.Show("Set Value Failed!");
                            return;
                        }
                    }
                }
                //两个八选一开关选择
            }
            #endregion 切单刀8开关
            if (!RouteChangeTo(9, Convert.ToInt32(switchD)).Contains("OK"))
            {
                if (!RouteChangeTo(9, Convert.ToInt32(switchD)).Contains("OK"))
                {
                    if (!RouteChangeTo(9, Convert.ToInt32(switchD)).Contains("OK"))
                    {
                        MessageBox.Show("Set Value Failed!");
                        return;
                    }
                }
            }
            if (!RouteChangeTo(10, Convert.ToInt32(switchB)).Contains("OK"))
            {
                if (!RouteChangeTo(10, Convert.ToInt32(switchB)).Contains("OK"))
                {
                    if (!RouteChangeTo(10, Convert.ToInt32(switchB)).Contains("OK"))
                    {
                        MessageBox.Show("Set Value Failed!");
                        return;
                    }
                }
            }
            if (switchB == 1)
            {
                if (!RouteChangeTo(11, Convert.ToInt32(portbnum)).Contains("OK"))
                {
                    if (!RouteChangeTo(11, Convert.ToInt32(portbnum)).Contains("OK"))
                    {
                        if (!RouteChangeTo(11, Convert.ToInt32(portbnum)).Contains("OK"))
                        {
                            MessageBox.Show("Set Value Failed!");
                            return;
                        }
                    }
                }
            }
            else
            {
                if (!RouteChangeTo(12, Convert.ToInt32(portbnum - 8)).Contains("OK"))
                {
                    if (!RouteChangeTo(12, Convert.ToInt32(portbnum - 8)).Contains("OK"))
                    {
                        if (!RouteChangeTo(12, Convert.ToInt32(portbnum - 8)).Contains("OK"))
                        {
                            MessageBox.Show("Set Value Failed!");
                            return;
                        }
                    }
                }
            }
        }
    }

    public class CalBoxToVertex : CalBox
    {
        public CalBoxToVertex(string ip, int portNum, IEntryData data) : base(ip, portNum, data)
        {

        }
    }
}