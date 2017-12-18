using Ivi.Visa.Interop;
using System.Net;
using System.Threading;

namespace System.Matrix
{
    public class NetworkAnalyzer_N5225A: IVectorNetworkAnalyzer
    {
        #region   连接仪表
        //定义仪表连接标示符
        //private MessageBasedSession messageBased;
        //private NationalInstruments.VisaNS.MessageBasedSession messageBased = null;
        private Ivi.Visa.Interop.FormattedIO488 messageBased;
        ////定义读取的长度
        //private int intReadLength;
        //定义是否连接
        private bool _isConnect = false;

        public bool IsConnect
        {
            get { return _isConnect; }
        }

        public int PhaMarkPoint => throw new NotImplementedException();

        public int AttMarkPoint => throw new NotImplementedException();

        public bool Connected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name => throw new NotImplementedException();

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

        public NetworkAnalyzer_N5225A()
        {
            messageBased = new FormattedIO488();
            // intReadLength = 65300;//6553600;
        }

        /// <summary>
        /// 连接仪表
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public override bool Connect(string IP)
        {
            //bool boolRe = true;
            //try
            //{
            //    messageBased = (MessageBasedSession)ResourceManager.GetLocalManager().Open("TCPIP0::" + ip + "::inst0");
            //}
            //catch
            //{
            //    boolRe = false;
            //}
            //_isConnect = boolRe;
            //return boolRe;

            bool boolRe = true;
            try
            {
                IPAddress ipAddress;
                if (IPAddress.TryParse(IP, out ipAddress))
                {
                    #region SOCKET连接方式
                    ResourceManager grm = new ResourceManager();
                    messageBased.IO = (IMessage)grm.Open("TCPIP0::" + IP + "::5025::SOCKET", AccessMode.NO_LOCK, 2000, "");
                    messageBased.IO.Timeout = 200000;
                    messageBased.IO.SendEndEnabled = !messageBased.IO.SendEndEnabled;
                    messageBased.IO.TerminationCharacterEnabled = !messageBased.IO.TerminationCharacterEnabled;
                    if (ReadIDN() != "")
                        _isConnect = true;
                    else
                        boolRe = false;
                    #endregion
                }
                else
                    boolRe = false;
            }
            catch
            {
                boolRe = false;
            }
            return boolRe;
        }
        public override void WaitRun()
        {
            try
            {
                Write("INITiate:CONTinuous OFF");
            }
            catch (Exception ex)
            {
                Common.OutputForm.WriteMessage("Wait Run()=>INITiate:CONTinuous OFF  执行失败！");
            }
            try
            {
                Write("INITiate:IMMediate;*wai");
            }
            catch (Exception ex)
            {
                Common.OutputForm.WriteMessage("Wait Run()=>INITiate:CONTinuous OFF  执行失败！");
            }
        }

        /// <summary>
        /// 读取IDN
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public override string ReadIDN()
        {
            string strRe = "";
            try
            {
                string strCmd = "*IDN?";
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
        /// 断开链接
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                if (messageBased != null)
                {
                    //messageBased.Clear();
                    messageBased.IO.Close();
                }
            }
            catch { }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            this.Disconnect();
        }
        #endregion   连接仪表
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
        private string[] doubleArrToStringArr(double[] douRe)
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
        public override string[] ReadTraces(string trace, string format)
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

            int intTrace = 0;
            trace = trace.Replace("Trc", "");
            int.TryParse(trace, out intTrace);

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

