using Ivi.Visa.Interop;
using System.Threading;

namespace System.Matrix
{
    public class VNAE5061B : VNA, IVectorNetworkAnalyzer
    {
        //定义仪表连接标示符
        //private MessageBasedSession messageBased;
        //private NationalInstruments.VisaNS.MessageBasedSession messageBased = null;
        //private FormattedIO488 messageBased;
        ////定义读取的长度
        //private int intReadLength;

        public VNAE5061B(DeviceData deviceData) : base(deviceData)
        {
        }

        public enum SNPFormat
        {
            DB,
            RI
        }

        public enum Trigger
        {
            HOLD,
            CONTINUOUS
        }

        public override bool Connected { get; set; }

        public int PhaMarkPoint => _deviceData.PhaMarkPoint;

        public int AttMarkPoint => _deviceData.AttMarkPoint;

        /// <summary>
        /// 连接仪表
        /// </summary>
        /// <param name="IP"></param>
        public override void Connect()
        {
            try
            {
                #region Socket 连接方式
                messageBased = new FormattedIO488();
                ResourceManager grm = new ResourceManager();
                messageBased.IO = (IMessage)grm.Open("TCPIP0::" + IP + "::5025::SOCKET", AccessMode.NO_LOCK, 2000, "");
                messageBased.IO.Timeout = 200000;
                messageBased.IO.SendEndEnabled = !messageBased.IO.SendEndEnabled;
                messageBased.IO.TerminationCharacterEnabled = !messageBased.IO.TerminationCharacterEnabled;
                if (ReadIDN(out string IDN))
                {
                    Connected = true;
                }
                else
                {
                    Connected = false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Connected = false;
                Close();
                Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        #region 设置当前Trace,读取Trace数据

        string[] DoubleArrToStringArr(double[] douRe)
        {
            string[] strRe = null;

            if (douRe != null)
            {
                strRe = new string[douRe.Length];
                for (int i = 0; i < douRe.Length; i++)
                {
                    strRe[i] = douRe[i].ToString();
                }
            }
            return strRe;
        }

        /// <summary>
        /// 读取指定Trace 数据（先选择对应 Trace，再读取）
        /// 调用ReadTraces("Trc1","FDATa")
        /// </summary>
        public string[] ReadTraces(string trace, string format)
        {
            string strCmd = "";
            int netChannel = 1;
            trace = trace.Replace("Trc", "");
            int.TryParse(trace, out int intTrace);
            string[] revValue = null;
            int index = 0;
            try
            {
                Write("INIT1:CONT ON");
                //Set the trigger source to Bus Trigger.
                Write(":TRIG:SOUR BUS");
                //Trigger the instrument to start a sweep cycle.
                Write(":TRIG:SING");
                //Execute the *OPC? command and wait until the command
                QueryString("*OPC?");


                strCmd = ":CALCulate" + netChannel.ToString().Trim() + ":TRACe" + intTrace + ":DATA:" + format + "?";
                //douRe = QueryBinary(strCmd);//messageBased.(strCmd, intReadLength);

                string value = QueryString(strCmd);
                Write("INIT1:CONT OFF");

                string[] arrValue = value.Split(',');
                revValue = new string[arrValue.Length];
                for (int i = 0; i < arrValue.Length; i++)
                {
                    //if (i % 2 == 0)
                    //{
                    revValue[index] = arrValue[i];
                    index++;
                    //}
                }

            }
            catch
            {
            }
            return revValue;
        }

        /// <summary>
        /// 读取指定Trace 数据（先选择对应 Trace，再读取）
        /// 调用ReadTraces("Trc1","FDATa")
        /// </summary>
        public double[] ReadTraces_new(string trace, string format)
        {
            double[] revValue = null;
            try
            {
                DataFormetToBinary();
                SetTrace(trace, format);
                //Turn on or off continuous initiation mode for each channel
                Write("INIT1:CONT ON");
                //Set the trigger source to Bus Trigger.
                Write(":TRIG:SOUR BUS");
                //Trigger the instrument to start a sweep cycle.
                Write(":TRIG:SING");
                //Execute the *OPC? command and wait until the command
                QueryString("*OPC?");
                revValue = QueryBinary(":CALCulate1:TRACe1:DATA:FDATa?");
                // string value = QueryString(":CALCulate1:TRACe1:DATA:FDATa?");
                Write("INIT1:CONT OFF");
            }
            catch
            {
            }
            return revValue;
        }

        /// <summary>
        /// 读取指定Trace 数据（先选择对应 Trace，再读取）
        /// 调用ReadTraces("Trc1","FDATa")
        /// </summary>
        public string ReadTrace(string trace, string format)
        {
            if (messageBased != null)
            {
                string strCmd = "";
                int netChannel = 1;

                trace = trace.Replace("Trc", "");
                int.TryParse(trace, out int intTrace);

                string value = null;
                try
                {

                    strCmd = ":CALCulate" + netChannel.ToString().Trim() + ":TRACe" + intTrace + ":DATA:" + format + "?";
                    value = QueryString(strCmd);
                }
                catch
                {
                }
                return value;
            }
            else
                return null;

        }

        #endregion
        #region 设置Channel的Power
        /// <summary>
        /// 设置Channel1 的Power. 此命令需测试验证.
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="format"></param>
        public void SetPower(string power)
        {
            string strCmd = "";
            int netChannel = 1;
            try
            {
                strCmd = ":SOUR" + netChannel.ToString().Trim() + ":POW " + power.ToString();
                Write(strCmd);
            }
            catch
            {
            }
        }
        #endregion
        #region 读取Channel的Power
        /// <summary>
        /// 读取指定Channel 的Power. 此命令需测试验证.
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="format"></param>
        public string ReadPower(string channel)
        {
            string strRe = "";
            try
            {
                string strCmd = "SOUR1:POW?";
                strRe = QueryString(strCmd);
            }
            catch
            {
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                strRe = "";
            }
            return strRe;
        }
        #endregion
        #region  读取IFBW
        /// <summary>
        /// 读取IFBW
        /// </summary>
        /// <returns></returns>
        public string ReadIFBW(string channel)//string channel
        {
            string strRe = "";
            string strCmd = "";
            int netChannel = 1;
            try
            {
                strCmd = "SENS" + netChannel.ToString().Trim() + ":BAND?";
                strRe = QueryString(strCmd);

            }
            catch
            {
                strRe = "";
            }
            return strRe;
        }
        #endregion
        #region  设置IFBW
        /// <summary>
        /// 设置IFBW
        /// </summary>
        /// <returns></returns>
        public void SetIFBW(int IfbwValue)
        {
            string strCmd = "";
            int netChannel = 1;
            try
            {
                strCmd = "SENS" + netChannel.ToString().Trim() + ":BAND " + IfbwValue;
                Write(strCmd);
                //switch (key)
                //{
                //    case ClassVNASwitchDevice.cmdKey.A:
                //        strCmd = "SENS1:BAND " + IfbwValue;
                //        break;
                //    case ClassVNASwitchDevice.cmdKey.B:
                //        strCmd = "SENS2:BAND " + IfbwValue;
                //        break;
                //}
                //Write(strCmd);
            }
            catch
            {
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion
        #region  设置single sweep mode
        /// <summary>
        /// 设置single sweep mode
        /// </summary>
        /// <returns></returns>
        public string SetSingleSweepMode()
        {
            string strCmd = "";
            int netChannel = 1;

            strCmd = "INIT" + netChannel.ToString().Trim() + ":CONT OFF";

            Write(strCmd);
            //这里实际无返回值,为兼容原有罗德里的函数接口.
            return strCmd;
        }
        /// <summary>
        /// 设置single sweep mode
        /// </summary>
        /// <returns></returns>
        public string SetSingleSweepMode(Trigger trigger)
        {
            string strCmd = "";
            int netChannel = 1;

            switch (trigger)
            {
                case Trigger.HOLD:
                    strCmd = "INIT" + netChannel.ToString().Trim() + ":CONT OFF";
                    break;
                case Trigger.CONTINUOUS:
                    strCmd = "INIT" + netChannel.ToString().Trim() + ":CONT ON";
                    break;
            }

            Write(strCmd);
            //这里实际无返回值,为兼容罗德的函数.
            return strCmd;
        }
        #endregion
        #region 设置复位
        /// <summary>
        /// 设置复位
        /// </summary>
        public void ReSetAnalyzer()
        {
            //:SYST:PRES
            string strCmd = "";
            try
            {
                strCmd = ":SYST:PRES";
                Write(strCmd);
            }
            catch
            {
            }
        }
        #endregion

        /// <summary>
        /// 设置扫频点数
        /// </summary>
        /// <returns></returns>
        public void SetSegmentPoint(int Points)
        {
            string strCmd = "";
            int netChannel = 1;
            try
            {

                strCmd = "SENS" + netChannel.ToString().Trim() + ":SWE:POIN " + Points.ToString();
                Write(strCmd);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 获取仪表支持的最小的频点,单位转换为MHz
        /// </summary>
        /// <returns>单位为MHz</returns>
        public double GetFreqMin()
        {
            string strRe = "";
            string strCmd = "";
            try
            {
                strCmd = ":SERV:SWE:FREQ:MIN?";
                strRe = QueryString(strCmd);

            }
            catch
            {
                strRe = "";
            }
            if (double.TryParse(strRe, out double Freq))
            {
                Freq = Freq / 1000000;
                return Freq;
            }
            else
                return double.NaN;
        }
        /// <summary>
        /// 获取仪表支持的最大的频点,单位转换为MHz
        /// </summary>
        /// <returns>单位为MHz</returns>
        public double GetFreqMax()
        {
            string strRe = "";
            string strCmd = "";
            try
            {
                strCmd = ":SERV:SWE:FREQ:MAX?";
                strRe = QueryString(strCmd);
            }
            catch
            {
                strRe = "";
            }
            if (double.TryParse(strRe, out double Freq))
            {
                Freq = Freq / 1000000;
                return Freq;
            }
            else
                return double.NaN;
        }

        /// <summary>
        /// 读取仪表告警
        /// </summary>
        /// <returns></returns>
        private string ReadSystemErr()
        {
            string strRe = "";
            try
            {
                string strCmd = "SYST:ERR?";
                strRe = QueryString(strCmd);
            }
            catch
            {
                strRe = "";
            }
            return strRe;
        }
        /// <summary>
        /// 清空仪表告警
        /// </summary>
        /// <returns></returns>
        private bool ClearSystemErr()
        {
            bool boolRe = true;
            try
            {
                string strCmd = "*CLS";
                Write(strCmd);
            }
            catch
            {
                boolRe = false;
            }
            return boolRe;
        }

        /// <summary>
        /// 加载文件
        /// </summary>
        /// <param name="FilePath"></param>
        public void LoadFile(string FilePath)
        {
            try
            {
                if (!ClearSystemErr())
                    ClearSystemErr();
                string strCmd = "MMEM:LOAD \"" + FilePath + "\"";
                Write(strCmd);
            }
            catch
            {
            }
        }
        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="FilePath"></param>
        public void StoreFile(string FilePath)
        {
            if (messageBased != null)
            {
                string strCmd = "MMEM:STOR \"" + FilePath + "\"";
                Write(strCmd);
            }
        }

        /// <summary>
        /// 设置Trace数量
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="traceNum"></param>
        public void SetTraceNumber(int channel, int traceNum)
        {
            try
            {
                string strCmd = "CALC" + channel + ":PAR:COUN " + traceNum;
                Write(strCmd);
            }
            catch
            {

            }
        }


        /// <summary>
        /// 新增Trace 以及绑定的 sParameter
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="format"></param>
        public void SetTrace(string trace, string sParameter)
        {
            trace = trace.Replace("Trc", "");
            string strCmd = "CALC1:PAR" + trace + ":DEF " + sParameter;
            Write(strCmd);

        }
        /// <summary>
        /// 读取激励值(频点)
        /// </summary>
        public string[] ReadStimulus()
        {
            string strCmd = "";
            string[] revValue = null;
            int index = 0;

            try
            {
                strCmd = "SENS1:FREQ:DATA?";
                string value = QueryString(strCmd);

                string[] arrValue = value.Split(',');
                revValue = new string[arrValue.Length];
                for (int i = 0; i < arrValue.Length; i++)
                {
                    revValue[index] = arrValue[i];
                    index++;
                }
            }
            catch (Exception ex)
            {
                Windows.Forms.MessageBox.Show(ex.ToString());
            }
            return revValue;
        }

        /// <summary>
        /// 数据格式改为二进制
        /// </summary>
        public void DataFormetToBinary()
        {
            string strCmd = "";
            strCmd = ":FORM:DATA REAL";
            Write(strCmd);
        }
        /// <summary>
        /// 读取频率
        /// </summary>
        /// <returns></returns>
        public double[] ReadFrq()//SENS{1-4}:FREQ:DATA?
        {
            double[] strRe = null;
            string strCmd = "";
            int netChannel = 1;

            try
            {
                strCmd = "SENS" + netChannel.ToString().Trim() + ":FREQ:DATA?";
                strRe = QueryBinary(strCmd);
            }
            catch (Exception ex)
            {
                Windows.Forms.MessageBox.Show(ex.ToString());
                strRe = null;
            }
            return strRe;
        }

        /// <summary>
        /// 设置起始频率 SetStartFreq(100MHz)
        /// </summary>
        /// <param name="freq"></param>
        public void SetStartFreq(string freq)
        {
            string strCmd = "";
            int netChannel = 1;
            #region 单位转换
            decimal StartFrequency = 0.0m;
            freq = freq.ToUpper();
            if (freq.Contains("MHZ"))
            {
                freq = freq.Replace("MHZ", "");
                decimal.TryParse(freq, out StartFrequency);
            }
            else if (freq.Contains("GHZ"))
            {
                freq = freq.Replace("GHZ", "");
                if (decimal.TryParse(freq, out StartFrequency))
                {
                    StartFrequency = StartFrequency * 1000;//GHz转换为MHz
                }
            }
            #endregion
            try
            {
                strCmd = "SENS" + netChannel.ToString().Trim() + ":FREQ:STAR " + (StartFrequency * 1000000);//转换为Hz
                Write(strCmd);
                Thread.Sleep(50);
            }
            catch
            {
                Connected = false;
            }
        }


        /// <summary>
        /// 设置终止频率 SetStopFreq(100MHz)
        /// </summary>
        /// <param name="freq"></param>
        public void SetStopFreq(string freq)
        {
            string strCmd = "";
            int netChannel = 1;
            #region 单位转换
            decimal StartFrequency = 0.0m;
            freq = freq.ToUpper();
            if (freq.Contains("MHZ"))
            {
                freq = freq.Replace("MHZ", "");
                decimal.TryParse(freq, out StartFrequency);
            }
            else if (freq.Contains("GHZ"))
            {
                freq = freq.Replace("GHZ", "");
                if (decimal.TryParse(freq, out StartFrequency))
                {
                    StartFrequency = StartFrequency * 1000;//GHz转换为MHz
                }
            }
            #endregion
            try
            {
                strCmd = "SENS" + netChannel.ToString().Trim() + ":FREQ:STOP " + (StartFrequency * 1000000);//转换为Hz
                Write(strCmd);
                Thread.Sleep(50);

            }
            catch
            {
                Connected = false;
            }
        }

        #region 设置模式
        public void SelectFormat(string format)
        {
            /*
            "MLOGarithmic": Specifies the log magnitude format.
            "PHASe": Specifies the phase format.
            "GDELay": Specifies the group delay format.
            "SLINear": Specifies the Smith chart format (Lin/Phase).
            "SLOGarithmic": Specifies the Smith chart format (Log/Phase).
            "SCOMplex": Specifies the Smith chart format (Re/Im).
            "SMITh": Specifies the Smith chart format (R+jX).
            "SADMittance": Specifies the Smith chart format (G+jB).
            "PLINear": Specifies the polar format (Lin/Phase).
            "PLOGarithmic": Specifies the polar format (Log/Phase).
            "POLar": Specifies the polar format (Re/Im).
            "MLINear": Specifies the linear magnitude format.
            "SWR": Specifies the SWR format.
            "REAL": Specifies the real format.
            "IMAGinary": Specifies the imaginary format.
            "UPHase": Specifies the expanded phase format.
            "PPHase": Specifies the positive phase format.
             */
            string strCmd = "";
            try
            {
                strCmd = "CALC1:FORM " + format.ToUpper();
                Write(strCmd);
                Thread.Sleep(50);

            }
            catch
            {
                Connected = false;
            }
        }
        #endregion

        #region 设置marker点显示
        public void SetMarkerState(bool display)
        {
            string strCmd = "";
            string state = display ? "ON" : "OFF";
            try
            {
                strCmd = "CALC1:MARK1 " + state;
                Write(strCmd);
                Thread.Sleep(50);
            }
            catch
            {
                Connected = false;
            }
        }
        #endregion

        #region 设置marker点激活
        public void SetMarkerActive()
        {
            //激活后自动display marker点
            string strCmd = "";
            try
            {
                strCmd = "CALC1:MARK1:ACT";
                Write(strCmd);
                Thread.Sleep(50);
            }
            catch
            {
                Connected = false;
            }
        }
        #endregion

        #region 设置marker横坐标
        public void SetMarkerX(int trace, long x)
        {
            string strCmd = "";
            try
            {
                strCmd = "CALC1:MARK1:X " + x.ToString();
                Write(strCmd);
                Thread.Sleep(50);
            }
            catch
            {
                Connected = false;
            }
        }
        #endregion

        #region 设置marker横坐标
        public void SetMarkerX(long x)
        {
            string strCmd = "";
            try
            {
                strCmd = "CALC1:MARK1:X " + x.ToString();
                Write(strCmd);
                Thread.Sleep(50);
            }
            catch
            {
                Connected = false;
            }
        }
        #endregion

        #region 读取marker横坐标
        public double GetMarkerY(int trace)
        {
            string strCmd = "";
            string sY;
            double dY;
            try
            {
                Write("INIT1:CONT ON");
                //Set the trigger source to Bus Trigger.
                Write(":TRIG:SOUR BUS");
                //Trigger the instrument to start a sweep cycle.
                Write(":TRIG:SING");
                //Execute the *OPC? command and wait until the command
                QueryString("*OPC?");
                //strCmd = "CALC1:MARK1:Y?";
                strCmd = string.Format("CALCulate1:TRACe{0}" + ":MARKer1" + ":Y? ", trace);
                sY = QueryString(strCmd);
                dY = Convert.ToDouble(sY.Split(',')[0]);
                return dY;
            }
            catch
            {
                Connected = false;
            }
            return 0;
        }
        #endregion

        public void SetSegmentFreqIns(string StartFreq, string StopFreq, int Points, string Power, string SegmentTime, string Unused, string MeasBandwidth)
        {
            throw new NotImplementedException();
        }

        public void ActiveSegmentFreq()
        {
            throw new NotImplementedException();
        }

        public void SetAGC_MANual()
        {
            throw new NotImplementedException();
        }

        public void SetAGC_Auto()
        {
            throw new NotImplementedException();
        }

        public void SetAGC_LNO()
        {
            throw new NotImplementedException();
        }

        public double GetMarkerY()
        {
            throw new NotImplementedException();
        }

        public double[] GetMarkerY(double[] dy)
        {
            throw new NotImplementedException();
        }

        public void DisConnect()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
