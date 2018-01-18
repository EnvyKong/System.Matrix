using System.Collections.Generic;
using System.Linq;

namespace System.Matrix
{
    public abstract class MatrixSystem
    {
        public MatrixSystem(params DeviceData[] deviceDatas)
        {
            _deviceDatas = deviceDatas.ToList();
            Initialize();
        }

        protected readonly List<DeviceData> _deviceDatas;

        protected IVectorNetworkAnalyzer VNA { get; set; }
        protected Matrix Matrix { get; set; }
        protected List<Vertex> Vertexs { get; set; }
        protected CalBoxToMatrix CalBoxToMatrix { get; set; }
        protected CalBoxToVertex CalBoxToVertex { get; set; }
        protected CalBoxWhole CalBoxWhole { get; set; }
        protected SwitchAdapter<ISwitch> SwitchAdapter { get; set; }

        protected virtual void Initialize()
        {
            Matrix = new Matrix(_deviceDatas.Find(d => d.TypeName.ToLower().Contains("matrix")));
            Vertexs = new List<Vertex>();
            var vertexDatas = _deviceDatas.FindAll(d => d.TypeName.ToLower().Contains("vertex"));
            foreach (var vertexData in vertexDatas)
            {
                Vertexs.Add(new Vertex(vertexData));
            }
            CalBoxToMatrix = new CalBoxToMatrix(_deviceDatas.Find(d => d.TypeName.ToLower().Contains("calboxtomatrix")));
            CalBoxToVertex = new CalBoxToVertex(_deviceDatas.Find(d => d.TypeName.ToLower().Contains("calboxtovertex")));
            CalBoxWhole = new CalBoxWhole(_deviceDatas.Find(d => d.TypeName.ToLower().Contains("calboxwhole")));
            VNA = VNAFactory.GetVNA(_deviceDatas.Find(d => d.TypeName.ToLower().Contains("vna")));
            SwitchAdapter = new SwitchAdapter<ISwitch>(CalBoxToMatrix, CalBoxToVertex, CalBoxWhole);
        }

        public abstract void Calibrate();
        public abstract void CalibrateByFile(string path);
        public abstract void ConnectAll();
        public abstract void DisConnectAll();
        public abstract void OutputResult(string savePath);
        public abstract void SetValueDynamic(string path);
        public abstract void SetValueStatic(string path);
    }
}