                int intTrace = 0;
                trace = trace.Replace("Trc", "");
                int.TryParse(trace, out intTrace);

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
        public override void SetPower(string power)
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
        public override string ReadPower(string channel)
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
        public override string ReadIFBW(string channel)//string channel
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
        public override void SetIFBW(int IfbwValue)
        {
            string strCmd = "";
            int netChannel = 1;
            try
            {
                strCmd = "SENS" + netChannel.ToString().Trim() + ":BWID " + IfbwValue;
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
        public override string SetSingleSweepMode()
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
        public override void ReSetAnalyzer()
        {
            //:SYST:PRES
            try
            {
                string strcmd = "SYST:FPReset";
                Write(strcmd);
                string strcmd1 = "DISPlay:WINDow1:STATE ON";
                Write(strcmd1);
                string strcmd2 = "FORM ASCii,0 ";
                Write(strcmd2);
                QueryString("*OPC?");
            }
            catch
            {
            }
        }
        #endregion
        #region  设置Trace的Meas N

        /// <summary>
        /// 设置Trace的Meas  N
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="meas"></param>
        public override void SetMeasurementN(int trace, string meas)
        {
            //try
            //{
            //    //  CALCulate<cnum>:PARameter[:DEFine]:EXTended <Mname>,<param>

            //    //string strCmd = "CALCulate1:PARameter:DEFine:EXT 'Positive','" + "S21" + "'";
            //    //string strCmd = "CALCulate1:PARameter:DEFine:EXT 'Positive','" + "S21'";
            //    //string strCmd = "CALCulate1" + ":PARameter" + ":DEFine " + meas;
            //    //string strCmd = "CALCulate1:PARameter:DEFine:EXT 'Positive','" + "S21" + "'";
            //    string strCmd = "CALC1:PAR:DEF 'ch1_S21','S21'";//CALC4:PAR:EXT 'ch4_S33', 'S33'
            //    Write(strCmd);
            //    string strCmd1 = "DISPlay:WINDow1:TRACe1:FEED 'Positive'";
            //    Write(strCmd1);
            //    QueryString("*OPC?");
            //}
            //catch
            //{
            //}


            try
            {
                //string strCmd = "CALCulate1:PARameter:DEFine:EXT 'Positive','" + "S11" + "'";
                //CALCulate<cnum>:PARameter:SELect <Mname>
//                CALCulate<cnum>:PARameter:MODify <param>
                //CALCulate<cnum>:PARameter:CATalog?

                string s = "CALCulate" + trace + ":PARameter:CATalog?";
                string req = QueryString(s);
                string Mname = "";
                if (req.Contains("NO CATALOG"))
                {
                    string strCmd = "CALCulate" + trace + ":PARameter:DEFine:EXT 'Positive','" + meas + "'";
                    Write(strCmd);
                    string strCmd1 = "DISPlay:WINDow1:TRACe1:FEED 'Positive'";
                    Write(strCmd1);
                    Mname = "Positive";
                }
                else
                {
                    Mname = req.Split(',')[0].Replace("\"","");
                }
                

                string strcmd = "CALCulate" + trace + ":PARameter:SELect '"+Mname+"'";
                Write(strcmd);
                strcmd = "CALCulate" + trace + ":PARameter:MODify "+meas;
                Write(strcmd);
                QueryString("*OPC?");
            }
            catch
            {
            }
        }

        public override void deleteTrace(string trace)
        { 
//            CALCulate<cnum>:PARameter:DELete [:NAME]<Mname>
            try
            {
                string strCmd="CALCulate"+trace+":PARameter:DELete 'Positive'";
                Write(strCmd);
                QueryString("*OPC?");
            }
            catch
            { }

        }
        #endregion  设置Trace的Meas N
        #region   设置扫描
        /// <summary>
        /// 设置扫频点数
        /// </summary>
        /// <returns></returns>
        public override void SetSegmentPoint(int Points)
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
        #endregion   设置扫描 
        #region  获取仪表支持频点
        /// <summary>
        /// 获取仪表支持的最小的频点,单位转换为MHz
        /// </summary>
        /// <returns>单位为MHz</returns>
        public override double getFREQMIN()
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
            double Freq = 0;
            if (double.TryParse(strRe, out Freq))
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
        public override double getFREQMAX()
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
            double Freq = 0;
            if (double.TryParse(strRe, out Freq))
            {
                Freq = Freq / 1000000;
                return Freq;
            }
            else
                return double.NaN;
        }
        #endregion  获取仪表支持频点
        #region   告警操作
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
        #endregion  告警操作
        #region   文件操作
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
        public override void LOADFile(string FilePath)
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
        public override void STORFile(string FilePath)
        {
            if (messageBased != null)
            {
                string strCmd = "MMEM:STOR \"" + FilePath + "\"";
                Write(strCmd);
                //Execute the *OPC? command and wait until the command
                //messageBased.Query("*OPC?");
            }
        }
        #endregion   文件操作
        #region  Trace操作
        /// <summary>
        /// 设置Trace数量
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="traceNum"></param>
        public override void SetTraceNumber(int channel, int traceNum)
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
        public override void SetTrace(string trace, string sParameter)
        {
            ////CALC2:PAR:SDEF 'Trc2', 'S12'
            ////string strCmd = ":CALC1:PAR1" + ":DEF " + trace;
            //string strCmd = "CALC1:PAR:SDEF '" + trace + "', '" + sParameter + "'";
            //messageBased.Write(strCmd);
            trace = trace.Replace("Trc", "");
            string strCmd = "CALC1:PAR" + trace + ":DEF " + sParameter;
            Write(strCmd);

        }
        #endregion  Trace操作
        #region   设置频率
        /// <summary>
        /// 读取激励值(频点)
        /// </summary>
        public override string[] ReadStimulus()
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
                strRe = null;
            }
            return strRe;
        }

