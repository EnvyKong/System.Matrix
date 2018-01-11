using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Matrix
{
    public abstract class DeviceData
    {
        protected DeviceData()
        {
            GetDeviceData();
        }

        public abstract void GetDeviceData();

        public string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        public string IP { get; set; }
        public int PortNum { get; set; }
        public int APortConnectNum { get; set; }
        public int BPortConnectNum { get; set; }
        public int APortNum { get; set; }
        public int BPortNum { get; set; }
        public double PhaseStep { get; internal set; }
        public long Frequency { get; internal set; }
        public double AttenuationStep { get; internal set; }
        public int AttMarkPoint { get; internal set; }
        public int PhaMarkPoint { get; internal set; }
        public int AttCalFre { get;  protected set; }
        public int PhaCalFre { get; protected set; }
        public PhaseStepShiftDirection PhaseStepShiftDirection { get; internal set; }
        public VNAType VNAType { get; set; }
    }
}
