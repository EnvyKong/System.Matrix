using System.Collections.Generic;

namespace System.Matrix
{
    class DeviceAdapter
    {
        public DeviceData DeviceData => throw new NotImplementedException();

        public IVectorNetworkAnalyzer VNA { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Matrix Matrix { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vertex Vertex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<Vertex> VertexList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CalBoxToMatrix CalBoxToMatrix { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CalBoxToVertex CalBoxToVertex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CalBoxWhole CalBoxWhole { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public SwitchAdapter<ISwitch> SwitchAdapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
