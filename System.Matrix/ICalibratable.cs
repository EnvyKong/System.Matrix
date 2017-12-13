using System.Net;

namespace System.Matrix
{
    interface ICalibratable
    {
        int this[int a, int b] { get; }
        string Name { get; }
        int Quantity { get; }
        int APortNum { get; }
        int BPortNum { get; }
        int APortConnectNum { get; }
        int BPortConnectNum { get; }
        int SignalPathCount { get; }
        string Cmd { get; set; }
        string IP { get; }
        IPAddress IPAddress { get; }
        int PortNum { get; }
        void Connect();
        string Send(string cmd);
        bool Connected { get; set; }
        void Close();
    }
}
