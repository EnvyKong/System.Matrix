namespace System.Matrix
{
    public interface IDeviceMember
    {
        IEntryData EntryData { get; }

        IVectorNetworkAnalyzer VNA { get; set; }
        ICalibratable Matrix { get; set; }
        ICalibratable[] Vertexs { get; set; }
        ICalibratable CalBoxToMatrix { get; set; }
        ICalibratable CalBoxToVertex { get; set; }

        SwitchAdapter SwitchAdapter { get; set; }
    }
}
