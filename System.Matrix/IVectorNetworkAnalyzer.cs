
namespace System.Matrix
{
    public interface IVectorNetworkAnalyzer : IConnected
    {
        /// <summary>
        /// 读取IDN
        /// </summary>
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

        /// <summary>
        /// 重置仪表
        /// </summary>
        void ReSetAnalyzer();

        /// <summary>
        /// 设置扫频点数
        /// </summary>
        void SetSegmentPoint(int Points);

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
        /// <param name="filePath">路径</param>
        void LoadFile(string filePath);

        /// <summary>
        /// 存储文件
        /// </summary>
        /// <param name="filePath">路径</param>
        void StoreFile(string filePath);

        /// <summary>
        /// 设置Trace数量
        /// </summary>
        void SetTraceNumber(int channel, int traceNum);

        /// <summary>
        /// 新增Trace 以及绑定的 sParameter
        /// </summary>
        void SetTrace(string trace, string sParameter);

        /// <summary>
        /// 读取激励值(频点)
        /// </summary>
        string[] ReadStimulus();

        /// <summary>
        /// 设置起始频率
        /// </summary>
        void SetStartFreq(string freq);

        /// <summary>
        /// 设置终止频率
        /// </summary>
        void SetStopFreq(string freq);

        /// <summary>
        /// 设置AGC
        /// </summary>
        void SetAGC_MANual();

        /// <summary>
        /// 设置AGC
        /// </summary>
        void SetAGC_Auto();

        /// <summary>
        /// 设置AGC
        /// </summary>
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

        int PhaMarkPoint { get; }
        int AttMarkPoint { get; }
    }
}
