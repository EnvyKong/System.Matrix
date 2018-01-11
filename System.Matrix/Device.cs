using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace System.Matrix
{
    public abstract class Device : TcpClient, ICalibratable
    {
        //public IEntryData EntryData { get; }
        protected DeviceData _deviceData;

        public const string CALIBRATE_OFFSET_DATA_PATH = "calibrate.txt";

        public string Name { get => GetType().Name; }

        public string BaseName { get => GetType().BaseType.Name; }

        //public int APortNum { get => (int)EntryData.GetPropertyValue($"{Name}ANum"); }

        //public int BPortNum { get => (int)EntryData.GetPropertyValue($"{Name}BNum"); }

        //public int APortConnectNum { get => (int)EntryData.GetPropertyValue($"{Name}AConnectNum"); }

        //public int BPortConnectNum { get => (int)EntryData.GetPropertyValue($"{Name}BConnectNum"); }

        public int APortNum { get => _deviceData.APortNum; }
        public int BPortNum { get => _deviceData.BPortNum; }
        public int APortConnectNum { get => _deviceData.APortConnectNum; }
        public int BPortConnectNum { get => _deviceData.BPortConnectNum; }

        public long Frequency { get => _deviceData.Frequency; }

        //public long Frequency { get => (long)EntryData.GetPropertyValue("Frequency"); }

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

        //protected Device(IEntryData data)
        //{
        //    EntryData = data;
        //}

        protected Device(DeviceData data)
        {
            _deviceData = data;
        }

        //public virtual string IP
        //{
        //    get
        //    {
        //        return EntryData.GetPropertyValue($"IPTo{Name}").ToString();
        //    }
        //}

        public virtual string IP
        {
            get
            {
                return _deviceData.IP;
            }
        }

        //public string VNAIP
        //{
        //    get
        //    {
        //        return (string)_deviceData.GetPropertyValue("VNAIP");
        //    }
        //}

        public IPAddress IPAddress
        {
            get
            {
                return IPAddress.Parse(IP);
            }
            private set { }
        }

        //public int PortNum { get => (int)EntryData.GetPropertyValue($"PortNumTo{Name}"); }

        public int PortNum { get => _deviceData.PortNum; }

        public virtual void Connect()
        {
            Connect(IPAddress, PortNum);
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