using System.Collections.Generic;
using System.Net;

namespace System.Matrix
{
    public interface ICalibratable : IConnected
    {
        int this[int a, int b] { get; }
        //int Quantity { get; }
        int APortNum { get; }
        int BPortNum { get; }
        int APortConnectNum { get; }
        int BPortConnectNum { get; }
        int SignalPathCount { get; }
        string Cmd { get; set; }
        List<string> IP { get; }
        List<IPAddress> IPAddress { get; }
        int PortNum { get; }
    }
}
