using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace System.Matrix
{
    public class Matrix : Device
    {
        public Matrix(DeviceData deviceData) : base(deviceData)
        {
            try
            {
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

        public List<Channel> ChannelList { get; }

        public double AttenuationStep { get => _deviceData.AttenuationStep; }
        public double PhaseStep { get => _deviceData.PhaseStep; }
        public int AttCalFre { get => _deviceData.AttCalFre; }
        public int PhaCalFre { get => _deviceData.PhaCalFre; }
        public PhaseStepShiftDirection PhaseStepShiftDirection { get => _deviceData.PhaseStepShiftDirection; }

        private string SetAttCmd(int id, int value)
        {
            _cmd = $"ATT:SHIFt:{id},{value}";
            return Send(_cmd);
        }

        private string SetPhaCmd(int id, int value)
        {
            _cmd = $"PHASe:SHIFt:{id},{value}";
            return Send(_cmd);
        }

        private string SetPhaAndAttCmd(int id, int pha, int att)
        {
            _cmd = $"SETM:{id},{pha},{att}";
            return Send(_cmd);
        }

        private string ReadIDNCmd()
        {
            _cmd = "*IDN?";
            return Send(_cmd);
        }

        internal int CurrentPha(int id)
        {
            _cmd = $"READM:{id}";
            return Send(_cmd).Replace("\r\n", "").Split(',')[1].ToInt32();
        }

        internal int CurrentAtt(int id)
        {
            _cmd = $"READM:{id}";
            return Send(_cmd).Replace("\r\n", "").Split(',')[2].ToInt32();
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

                if (Path.GetExtension(filePath).Equals(".txt"))
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
                else if (Path.GetExtension(filePath).Equals(".csv"))
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
                        var offset = (int)Math.Round(channel.PhaOffset / _deviceData.PhaseStep);
                        var x = SetPhaCmd(this[channel.APortID, channel.BPortID], (currentPha + offset) % (int)(360 / _deviceData.PhaseStep));
                        if ((!x.ToUpper().Contains("OK")))
                        {
                            Log.log.ErrorFormat("Signal Path ID : A{0}B{1} Set Value Error!", channel.APortID, channel.BPortID);
                        }
                    }
                }
                else
                {
                    foreach (var channel in channelList)
                    {
                        var offset = (int)Math.Round(channel.PhaOffset / _deviceData.PhaseStep);
                        var x = SetPhaCmd(this[channel.APortID, channel.BPortID], (channel.PhaStdCode + offset) % (int)(360 / _deviceData.PhaseStep));
                        if ((!x.ToUpper().Contains("OK")))
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
                        int offset = (int)(Math.Round(channel.AttOffset / _deviceData.AttenuationStep) % 240);
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
                        int offset = (int)(Math.Round(channel.AttOffset / _deviceData.AttenuationStep) % 240);
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
            _cmd = $"SCENe:ADD:{frameInfo}";
            return Send(_cmd);
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
            _cmd = $"SCENe:VERIfy:{checkInfo}";
            return Send(_cmd);
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
            _cmd = $"SCENe:DOWNload:{downLoadInfo}";
            return Send(_cmd);
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
            _cmd = $"SCENe:PLAY:{playInfo}";
            return Send(_cmd);
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
                        _cmd = $"SYST:RLINK:BA{inPortID}{outPortID}:STATe ON";
                        break;
                    case UpDown.DOWN:
                        _cmd = $"SYST:RLINK:AB{inPortID}{outPortID}:STATe ON";
                        break;
                    default:
                        _cmd = "";
                        break;
                }
                Send(_cmd).WaitCompleted(Send, _cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Vertex(DeviceData deviceData) : base(deviceData)
        {

        }

        internal void CloseChannel(int inPortID, int outPortID, UpDown linkUpDown)
        {
            try
            {
                switch (linkUpDown)
                {
                    case UpDown.UP:
                        _cmd = $"SYST:RLINK:BA{inPortID}{outPortID}:STATe OFF";
                        break;
                    case UpDown.DOWN:
                        _cmd = $"SYST:RLINK:AB{inPortID}{outPortID}:STATe OFF";
                        break;
                    default:
                        _cmd = "";
                        break;
                }
                Send(_cmd).WaitCompleted(Send, _cmd);
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
                        _cmd = $"SYST:RLINK:BA{b}{a}:STATe OFF";
                        Send(_cmd).WaitCompleted(Send, _cmd);
                        _cmd = $"SYST:RLINK:AB{a}{b}:STATe OFF";
                        Send(_cmd).WaitCompleted(Send, _cmd);
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
                _cmd = "*RST";
                Send(_cmd).WaitCompleted(Send, _cmd);
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
                _cmd = $"RLINK:BA{port[1]}{port[0]}:PHAse {upLinkValue}";
                Send(_cmd).WaitCompleted(Send, _cmd);
                _cmd = $"RLINK:AB{port[0]}{port[1]}:PHAse {downLinkValue}";
                Send(_cmd).WaitCompleted(Send, _cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        internal void SelfCalibrate()
        {
            _cmd = "CONnection:IPCAL:BEGin";
            Send(_cmd);
        }

        internal bool IsSelfCalibrateComplete()
        {
            _cmd = "CONnection:IPCAL:STATus?";
            return Send(_cmd).Contains("Completed");
        }
    }

    public abstract class CalBox : Device, ISwitch
    {
        public CalBox(DeviceData deviceData) : base(deviceData)
        {
            CalBoxData = new CalBoxData();
        }

        public CalBoxData CalBoxData { get; set; }

        protected void SetSwitch(int portNumID)
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
            _cmd = "SETSW:" + switchID.ToString() + ":" + pin.ToString();
            return "";
        }

        internal string RouteChangeTo(int v1, int v2)
        {
            _cmd = $"SET:{v1}:{v2}";
            return Send(_cmd);
        }

        internal string GetCalBoxDataCmd(int frequency)
        {
            _cmd = $"READcb:{frequency}";
            return Send(_cmd);
        }

        public virtual void DoSwitch(int aPort, int bPort)
        {
            throw new NotImplementedException();
        }

        public virtual void GetCalBoxData()
        {
            throw new NotImplementedException();
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
    }

    public class CalBoxWhole : CalBox
    {
        public CalBoxWhole(DeviceData deviceData) : base(deviceData)
        {
        }

        public override void DoSwitch(int aPort, int bPort)
        {
            Set64B16Switch(aPort, bPort, 1, 1);
        }

        public override void GetCalBoxData()
        {
            string result = GetCalBoxDataCmd((int)_deviceData.Frequency * 1000);
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
            //return CalBoxData;
        }

        private void Set64B16Switch(int portanum, int portbnum, int switchD, int switchB)
        {
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

    public class CalBoxToMatrix : CalBox
    {
        public CalBoxToMatrix(DeviceData deviceData) : base(deviceData)
        {
        }

        public override void DoSwitch(int aPort, int bPort)
        {
            SetSwitch(aPort);
        }

        public override void GetCalBoxData()
        {
            string result = GetCalBoxDataCmd((int)_deviceData.Frequency * 1000);
            result.Replace("\r\n", "");
            string[] calBoxVal = result.Split(':')[2].Split(';');

            CalBoxData.APortDataList.Add(new PortData
            {
                Attenuation = -1,
                Phase = -1
            });

            //return CalBoxData;
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
    }

    public class CalBoxToVertex : CalBox
    {
        public CalBoxToVertex(DeviceData deviceData) : base(deviceData)
        {
        }

        public override void DoSwitch(int aPort, int bPort)
        {
            SetSwitch(bPort);
        }

        public override void GetCalBoxData()
        {
            string result = GetCalBoxDataCmd((int)_deviceData.Frequency * 1000);
            result.Replace("\r\n", "");
            string[] calBoxVal = result.Split(':')[2].Split(';');

            CalBoxData.BPortDataList.Add(new PortData
            {
                Attenuation = -2,
                Phase = -2
            });

            //return CalBoxData;
        }
    }
}
