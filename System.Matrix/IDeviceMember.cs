using System.Collections.Generic;

namespace System.Matrix
{
    public interface IDeviceMember
    {
        IEntryData EntryData { get; }

        IVectorNetworkAnalyzer VNA { get; set; }
        Matrix Matrix { get; set; }
        Vertex Vertex { get; set; }
        List<Vertex> Vertexs { get; set; }
        CalBoxToMatrix CalBoxToMatrix { get; set; }
        CalBoxToVertex CalBoxToVertex { get; set; }
        CalBoxWhole CalBoxWhole { get; set; }

        SwitchAdapter<ISwitch> SwitchAdapter { get; set; }
    }
}
