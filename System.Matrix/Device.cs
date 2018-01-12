using System.Net.Sockets;
using System.Net;
using System.Text;

namespace System.Matrix
{
    public abstract class Device : TcpClient
    {
        public DeviceData DeviceData { get; }

        public const string CALIBRATE_OFFSET_DATA_PATH = "calibrate.txt";

        public string Name { get => GetType().Name; }

        public string BaseName { get => GetType().BaseType.Name; }

        public int APortNum { get => DeviceData.APortNum; }

        public int BPortNum { get => DeviceData.BPortNum; }

        public int APortConnectNum { get => DeviceData.APortConnectNum; }

        public int BPortConnectNum { get => DeviceData.BPortConnectNum; }

        public long Frequency { get => DeviceData.Frequency; }

        public int this[int aPortID, int bPortID]
        {
            get
            {
                if (aPortID > APortNum | bPortID > BPortNum)
                {
                    throw new Exception($"{Name} Signal Path ID Error! ");
                }
                return BPortNum * (aPortID - 1) + bPortID;
            }
        }

        public int SignalPathCount
        {
            get
            {
                return APortNum * BPortNum;
            }
        }

        public virtual string Cmd { get; set; }

        protected Device(DeviceData deviceData)
        {
            DeviceData = deviceData;
        }

        public virtual string IP
        {
            get
            {
                return DeviceData.IP;
            }
        }

        public IPAddress IPAddress
        {
            get
            {
                return IPAddress.Parse(IP);
            }
            private set { }
        }

        public int PortNum { get => DeviceData.PortNum; }

        public virtual void Connect()
        {
            try
            {
                Connect(IPAddress, PortNum);
            }
            catch (Exception)
            {
            }
        }

        public string Send(string cmd)
        {
            var stream = GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(cmd + "\r\n");
            stream.Write(buffer, 0, buffer.Length);
            byte[] bufferResponse = new byte[1024 * 1024];
            int num = stream.Read(bufferResponse, 0, bufferResponse.Length);
            return Encoding.UTF8.GetString(bufferResponse, 0, num);
        }

        public new virtual bool Connected
        {
            get
            {
                return base.Connected;
            }
            set { }
        }

        public new virtual void Close()
        {
            base.Close();
        }
    }
}