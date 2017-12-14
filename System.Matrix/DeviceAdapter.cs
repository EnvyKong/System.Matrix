namespace System.Matrix
{
    class DeviceAdapter : IDeviceMember
    {
        public IVectorNetworkAnalyzer VNA { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IEntryData EntryData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICalibratable Matrix { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICalibratable[] Vertexs { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICalibratable CalBoxToMatrix { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICalibratable CalBoxToVertex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public SwitchAdapter SwitchAdapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
