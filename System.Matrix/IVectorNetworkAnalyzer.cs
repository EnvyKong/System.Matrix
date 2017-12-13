using TopYoung.MV.Core;

namespace System.Matrix
{
    public interface IVectorNetworkAnalyzer : IConnected
    {
        //定义仪表连接标示符
        //protected NationalInstruments.VisaNS.MessageBasedSession messageBased = null;

        /// <summary>
        /// 读取IDN
        /// </summary>
        /// <param name="IDN">设备IDN</param>
        /// <returns>是否读取成功</returns>
        bool ReadIDN(out string IDN);

        /// <summary>
        /// 读取指定 Trace 数据（先选择对应 Trace，再读取）
        /// </summary>
        string[] ReadTraces(string trace, string format);

        /// <summary>
        /// 设置Channel1 的Power. 此命令需测试验证.
        /// </summary>
        void SetPower(string power);


        /// <summary>
        /// 读取指定Channel 的Power. 此命令需测试验证.
        /// </summary>
        string ReadPower(string channel);

        /// <summary>
        /// 读取IFBW
        /// </summary>
        string ReadIFBW(string channel);

        /// <summary>
        /// 设置IFBW
        /// </summary>
        void SetIFBW(int IfbwValue);


        /// <summary>
        /// 设置single sweep mode
        /// </summary>
        string SetSingleSweepMode();


        // Reset the analyzer
        //*RST
        //:SYSTEM:DISPLAY:UPDATE ON
        //// Delete external device tables
        //:SYSTem:COMMunicate:RDEVice:GENerator:DELete
        //:SYSTem:COMMunicate:RDEVice:PMETer:DELete    

        /// <summary>
        /// 重置仪表
        /// </summary>
        void ReSetAnalyzer();

        /// <summary>
        /// 设置扫频点数
        /// </summary>
        void SetSegmentPoint(int Points);
        /// <summary>
        /// 
        /*SENSe<Ch>:]SEGMent<Seg>:INSert <StartFreq>, <StopFreq>, <Points>, <Power>, <SegmentTime>|<MeasDelay>, <Unused>, <MeasBandwidth>[, <LO>, <Selectivity>]
         Example:  SEGM:INS 1MHZ, 1.5MHZ, 111, -21DBM, 0.5S, 0, 10KHZ
        Create a sweep segment with a sweep range between 1.0 MHz and 1.5 MHz.
        SEGM2:ADD
        Create a second sweep segment. The frequency range of the second segment will be between 1.5 MHz and the maximum 
         * 这里需要增加容错功能:StartFreq,  StopFreq 必须在网络分析仪支持的频率范围.
            */
        /// </summary>
        /// <param name="StartFreq"></param>
        /// <param name="StopFreq"></param>
        /// <param name="Points"></param>
        /// <param name="Power"></param>
        /// <param name="SegmentTime"></param>
        /// <param name="Unused"></param>
        /// <param name="MeasBandwidth"></param>
        void SetSegmentFreqIns(string StartFreq, string StopFreq, int Points, string Power, string SegmentTime, string Unused, string MeasBandwidth);


        /// <summary>
        /// 激活分段扫描：
        /// </summary>
        void ActiveSegmentFreq();


        /// <summary>
        /// 获取仪表支持的最小的频点,单位转换为MHz
        /// </summary>
        /// <returns>单位为MHz</returns>
        double GetFREQMIN();

        /// <summary>
        /// 获取仪表支持的最大的频点,单位转换为MHz
        /// </summary>
        /// <returns>单位为MHz</returns>
        double GetFREQMAX();


        /// <summary>
        /// 加载文件
        /// </summary>
        /// <param name="filePath"></param>
        void LoadFile(string filePath);


        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="filePath"></param>
        void StoreFile(string filePath);


        /// <summary>
        /// 设置Trace数量
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="traceNum"></param>
        void SetTraceNumber(int channel, int traceNum);

        /// <summary>
        /// 新增Trace 以及绑定的 sParameter
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="format"></param>
        void SetTrace(string trace, string sParameter);

        ///// <summary>
        ///// 读取激励值(频点)
        ///// </summary>
        //public string ReadStimulus()
        //{
        //    if (messageBased != null)
        //    {
        //        string strCmd = " CALC:DATA:STIM? ";
        //        string value = messageBased.Query(strCmd);
        //        return value;
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //    }
        //    return null;
        //}
        /// <summary>
        /// 读取激励值(频点)
        /// </summary>
        string[] ReadStimulus();

        /// <summary>
        /// 设置起始频率
        /// </summary>
        /// <param name="freq"></param>
        void SetStartFreq(string freq);

        /// <summary>
        /// 设置终止频率
        /// </summary>
        /// <param name="freq"></param>
        void SetStopFreq(string freq);


        /// <summary>
        /// 设置AGC
        /// </summary>
        /// <param name="freq"></param>
        void SetAGC_MANual();


        /// <summary>
        /// 设置AGC
        /// </summary>
        /// <param name="freq"></param>
        void SetAGC_Auto();


        /// <summary>
        /// 设置AGC
        /// </summary>
        /// <param name="freq"></param>
        void SetAGC_LNO();


        void SelectFormat(string format);


        void SetMarkerState(bool display);


        void SetMarkerActive();


        void SetMarkerX(int trace, long x);


        void SetMarkerX(long x);


        double[] ReadFrq();


        double GetMarkerY(int trace);


        double GetMarkerY();

        double[] GetMarkerY(double[] dy);


        ///// <summary>
        ///// 写命令
        ///// </summary>
        ///// <param name="FilePath"></param>
        //public void Write(string strCmd)
        //{
        //    if (messageBased != null)
        //    {
        //        // string strCmd = " MMEM:STOR:STAT 1,'" + FilePath + "' ";
        //        messageBased.Write(strCmd);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //    }
        //}
        ///// <summary>
        ///// 查询命令
        ///// </summary>
        ///// <param name="FilePath"></param>
        //public string Query(string strCmd)
        //{
        //    if (messageBased != null)
        //    {
        //        // string strCmd = " MMEM:STOR:STAT 1,'" + FilePath + "' ";
        //        string value = messageBased.Query(strCmd, 66523);
        //        //Execute the *OPC? command and wait until the command
        //        //messageBased.Query("*OPC?");
        //        return value;
        //    }
        //    return null;
        //}
    }

}
