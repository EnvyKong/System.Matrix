namespace System.Matrix
{
    public abstract class DeviceData
    {
        protected DeviceData()
        {
            GetCurrentDeviceData();
        }
        public abstract void GetCurrentDeviceData();
        public virtual string TypeName { get { return GetType().Name; } }
        public string IP { get; protected set; } = "192.168.0.0";
        public int PortNum { get; protected set; }
        public int APortConnectNum { get; protected set; }
        public int BPortConnectNum { get; protected set; }
        public int APortNum { get; protected set; }
        public int BPortNum { get; protected set; }
        public long Frequency { get; protected set; }
        public double PhaseStep { get; protected set; }
        public double AttenuationStep { get; protected set; }
        public int AttMarkPoint { get; protected set; }
        public int PhaMarkPoint { get; protected set; }
        public int AttCalFre { get; protected set; }
        public int PhaCalFre { get; protected set; }
        public PhaseStepShiftDirection PhaseStepShiftDirection { get; protected set; }
        public VNAType VNAType { get; protected set; }
    }
}
