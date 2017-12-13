namespace System.Matrix
{
    public static class ViewConfigInfo
    {
        /// <summary>
        /// 校准前设置频率
        /// </summary>
        public static long Frequency { get; set; }

        /// <summary>
        /// 衰减步进
        /// </summary>
        public static double AttenuationStep { get; set; }

        /// <summary>
        /// 移相步进
        /// </summary>
        public static double PhaseStep { get; set; }

        /// <summary>
        /// 矢量网络分析仪IP
        /// </summary>
        public static string IPToVNA { get; set; }

        /// <summary>
        /// Vertex的IP
        /// </summary>
        public static string IPToVertex { get; set; }

        /// <summary>
        /// 连接Matrix设备CalBox的IP
        /// </summary>
        public static string IPToCalBoxForMatrix { get; set; }

        /// <summary>
        /// 连接Vertex设备CalBox的IP
        /// </summary>
        public static string IPToCalBoxForVertex { get; set; }

        /// <summary>
        /// 设码值时移相器偏移方向
        /// </summary>
        public static PhaseStepShiftDirection PhaseStepShiftDirection { get; set; }
    }
}
