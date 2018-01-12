using System.Collections.Generic;

namespace System.Matrix
{
    public abstract class MatrixSystem
    {
        public MatrixSystem(List<DeviceData> deviceDatas)
        {
            DeviceData = deviceDatas;
            Initialize();
        }

        protected List<DeviceData> DeviceData { get; }

        protected IVectorNetworkAnalyzer VNA { get; set; }
        protected Matrix Matrix { get; set; }
        protected List<Vertex> Vertexs { get; set; }
        protected CalBoxToMatrix CalBoxToMatrix { get; set; }
        protected CalBoxToVertex CalBoxToVertex { get; set; }
        protected CalBoxWhole CalBoxWhole { get; set; }
        protected SwitchAdapter<ISwitch> SwitchAdapter { get; set; }

        public abstract void Initialize();
        public abstract void Calibrate();
        public abstract void CalibrateByFile(string path);
        public abstract void ConnectAll();
        public abstract void DisConnectAll();
        public abstract void OutputResult(string savePath);
        public abstract void SetValueDynamic(string path);
        public abstract void SetValueStatic(string path);
    }
}