        /// <summary>
        /// 设置起始频率 SetStartFreq(100MHz)
        /// </summary>
        /// <param name="freq"></param>
        public override void SetStartFreq(string freq)
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
                _isConnect = false;
            }
        }


        /// <summary>
        /// 设置终止频率 SetStopFreq(100MHz)
        /// </summary>
        /// <param name="freq"></param>
        public override void SetStopFreq(string freq)
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
                _isConnect = false;
            }
        }
        #endregion   设置频率
        #region 设置模式
        public override void SelectFormat(string format)
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
                //CALCulate<cnum>:FORMat <char>
                //strCmd = "CALC1:FORM " + format.ToUpper();
                //strCmd = "CALCulate1:TRACe1:FORMat " + format.ToUpper();
                strCmd = "CALC:FORM " + format;
                Write(strCmd);
                Thread.Sleep(50);

            }
            catch
            {
                _isConnect = false;
            }
        }
        #endregion
        #region 设置marker点显示
        public override void SetMarkerState(bool display)
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
                _isConnect = false;
            }
        }
        #endregion
        #region 设置marker点激活
        public override void SetMarkerActive()
        {
            //激活后自动display marker点
            string strCmd = "";
            try
            {
                //strCmd = "CALC1:MARK1:ACT";
                strCmd = "CALC1:MARKer1:ON";
                Write(strCmd);
                Thread.Sleep(50);
            }
            catch
            {
                _isConnect = false;
            }
        }
        public override void SetMarkerActive(string onoff)
        {
            //激活后自动display marker点
            string strCmd = "";
            try
            {
                //strCmd = "CALC1:MARK1:ACT";
                //strCmd = "CALC1:MARKer1:ON";
                //                CALCulate<cnum>:MARKer<mkr>[:STATe] <ON|OFF>

                strCmd = "CALCulate1:MARKer1 " + onoff;
                Write(strCmd);
                Thread.Sleep(50);
            }
            catch
            {
                _isConnect = false;
            }
        }
        #endregion
        #region 设置marker横坐标
        public override void SetMarkerX(int trace, long x)
        {
            string strCmd = "";
            try
            {
                //strCmd = "CALC1:MARK1:X " + x.ToString();
                strCmd = "CALCulate"+trace+":MARKer1:X " + x.ToString();
                Write(strCmd);
                Thread.Sleep(50);
            }
            catch
            {
                _isConnect = false;
            }
        }
        #endregion
        #region 读取marker横坐标
        public override double GetMarkerY(int trace)
        {
            string strCmd = "";
            string sY;
            double dY;
            try
            {
                Write("INIT1:CONT ON");  //OK
                //Set the trigger source to Bus Trigger.
                //Write(":TRIG:SOUR BUS");  
                //Trigger the instrument to start a sweep cycle.
                //Write(":TRIG:SING"); 
                //Execute the *OPC? command and wait until the command
                QueryString("*OPC?");
                //strCmd = "CALC1:MARK1:Y?";
                //
                //strCmd = string.Format("CALCulate1:TRACe{0}" + ":MARKer1" + ":Y? ", trace);
                //                CALCulate<cnum>:MARKer<mkr>:Y?
                strCmd = "CALCulate" + trace + ":MARKer1:Y?";
                sY = QueryString(strCmd);
                dY = Convert.ToDouble(sY.Split(',')[0]);
                return dY;
            }
            catch
            {
                _isConnect = false;
            }
            return 0;
        }
        #endregion
        #region   设置AGC
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
        #endregion
        #region   发送、查询命令  
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="cmd"></param>
        public void Write(string cmd)
        {
            try
            {
                messageBased.WriteString(cmd);
                //   Log.Log.Logs("Write", cmd);

            }
            catch { }
            //Thread.Sleep(10);
        }
        public override double[] ReadTraceToDoubleATTN(string trace, int plds)
        {
            double[] revValue = null;
            if (messageBased != null)
            {
                //DataFormetToBinary();

                //SetTrace(trace, format);
                double[] data = new double[plds * 2];
                double[] data2 = new double[plds];
                //Write("INIT1:CONT ON");
                string str = QueryString("CALCulate" + trace.ToString() + ":DATA? SDATA");
                //Write("INIT1:CONT OFF");
                string[] strs = str.Split(',');
                for (int i = 0; i < plds * 2; i++)
                {
                    data[i] = float.Parse(strs[i]);
                }

                for (int i = 0; i < plds; i++)
                {
                    data2[i] = (double)(10 * Math.Log10((Math.Pow(data[i * 2], 2) + Math.Pow(data[i * 2 + 1], 2))));
                    //data2[i * 2 + 1] = (double)(Math.Atan2(data[i * 2 + 1], data[i * 2]) * 180 / Math.PI);
                }

                return data2;
                //return revValue;
            }
            return revValue;
        }
        /// <summary>
        /// 查询命令
        /// </summary>
        public string QueryString(string cmd)
        {
            string messageReCmd = "";
            messageBased.WriteString(cmd);//messageBased.Query(cmd, intReadLength);
            messageReCmd = messageBased.ReadString();
            // Log.Log.Logs("QueryString", cmd);
            if (messageReCmd == "")//容错机制
            {
                messageBased.WriteString(cmd);//messageBased.Query(cmd, intReadLength);
                messageReCmd = messageBased.ReadString();

                // Log.Log.Logs("QueryString", cmd);
                return messageReCmd;
            }
            else
            {
                this._isConnect = false;
                return messageReCmd;
            }
        }

        /// <summary>
        /// 查询命令
        /// </summary>
        public double[] QueryBinary(string cmd)
        {
            double[] messageReCmd = null;
            messageBased.WriteString(cmd);//messageBased.Query(cmd, intReadLength);
            messageReCmd = messageBased.ReadIEEEBlock(Ivi.Visa.Interop.IEEEBinaryType.BinaryType_R8, false, true) as double[];

            //    Log.Log.Logs("QueryBinary", cmd); 
            if (messageReCmd == null)//容错机制
            {
                messageBased.WriteString(cmd);//messageBased.Query(cmd, intReadLength);
                messageReCmd = messageBased.ReadIEEEBlock(Ivi.Visa.Interop.IEEEBinaryType.BinaryType_R8, false, true) as double[];
                return messageReCmd;
            }
            else
            {
                this._isConnect = false;
                return messageReCmd;
            }
        }

        public bool ReadIDN(out string IDN)
        {
            throw new NotImplementedException();
        }

        public void SetSegmentFreqIns(string StartFreq, string StopFreq, int Points, string Power, string SegmentTime, string Unused, string MeasBandwidth)
        {
            throw new NotImplementedException();
        }

        public void ActiveSegmentFreq()
        {
            throw new NotImplementedException();
        }

        public double GetFREQMIN()
        {
            throw new NotImplementedException();
        }

        public double GetFREQMAX()
        {
            throw new NotImplementedException();
        }

        public void LoadFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public void StoreFile(string filePath)
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

        public void SetMarkerX(long x)
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

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }
        #endregion   发送、查询命令
    }
}
