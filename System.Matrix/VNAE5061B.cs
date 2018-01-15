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

        //public int PhaMarkPoint => (int)EntryData.GetPropertyValue("PhaMarkPoint");

        //public int AttMarkPoint => (int)EntryData.GetPropertyValue("AttMarkPoint");

        public int PhaMarkPoint => DeviceData.PhaMarkPoint;

        public int AttMarkPoint => DeviceData.AttMarkPoint;

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
        ///// <summary>
        ///// 选择Trace:更改当前Trace
        ///// </summary>
        ///// <param name="trace"></param>
        ///// <param name="format"></param>
        //public override bool SelTrace(string trace)
        //{
        //    bool boolRe = true;
        //    string strCmd = "";
        //    int netChannel = 1;
        //    int intTrace = 0;
        //    trace = trace.Replace("Trc", "");
        //    int.TryParse(trace, out intTrace);

        //    try
        //    {

        //        //strCmd = "CALC" + netChannel.ToString().Trim() + ":PAR" + (intTrace + 1) + ":SEL";// (intTrace + 1)
        //        strCmd = "CALC" + netChannel.ToString().Trim() + ":PAR" + intTrace.ToString() + ":SEL";// (intTrace + 1)
        //        Write(strCmd);

        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        Write("CALC1:PAR" + (Trace + 1) + ":SEL");
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        Write("CALC2:PAR" + (Trace + 1) + ":SEL");
        //        //        break;
        //        //}
        //    }
        //    catch
        //    {
        //        boolRe = false;
        //    }
        //    return boolRe;

        //    ////CALC2:PAR:SDEF 'Trc2', 'S12'
        //    ////string strCmd = ":CALC1:PAR1" + ":DEF " + trace;
        //    //string strCmd = "CALC1:PAR:SEL '" + trace + "'";
        //    //messageBased.Write(strCmd);
        //}
        private string[] DoubleArrToStringArr(double[] douRe)
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
            //try
            //{
            //    DataFormetToBinary();
            //    SetTrace(trace, format);
            //    //Turn on or off continuous initiation mode for each channel
            //    Write("INIT1:CONT ON");
            //    //Set the trigger source to Bus Trigger.
            //    Write(":TRIG:SOUR BUS");
            //    //Trigger the instrument to start a sweep cycle.
            //    Write(":TRIG:SING");
            //    //Execute the *OPC? command and wait until the command
            //    QueryString("*OPC?");
            //    // revValue = QueryBinary(":CALCulate1:TRACe1:DATA:FDATa?");
            //    string value = QueryString(":CALCulate1:TRACe1:DATA:FDATa?");
            //    Write("INIT1:CONT OFF");
            //    string[] arrValue = value.Split(',');
            //    revValue = new string[arrValue.Length];
            //    for (int i = 0; i < arrValue.Length; i++)
            //    {
            //        //if (i % 2 == 0)
            //        //{
            //        revValue[index] = arrValue[i];
            //        index++;
            //        //}
            //    }

            //}
            //catch
            //{
            //    //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //    //douRe = null;
            //}
            ////strRe = doubleArrToStringArr(douRe);
            //return revValue;

            ////////////////////////////////////////////

            //:CALCulate{[1]-160}:TRACe{[1]-16}:DATA:FDATa?
            //double[] douRe = null;
            //string[] strRe = null;
            string strCmd = "";
            int netChannel = 1;

            trace = trace.Replace("Trc", "");
            int.TryParse(trace, out int intTrace);

            string[] revValue = null;
            int index = 0;
            try
            {
                //SetTrace(trace, format);

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
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //douRe = null;
            }
            //strRe = doubleArrToStringArr(douRe);
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
                //string[] arrValue = value.Split(',');
                //revValue = new string[arrValue.Length];
                //for (int i = 0; i < arrValue.Length; i++)
                //{
                //    if (i % 2 == 0)
                //    {
                //        revValue[index] = arrValue[i];
                //        index++;
                //    }
                //}

            }
            catch
            {
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //douRe = null;
            }
            //strRe = doubleArrToStringArr(douRe);
            return revValue;

            ////////////////////////////////////////////

            ////:CALCulate{[1]-160}:TRACe{[1]-16}:DATA:FDATa?
            ////double[] douRe = null;
            ////string[] strRe = null;
            //string strCmd = "";
            //int netChannel = 1;

            //int intTrace = 0;
            //trace = trace.Replace("Trc", "");
            //int.TryParse(trace, out intTrace);

            //string[] revValue = null;
            //int index = 0;
            //try
            //{
            //    SetTrace(trace, format);

            //    Write("INIT1:CONT ON");
            //    //Set the trigger source to Bus Trigger.
            //    Write(":TRIG:SOUR BUS");
            //    //Trigger the instrument to start a sweep cycle.
            //    Write(":TRIG:SING");
            //    //Execute the *OPC? command and wait until the command
            //    QueryString("*OPC?");


            //    strCmd = ":CALCulate" + netChannel.ToString().Trim() + ":TRACe" + intTrace + ":DATA:" + format + "?";
            //    //douRe = QueryBinary(strCmd);//messageBased.(strCmd, intReadLength);

            //    string value = QueryString(strCmd);
            //    Write("INIT1:CONT OFF");

            //    string[] arrValue = value.Split(',');
            //    revValue = new string[arrValue.Length];
            //    for (int i = 0; i < arrValue.Length; i++)
            //    {
            //        //if (i % 2 == 0)
            //        //{
            //        revValue[index] = arrValue[i];
            //        index++;
            //        //}
            //    }

            //}
            //catch
            //{
            //    //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //    //douRe = null;
            //}
            ////strRe = doubleArrToStringArr(douRe);
            //return revValue;


        }

        /// <summary>
        /// 读取指定Trace 数据（先选择对应 Trace，再读取）
        /// 调用ReadTraces("Trc1","FDATa")
        /// </summary>
        public string ReadTrace(string trace, string format)
        {
            if (messageBased != null)
            { //:CALCulate{[1]-160}:TRACe{[1]-16}:DATA:FDATa?
                string strCmd = "";
                int netChannel = 1;

                trace = trace.Replace("Trc", "");
                int.TryParse(trace, out int intTrace);

                string value = null;
                try
                {

                    strCmd = ":CALCulate" + netChannel.ToString().Trim() + ":TRACe" + intTrace + ":DATA:" + format + "?";
                    //douRe = QueryBinary(strCmd);//messageBased.(strCmd, intReadLength);

                    value = QueryString(strCmd);


                }
                catch
                {
                    //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //douRe = null;
                }
                //strRe = doubleArrToStringArr(douRe);
                return value;
            }
            else
                return null;

        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public string ReadData()
        //{
        //    if (messageBased != null)
        //    {
        //        string strCmd = " CALC:DATA? FDAT ";
        //        string value = messageBased.Query(strCmd, 966523);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");

        //        return value;
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// 读取指定Trace 数据
        ///// </summary>
        ///// <param name="trace"></param>
        ///// <param name="format"></param>
        //public string ReadTrace(string trace, string format)
        //{
        //    //CALC2:PAR:SDEF 'Trc2', 'S12'
        //    if (messageBased != null)
        //    {
        //        //string strCmd = "CALC:DATA:TRAC? '" + trace + "', " + format + "";
        //        //设置当前Trace
        //        SelTrace(trace);
        //        //string strCmd = "CALC:DATA:TRAC? '" + trace + "', " + format + "";
        //        string strCmd = "CALC:DATA? " + format;
        //        string value = messageBased.Query(strCmd, 966523);

        //        return value;
        //    }
        //    return null;
        //}

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

                //switch (key)
                //{
                //    case ClassVNASwitchDevice.cmdKey.A:
                //        strCmd = "SENS1:SWE:POIN " + sweepPoints.ToString();
                //        break;
                //    case ClassVNASwitchDevice.cmdKey.B:
                //        strCmd = "SENS2:SWE:POIN " + sweepPoints.ToString();
                //        break;
                //}
                //Write(strCmd);
            }
            catch
            {
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        //public string[] ReadTrace(string trace, string format)
        //{
        //    string[] revValue = null;
        //    int index = 0;
        //    if (messageBased != null)
        //    {
        //        SetTrace(trace, format);
        //        //Turn on or off continuous initiation mode for each channel
        //        Write("INIT1:CONT ON");
        //        //Set the trigger source to Bus Trigger.
        //        Write(":TRIG:SOUR BUS");
        //        //Trigger the instrument to start a sweep cycle.
        //        Write(":TRIG:SING");
        //        //Execute the *OPC? command and wait until the command
        //        QueryString("*OPC?");
        //        string value = QueryString(":CALCulate1:TRACe1:DATA:FDATa?");
        //        string[] arrValue = value.Split(',');
        //        revValue = new string[arrValue.Length / 2];
        //        for (int i = 0; i < arrValue.Length; i++)
        //        {
        //            if (i % 2 == 0)
        //            {
        //                revValue[index] = arrValue[i];
        //                index++;
        //            }
        //        }
        //        Write("INIT1:CONT OFF");
        //        return revValue;
        //    }
        //    return revValue;
        //}

        ///// <summary>
        ///// 设置分段扫描
        ///// </summary>
        ///// <param name="StartFreq"></param>
        ///// <param name="StopFreq"></param>
        ///// <param name="Points"></param>
        ///// <param name="Power"></param>
        ///// <param name="SegmentTime"></param>
        ///// <param name="Unused"></param>
        ///// <param name="MeasBandwidth"></param>
        //public void SetSegmentFreqIns(string StartFreq, string StopFreq, int Points, string Power, string SegmentTime, string Unused, string MeasBandwidth)
        //{//SENSe<Ch>:]SEGMent<Seg>:INSert <StartFreq>, <StopFreq>, <Points>, <Power>, <SegmentTime>|<MeasDelay>, <Unused>, <MeasBandwidth>[, <LO>, <Selectivity>]

        //    //SEGM:INS 3GHZ, 8.5GHZ, 55,-21DBM, 0.5S, 0, 10KHZ
        //    if (messageBased != null)
        //    {
        //        string strCmd = " SEGM:INS " + StartFreq + "," + StopFreq + "," + Points.ToString() + "," + Power + "," + SegmentTime + "," + Unused + "," + MeasBandwidth;
        //        messageBased.Write(strCmd);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //    }
        //}
        ///// </summary>
        //public void SetSegmentFreqAdd()
        //{//SENSe<Ch>:]SEGMent<Seg>:INSert <StartFreq>, <StopFreq>, <Points>, <Power>, <SegmentTime>|<MeasDelay>, <Unused>, <MeasBandwidth>[, <LO>, <Selectivity>]

        //    //SEGM:INS 3GHZ, 8.5GHZ, 55,-21DBM, 0.5S, 0, 10KHZ
        //    if (messageBased != null)
        //    {
        //        string strCmd = " SEGM:ADD ";
        //        messageBased.Write(strCmd);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //    }
        //}
        ///// <summary>
        ///// 激活分段扫描：
        ///// </summary>
        //public void ActiveSegmentFreq()
        //{//SENSe<Ch>:]SEGMent<Seg>:INSert <StartFreq>, <StopFreq>, <Points>, <Power>, <SegmentTime>|<MeasDelay>, <Unused>, <MeasBandwidth>[, <LO>, <Selectivity>]

        //    //SEGM:INS 3GHZ, 8.5GHZ, 55,-21DBM, 0.5S, 0, 10KHZ
        //    if (messageBased != null)
        //    {
        //        string strCmd = " SWEep:TYPE SEGMent ";
        //        messageBased.Write(strCmd);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //    }
        //}

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
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                boolRe = false;
            }
            return boolRe;
        }
        ///// <summary>
        ///// 加载文件
        ///// </summary>
        ///// <param name="FilePath"></param>
        //public override bool LOADFile(string FilePath)
        //{

        //    bool boolRe = true;
        //    try
        //    {
        //        if (!ClearSystemErr())
        //            ClearSystemErr();

        //        //   string strCmd = "MMEM:LOAD \"" + FilePath + ".sta\"";
        //        string strCmd = "MMEM:LOAD \"" + FilePath + "\"";
        //        Write(strCmd);
        //        if (ReadSystemErr() != "-256,\"File name not found\"\n")//"-256,\"File name not found\"\n"
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch
        //    {
        //        //    MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        boolRe = false;
        //    }
        //    return boolRe;
        //    //if (messageBased != null)
        //    //{
        //    //    string strCmd = " MMEM:LOAD:STAT 1,'" + FilePath + "' ";
        //    //    messageBased.Write(strCmd);
        //    //    //Execute the *OPC? command and wait until the command
        //    //    //messageBased.Query("*OPC?");
        //    //}
        //}

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

                //   string strCmd = "MMEM:LOAD \"" + FilePath + ".sta\"";
                string strCmd = "MMEM:LOAD \"" + FilePath + "\"";
                Write(strCmd);
                //if (ReadSystemErr() != "-256,\"File name not found\"\n")//"-256,\"File name not found\"\n"
                //    return true;
                //else
                //    return false;
            }
            catch
            {
            }
        }

        ///// <summary>
        ///// 存储文件
        ///// </summary>
        ///// <param name="FilePath"></param>
        //public override bool STORFile(string FilePath)
        //{
        //    bool boolRe = true;
        //    try
        //    {
        //        // string strCmd = "MMEM:STOR \"" + FilePath + ".sta\"";
        //        string strCmd = "MMEM:STOR \"" + FilePath + "\"";
        //        Write(strCmd);
        //        // Log.Log.Logs("SaveStateFile", "bool SaveS2PFile(string FileName)", "保存S2P文件", "Write" + strCmd);

        //    }
        //    catch
        //    {
        //        //   MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        boolRe = false;
        //    }
        //    return boolRe;

        //    //if (messageBased != null)
        //    //{
        //    //    string strCmd = " MMEM:STOR:STAT 1,'" + FilePath + "' ";
        //    //    messageBased.Write(strCmd);
        //    //    //Execute the *OPC? command and wait until the command
        //    //    //messageBased.Query("*OPC?");
        //    //}
        //}
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
                //Execute the *OPC? command and wait until the command
                //messageBased.Query("*OPC?");
            }
        }


        ///// <summary>
        ///// 读取检测数据
        ///// </summary>
        //public string[] ReadDatas()
        //{
        //    string[] revValue = null;
        //    int index = 0;
        //    if (messageBased != null)
        //    {
        //        string strCmd = " CALC:DATA? FDAT ";
        //        string value = messageBased.Query(strCmd, 966523);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //        string[] arrValue = value.Split(',');
        //        revValue = new string[arrValue.Length];
        //        for (int i = 0; i < arrValue.Length; i++)
        //        {
        //            //if (i % 2 == 0)
        //            //{                    
        //            revValue[index] = arrValue[i];
        //            index++;
        //            //}
        //        }
        //        //messageBased.Write("INIT1:CONT OFF");
        //        return revValue;
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// 读取检测数据
        ///// </summary>
        //public double[] ReadDatasDouble()
        //{
        //    double[] revValue = null;
        //    int index = 0;
        //    if (messageBased != null)
        //    {
        //        string strCmd = " CALC:DATA? FDAT ";
        //        string value = messageBased.Query(strCmd, 66523);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //        string[] arrValue = value.Split(',');
        //        revValue = new double[arrValue.Length];
        //        for (int i = 0; i < arrValue.Length; i++)
        //        {
        //            //if (i % 2 == 0)
        //            //{                    
        //            revValue[index] = double.Parse(arrValue[i]);
        //            index++;
        //            //}
        //        }
        //        //messageBased.Write("INIT1:CONT OFF");
        //        return revValue;
        //    }
        //    return null;
        //}


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
                // boolConnected = false;
                //   MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        /// <summary>
        /// 新增Trace 以及绑定的 sParameter
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="format"></param>
        public void SetTrace(string trace, string sParameter)
        {
            ////CALC2:PAR:SDEF 'Trc2', 'S12'
            ////string strCmd = ":CALC1:PAR1" + ":DEF " + trace;
            //string strCmd = "CALC1:PAR:SDEF '" + trace + "', '" + sParameter + "'";
            //messageBased.Write(strCmd);
            trace = trace.Replace("Trc", "");
            string strCmd = "CALC1:PAR" + trace + ":DEF " + sParameter;
            Write(strCmd);

        }
        /// <summary>
        /// 读取激励值(频点)
        /// </summary>
        public string[] ReadStimulus()
        {
            //  double[] douRe = null;
            string strCmd = "";
            string[] revValue = null;
            int index = 0;

            try
            {
                strCmd = "SENS1:FREQ:DATA?";
                //douRe = QueryBinary(strCmd);
                string value = QueryString(strCmd);

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

                //switch (key)
                //{
                //    case ClassVNASwitchDevice.cmdKey.A:
                //        strCmd = "SENS1:FREQ:DATA?";
                //        strRe = QueryBinary(strCmd);
                //        break;
                //    case ClassVNASwitchDevice.cmdKey.B:
                //        strCmd = "SENS2:FREQ:DATA?";
                //        strRe = QueryBinary(strCmd);
                //        break;
                //}
            }
            catch (Exception ex)
            {
                Windows.Forms.MessageBox.Show(ex.ToString());
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //douRe = null;
            }

            //  string[] strRe = doubleArrToStringArr(douRe);
            return revValue;


            //string[] revValue = null;
            //int index = 0;
            //if (messageBased != null)
            //{
            //    string strCmd = " CALC:DATA:STIM? ";
            //    string value = messageBased.Query(strCmd, 966523);
            //    //return value;
            //    //Execute the *OPC? command and wait until the command
            //    //messageBased.Query("*OPC?");
            //    string[] arrValue = value.Split(',');
            //    revValue = new string[arrValue.Length];
            //    for (int i = 0; i < arrValue.Length; i++)
            //    {
            //        //if (i % 2 == 0)
            //        //{
            //        revValue[index] = arrValue[i];
            //        index++;
            //        //}
            //    }
            //    //messageBased.Write("INIT1:CONT OFF");
            //    return revValue;
            //}
            //return null;
        }
        ///// <summary>
        ///// 读取激励值(频点)
        ///// </summary>
        //public override string ReadStimulu()
        //{
        //    if (messageBased != null)
        //    {
        //        string strCmd = "SENS1:FREQ:DATA?";
        //        //douRe = QueryBinary(strCmd);
        //        string value = QueryString(strCmd);

        //        //messageBased.Write("INIT1:CONT OFF");
        //        return value;
        //    }
        //    return null;
        //}

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

                //switch (key)
                //{
                //    case ClassVNASwitchDevice.cmdKey.A:
                //        strCmd = "SENS1:FREQ:DATA?";
                //        strRe = QueryBinary(strCmd);
                //        break;
                //    case ClassVNASwitchDevice.cmdKey.B:
                //        strCmd = "SENS2:FREQ:DATA?";
                //        strRe = QueryBinary(strCmd);
                //        break;
                //}
            }
            catch (Exception ex)
            {
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

                //switch (key)
                //{
                //    case ClassVNASwitchDevice.cmdKey.A:
                //        strCmd = "SENS1:FREQ:STAR " + (StartFrequency * 1000000);//转换为Hz
                //        break;
                //    case ClassVNASwitchDevice.cmdKey.B:
                //        strCmd = "SENS2:FREQ:STAR " + (StartFrequency * 1000000);//转换为Hz
                //        break;
                //}
                //Write(strCmd);
                //Thread.Sleep(50);
            }
            catch
            {
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

        ///// <summary>
        ///// 设置AGC
        ///// </summary>
        ///// <param name="freq"></param>
        //public void SetAGC_MANual()
        //{
        //    if (messageBased != null)
        //    {
        //        //  string strCmd = "SENS1:FREQ:STOP " + freq;//转换为Hz SENSe:POWer:GAINcontrol 'B2D1', LNO

        //        string strCmd = ":SENSe:POWer:GAINcontrol:GLOBal MANual";
        //        messageBased.Write(strCmd);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //    }
        //}
        ///// <summary>
        ///// 设置AGC
        ///// </summary>
        ///// <param name="freq"></param>
        //public void SetAGC_LNO()
        //{
        //    if (messageBased != null)
        //    {
        //        //  string strCmd = "SENS1:FREQ:STOP " + freq;//转换为Hz SENSe:POWer:GAINcontrol 'B2D1', LNO

        //        string strCmd = ":SENSe:POWer:GAINcontrol:GLOBal LNO";
        //        messageBased.Write(strCmd);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //    }
        //}
        //-----------------------------------------------------------------------------






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



        ///// <summary>
        ///// 设置校准类型，默认为2端口校准
        ///// </summary>
        //public void SetCalType(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber)
        //{
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = "SENS" + netChannel.ToString().Trim() + ":CORR:COLL:METH:SOLT2 1,2";
        //            Write(strCmd);
        //            Thread.Sleep(50);
        //        }

        //        //switch (key)
        //        //{ 
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = "SENS1:CORR:COLL:METH:SOLT2 1,2";
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = "SENS2:CORR:COLL:METH:SOLT2 1,2";
        //        //        break;
        //        //}
        //        //Write(strCmd);
        //        //Thread.Sleep(50);
        //    }
        //    catch
        //    {
        //        MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        _isConnect = false;
        //    }
        //}


        ///// <summary>
        ///// 设置起始频率
        ///// </summary>
        ///// <returns></returns>
        //public void SetStartFrequency(decimal StartFrequency, ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber)
        //{
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = "SENS" + netChannel.ToString().Trim() + ":FREQ:STAR " + (StartFrequency * 1000000);//转换为Hz
        //            Write(strCmd);
        //            Thread.Sleep(50);
        //        }

        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = "SENS1:FREQ:STAR " + (StartFrequency * 1000000);//转换为Hz
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = "SENS2:FREQ:STAR " + (StartFrequency * 1000000);//转换为Hz
        //        //        break;
        //        //}
        //        //Write(strCmd);
        //        //Thread.Sleep(50);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        _isConnect = false;
        //    }
        //}

        ///// <summary>
        ///// 设置终止频率
        ///// </summary>
        ///// <returns></returns>
        //public void SetStopFrequency(decimal StopFrequency, ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber)
        //{
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = "SENS" + netChannel.ToString().Trim() + ":FREQ:STOP " + (StopFrequency * 1000000);//转换为Hz
        //            Write(strCmd);
        //            Thread.Sleep(50);
        //        }
        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = "SENS1:FREQ:STOP " + (StopFrequency * 1000000);//转换为Hz
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = "SENS2:FREQ:STOP " + (StopFrequency * 1000000);//转换为Hz
        //        //        break;
        //        //}
        //        //Write(strCmd);
        //        //Thread.Sleep(50);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        _isConnect = false;
        //    }
        //}

        ///// <summary>
        ///// 读取Power
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public string ReadPower(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber)
        //{
        //    string strRe = "";
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = ":SOUR" + netChannel.ToString().Trim() + ":POW?";
        //            strRe = QueryString(strCmd);
        //        }

        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = ":SOUR1:POW?";
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = ":SOUR2:POW?";
        //        //        break;
        //        //}
        //        //strRe = QueryString(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        strRe = "";
        //    }
        //    return strRe;
        //}

        ///// <summary>
        ///// 设置Power
        ///// </summary>
        ///// <param name="power"></param>
        ///// <param name="key"></param>
        //public void SetPower(decimal power, ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber)
        //{
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = ":SOUR" + netChannel.ToString().Trim() + ":POW " + power.ToString();
        //            Write(strCmd);
        //        }

        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        Write(":SOUR1:POW " + power.ToString());
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        Write(":SOUR2:POW " + power.ToString());
        //        //        break;
        //        //}
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}


        ///// <summary>
        ///// 读取Trace的个数
        ///// </summary>
        ///// <returns></returns>
        //public string ReadTraceNumber(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber)//CALC1:PAR:COUN?
        //{
        //    string strRe = "";
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            // strCmd = "CALCulate" + netChannel.ToString().Trim() + ":TRACe" + (traceIndex + 1) + ":MARKer" + (markerIndex + 1) + ":X " + freq;
        //            strCmd = "CALC" + netChannel.ToString().Trim() + ":PAR:COUN?";
        //            strRe = QueryString(strCmd);
        //        }

        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = "CALC1:PAR:COUN?";
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = "CALC2:PAR:COUN?";
        //        //        break;
        //        //}

        //        //strRe = QueryString(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        strRe = "";
        //    }
        //    return strRe;
        //}

        /////// <summary>
        /////// 设置Trace的个数
        /////// </summary>
        /////// <returns></returns>
        ////public void SetTraceNumber(ClassVNASwitchDevice.cmdKey key,string traceNum)//CALC1:PAR:COUN?
        ////{
        ////    string strCmd = "";
        ////    try
        ////    {
        ////        switch (key)
        ////        {
        ////            case ClassVNASwitchDevice.cmdKey.A:
        ////                strCmd = "CALC1:PAR:COUN " + traceNum;
        ////                break;
        ////            case ClassVNASwitchDevice.cmdKey.B:
        ////                strCmd = "CALC2:PAR:COUN " + traceNum;
        ////                break;
        ////        }
        ////        Write(strCmd);
        ////    }
        ////    catch
        ////    {
        ////        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        ////    }
        ////}

        ///// <summary>
        ///// 设置Trace的个数
        ///// </summary>
        ///// <returns></returns>
        //public void SetTraceNumber(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber, string traceNum)//CALC1:PAR:COUN?
        //{
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {

        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = "CALC" + netChannel.ToString().Trim() + ":PAR:COUN " + traceNum;
        //            Write(strCmd);
        //        }

        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = "CALC1:PAR:COUN " + traceNum;
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = "CALC2:PAR:COUN " + traceNum;
        //        //        break;
        //        //}
        //        //Write(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        ///// <summary>
        ///// 设置Trace数量
        ///// </summary>
        ///// <param name="channel"></param>
        ///// <param name="traceNum"></param>
        //public void SetTraceNumber(int channel, int traceNum)
        //{
        //    try
        //    {
        //        string strCmd = "CALC" + channel + ":PAR:COUN " + traceNum;
        //        Write(strCmd);
        //    }
        //    catch
        //    {
        //        _isConnect = false;
        //        MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        ///// <summary>
        ///// 设置Trace的Meas
        ///// </summary>
        ///// <param name="channel"></param>
        ///// <param name="traceNum"></param>
        //public void SetMeasurement(int channel, int trace, string meas)
        //{
        //    try
        //    {
        //        string strCmd = "CALC" + channel + ":PAR" + trace + ":DEF " + meas;
        //        Write(strCmd);
        //    }
        //    catch
        //    {
        //        _isConnect = false;
        //        MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}
        ////--------------------------
        ///// <summary>
        ///// 读取Trace的个数
        ///// </summary>
        ///// <returns></returns>
        //public string ReadCurrentTrace(int trace, ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber)//CALC1:PAR6:DEF?
        //{
        //    string strRe = "";
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = "CALC" + netChannel.ToString().Trim() + ":PAR" + (trace + 1) + ":DEF?";
        //            strRe = QueryString(strCmd);//messageBased.(strCmd, intReadLength);
        //        }
        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = "CALC1:PAR" + (trace + 1) + ":DEF?";
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = "CALC2:PAR" + (trace + 1) + ":DEF?";
        //        //        break;
        //        //}
        //        //strRe = QueryString(strCmd);//messageBased.(strCmd, intReadLength);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        strRe = "";
        //    }
        //    return strRe;
        //}

        ///// <summary>
        ///// 读取Trace的Format
        ///// </summary>
        ///// <returns></returns>
        //public string ReadFormat(int TraceIndex, ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber)//CALCulate1:TRACe2:FORMat?
        //{
        //    string strRe = "";
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {

        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = "CALCulate" + netChannel.ToString().Trim() + ":TRACe" + (TraceIndex + 1) + ":FORMat?";
        //            strRe = QueryString(strCmd).Replace("\n", "");
        //        }
        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = "CALCulate1:TRACe" + (TraceIndex + 1) + ":FORMat?";
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = "CALCulate2:TRACe" + (TraceIndex + 1) + ":FORMat?";
        //        //        break;
        //        //}
        //        //strRe = QueryString(strCmd).Replace("\n", "");
        //    }
        //    catch
        //    {
        //        strRe = "";
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //    return strRe;
        //}
        ///// <summary>
        ///// 读取Trace的Data数据
        ///// </summary>
        ///// <param name="trace"></param>
        ///// <param name="key"></param>
        ///// <param name="VNAChannelNumber"></param>
        ///// <returns></returns>
        //public double[] ReadFormData(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber, int trace)
        //{
        //    //:CALCulate{[1]-160}:TRACe{[1]-16}:DATA:FDATa?
        //    double[] strRe = null;
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = ":CALCulate" + netChannel.ToString().Trim() + ":TRACe" + (trace + 1) + ":DATA:FDATa?";
        //            strRe = QueryBinary(strCmd);//messageBased.(strCmd, intReadLength);
        //        }
        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = ":CALCulate1:TRACe" + (trace + 1) + ":DATA:FDATa?";
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = ":CALCulate2:TRACe" + (trace + 1) + ":DATA:FDATa?";
        //        //        break;
        //        //}
        //        //strRe = QueryBinary(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        strRe = null;
        //    }
        //    return strRe;
        //}
        ///// <summary>
        ///// 读取Trace的Data数据(兼容  revision A.9.60  以前的版本)
        ///// </summary>
        ///// <param name="trace"></param>
        ///// <param name="key"></param>
        ///// <param name="VNAChannelNumber"></param>
        ///// <returns></returns>
        //public double[] ReadFormDataOld(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber, int trace)
        //{
        //    //:CALCulate{[1]-160}:TRACe{[1]-16}:DATA:FDATa?
        //    double[] strRe = null;
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);
        //            //:CALCulate{[1]-4}:PARameter{[1]-4}:SELect 选择Channel和Trace
        //            //激活指定Trace
        //            this.SelectTrace(netChannel, trace);
        //            //获取数据
        //            strCmd = ":CALCulate" + netChannel.ToString().Trim() + ":DATA:FDATa?";
        //            strRe = QueryBinary(strCmd);//messageBased.(strCmd, intReadLength);
        //        }
        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = ":CALCulate1:TRACe" + (trace + 1) + ":DATA:FDATa?";
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = ":CALCulate2:TRACe" + (trace + 1) + ":DATA:FDATa?";
        //        //        break;
        //        //}
        //        //strRe = QueryBinary(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        strRe = null;
        //    }
        //    return strRe;
        //}

        ///// <summary>
        ///// 读取Trace的Mem数据
        ///// </summary>
        ///// <param name="key"></param>
        //public double[] ReadFormMem(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber, int trace)
        //{//;":CALC1:MATH:MEM"

        //    //CALCulate{[1]-160}:TRACe{[1]-16}:MATH:MEMorize
        //    double[] strRe = null;
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = "CALCulate" + netChannel.ToString().Trim() + ":TRACe" + (trace + 1) + ":DATA:FMEMory?";
        //            strRe = QueryBinary(strCmd);//messageBased.(strCmd, intReadLength);

        //        }

        //        //switch (key)
        //        //{
        //        //    case ClassVNASwitchDevice.cmdKey.A:
        //        //        strCmd = "CALCulate1:TRACe" + (trace + 1) + ":MATH:MEMorize";
        //        //        break;
        //        //    case ClassVNASwitchDevice.cmdKey.B:
        //        //        strCmd = "CALCulate2:TRACe" + (trace + 1) + ":MATH:MEMorize";
        //        //        break;
        //        //}
        //        //Write(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        strRe = null;
        //    }
        //    return strRe;
        //}

        //public void SetFormData(int trace, string format, ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber)
        //{
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = ":CALCulate" + netChannel.ToString().Trim() + ":TRACe" + (trace + 1) + ":FORMat " + format;
        //            Write(strCmd);
        //        }
        //        // switch (key)
        //        // {
        //        //     case ClassVNASwitchDevice.cmdKey.A:
        //        //         strCmd = ":CALCulate1:TRACe" + (trace + 1) + ":FORMat " + format;
        //        //         break;
        //        //     case ClassVNASwitchDevice.cmdKey.B:
        //        //         strCmd = ":CALCulate2:TRACe" + (trace + 1) + ":FORMat " + format;
        //        //         break;
        //        // }
        //        //Write( strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        ///// <summary>
        ///// 读取仪表的频率范围——最小值
        ///// </summary>
        ///// <returns></returns>
        //public string ReadVNAFreqMin()
        //{
        //    string strRe = "";
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = ":SERV:SWE:FREQ:MIN?";
        //        strRe = QueryString(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        strRe = "";
        //    }
        //    return strRe;
        //}

        ///// <summary>
        ///// 读取仪表的频率范围——最大值
        ///// </summary>
        ///// <returns></returns>
        //public string ReadVNAFreqMax()
        //{
        //    string strRe = "";
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = ":SERV:SWE:FREQ:MAX?";
        //        strRe = QueryString(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        strRe = "";
        //    }
        //    return strRe;
        //}

        //#region Display > Display > Mem (when the data trace display is OFF)
        ///// <summary>
        ///// 设置  Display > Display -> Data/Math/ Data&&Math
        ///// 设置数据的显示方式:是否显示Mem
        ///// </summary>
        ///// <param name="key"></param>
        //public void SetDisplyMem(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber, int trace, string OnOff)
        //{
        //    //CALCulate{[1]-160}:TRACe{[1]-16}:MATH:MEMorize
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);
        //            //:DISP:WIND1:TRAC2:MEM OFF
        //            strCmd = ":DISP:WIND" + netChannel.ToString().Trim() + ":TRACe" + (trace + 1) + ":MEM " + OnOff;
        //            Write(strCmd);
        //        }

        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        ///// <summary>
        ///// 设置  Display > Display -> Data/Math/ Data&&Math
        ///// 设置数据的显示方式:是否显示Mem
        ///// </summary>
        ///// <param name="key"></param>
        //public void SetDisplyData(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber, int trace, string OnOff)
        //{
        //    //CALCulate{[1]-160}:TRACe{[1]-16}:MATH:MEMorize
        //    string strCmd = "";
        //    int netChannel = 0;

        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);
        //            //:DISP:WIND1:TRAC2:MEM OFF
        //            strCmd = ":DISP:WIND" + netChannel.ToString().Trim() + ":TRACe" + (trace + 1) + ":STAT " + OnOff;
        //            Write(strCmd);
        //        }
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}
        //#endregion

        ///// <summary>
        ///// 读取OPC
        ///// </summary>
        ///// <param name="trace"></param>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public string ReadOPC()
        //{
        //    //*OPC?
        //    string strRe = "";
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = "*OPC?";
        //        strRe = QueryString(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //        strRe = "";
        //    }
        //    return strRe;
        //}

        ///// <summary>
        ///// 默认为总线模式
        ///// </summary>
        //public void SetTriggerSource()
        //{
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = ":TRIG:SOUR BUS";
        //        Write(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        ///// <summary>
        ///// 设置触发单个通道
        ///// </summary>
        //public void SetTriggerScope()
        //{
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = ":TRIG:SCOP ACT";
        //        Write(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}
        ///// <summary>
        ///// 设置触发所有通道
        ///// </summary>
        //public void SetTriggerScopeAll()
        //{
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = ":TRIG:SCOP ALL";
        //        Write(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        ///// <summary>
        ///// 默认为总线模式
        ///// </summary>
        //public void SetTriggerSource_Internal()
        //{
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = ":TRIG:SOUR INTernal";
        //        Write(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        ///// <summary>
        ///// 默认为总线模式
        ///// </summary>
        //public void SetTriggerSource_srq_Sginal()
        //{
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = "*TRG";
        //        Write(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}

        ///// <summary>
        ///// 设置扫描周期
        ///// </summary>
        //public void SetTriggerSweepCycle()
        //{
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = ":TRIG:SING";
        //        Write(strCmd);
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("VNA disconnected.Please open again.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }
        //}


        ////D1_2
        ///// <summary>
        ///// 读取Channel状态
        ///// </summary>
        ///// <param name="key"></param>
        //public string ReadChannelSplit()
        //{
        //    //:DISP:SPL?
        //    string strCmd = "";
        //    string strRe = "";
        //    try
        //    {
        //        strCmd = ":DISP:SPL?";
        //        strRe = QueryString(strCmd);
        //    }
        //    catch
        //    {
        //    }
        //    return strRe;
        //}


        ///// <summary>
        ///// 设置关闭Smoothing
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="traceIndex"></param>
        ///// <returns></returns>
        //public void SetSmoothing(ClassVNASwitchDevice.cmdKey key, int VNAChannelNumber, int traceIndex)
        //{
        //    //:CALCulate{[1]-4}[:SELected]:SMOothing[:STATe] 
        //    string strCmd = "";
        //    int netChannel = 0;
        //    try
        //    {
        //        if (VNAChannelNumber >= 1 && VNAChannelNumber <= 2)// VNA multlplexer box一个端口最多支持2各Channel
        //        {
        //            netChannel = GetNetChannel(key, VNAChannelNumber);

        //            strCmd = "CALC" + netChannel.ToString().Trim() + ":TRAce" + (traceIndex + 1) + ":SMOothing OFF";
        //            Write(strCmd);
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        ///// <summary>
        ///// 控制仪表界面实时显示
        ///// </summary>
        ///// <param name="enble"></param>
        //public void DisplayEnble(bool enble)
        //{
        //    string strCmd = "";
        //    if (enble)
        //        strCmd = ":DISP:ENAB ON";
        //    else
        //        strCmd = ":DISP:ENAB OFF";
        //    Write(strCmd);
        //}

        ///// <summary>
        ///// 设置仪表界面和按钮锁
        ///// </summary>
        ///// <param name="lockKey"></param>
        //public void SetPanel_KeyboardLock(bool lockKey)
        //{
        //    string strCmd = "";
        //    if (lockKey)
        //        strCmd = ":SYST:KLOC:KBD ON";
        //    else
        //        strCmd = ":SYST:KLOC:KBD OFF";
        //    Write(strCmd);
        //}

        ///// <summary>
        ///// 设置仪表界面和按钮锁
        ///// </summary>
        ///// <param name="lockKey"></param>
        //public void SetTouch_MouseLock(bool lockKey)
        //{
        //    string strCmd = "";
        //    if (lockKey)
        //        strCmd = ":SYST:KLOC:MOUS  ON";
        //    else
        //        strCmd = ":SYST:KLOC:MOUS  OFF";
        //    Write(strCmd);
        //}

        ///// <summary>
        ///// 数据格式改为二进制
        ///// </summary>
        //public void DataFormetToBinary()
        //{
        //    string strCmd = "";
        //    strCmd = ":FORM:DATA REAL";
        //    Write(strCmd);
        //}

        ///// <summary>
        ///// 设置复位
        ///// </summary>
        //public void SetPreset()
        //{
        //    //:SYST:PRES
        //    string strCmd = "";
        //    try
        //    {
        //        strCmd = ":SYST:PRES";
        //        Write(strCmd);
        //    }
        //    catch
        //    {
        //    }
        //}


        ///// <summary>
        ///// ABORt
        ///// </summary>
        //public void ABORt()
        //{
        //    Write("ABORt");
        //}

        /////// <summary>
        /////// 清空设备
        /////// </summary>
        ////public void ClearDevice()
        ////{
        ////    messageBased.IO.Clear();
        ////}

        ///// <summary>
        ///// 发送命令
        ///// </summary>
        ///// <param name="cmd"></param>
        //private void Write(string cmd)
        //{
        //    try
        //    {
        //        messageBased.WriteString(cmd);
        //        //   Log.Log.Logs("Write", cmd);

        //    }
        //    catch { }
        //    //Thread.Sleep(10);
        //}

        ///// <summary>
        ///// 查询命令
        ///// </summary>
        //private string QueryString(string cmd)
        //{
        //    string messageReCmd = "";
        //    messageBased.WriteString(cmd);//messageBased.Query(cmd, intReadLength);
        //    messageReCmd = messageBased.ReadString();
        //    // Log.Log.Logs("QueryString", cmd);
        //    if (messageReCmd == "")//容错机制
        //    {
        //        messageBased.WriteString(cmd);//messageBased.Query(cmd, intReadLength);
        //        messageReCmd = messageBased.ReadString();

        //        // Log.Log.Logs("QueryString", cmd);
        //        return messageReCmd;
        //    }
        //    else
        //    {
        //        this._isConnect = false;
        //        return messageReCmd;
        //    }
        //}

        ///// <summary>
        ///// 查询命令
        ///// </summary>
        //private double[] QueryBinary(string cmd)
        //{
        //    double[] messageReCmd = null;
        //    messageBased.WriteString(cmd);//messageBased.Query(cmd, intReadLength);
        //    messageReCmd = messageBased.ReadIEEEBlock(Ivi.Visa.Interop.IEEEBinaryType.BinaryType_R8, false, true) as double[];

        //    //    Log.Log.Logs("QueryBinary", cmd); 
        //    if (messageReCmd == null)//容错机制
        //    {
        //        messageBased.WriteString(cmd);//messageBased.Query(cmd, intReadLength);
        //        messageReCmd = messageBased.ReadIEEEBlock(Ivi.Visa.Interop.IEEEBinaryType.BinaryType_R8, false, true) as double[];
        //        return messageReCmd;
        //    }
        //    else
        //    {
        //        this._isConnect = false;
        //        return messageReCmd;
        //    }
        //}

        ///// <summary>
        ///// 等待
        ///// </summary>
        //private void Wait()
        //{
        //    //messageBased.Write("*WAI");
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public void Close()
        //{
        //    try
        //    {
        //        if (messageBased.IO != null)
        //        {
        //            messageBased.IO.Clear();
        //            messageBased.IO.Close();
        //        }
        //    }
        //    catch { }
        //}

        ///// <summary>
        ///// 释放资源
        ///// </summary>
        //public void Dispose()
        //{
        //    try
        //    {
        //        if (messageBased != null)
        //        {
        //            //messageBased.Clear();
        //            messageBased.IO.Close();
        //        }
        //    }
        //    catch { }
        //}


    }
}
