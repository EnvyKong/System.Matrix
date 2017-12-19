using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace System.Matrix
{
    public abstract class Device : TcpClient, ICalibratable
    {
        public IEntryData EntryData { get; }

        public const string CALIBRATE_OFFSET_DATA_PATH = "calibrate.txt";

        public string Name { get => GetType().Name; }

        public string BaseName { get => GetType().BaseType.Name; }

        public int APortNum { get => (int)EntryData.GetPropertyValue($"{Name}ANum"); }

        public int BPortNum { get => (int)EntryData.GetPropertyValue($"{Name}BNum"); }

        public int APortConnectNum { get => (int)EntryData.GetPropertyValue($"{Name}AConnectNum"); }

        public int BPortConnectNum { get => (int)EntryData.GetPropertyValue($"{Name}BConnectNum"); }

        public long Frequency { get => (long)EntryData.GetPropertyValue("Frequency"); }

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

        protected Device(IEntryData data)
        {
            EntryData = data;
        }

        //private string _ip;
        public virtual List<string> IP
        {
            get
            {
                return (List<string>)EntryData.GetPropertyValue($"IPTo{Name}");
            }
            //private set
            //{
            //    if (IPAddress.TryParse(value, out IPAddress address))
            //    {
            //        IPAddress = address;
            //        _ip = address.ToString();
            //    }
            //    else
            //    {
            //        MessageBox.Show($"{Name} IP Formal Error!");
            //        Log.log.ErrorFormat("{0} IP Formal Error!", Name);
            //    }
            //    Ping ping = new Ping();
            //    var pingReply = ping.Send(address);
            //    if (pingReply.Status != IPStatus.Success)
            //    {
            //        MessageBox.Show($"{Name} IP Ping Error!");
            //        Log.log.ErrorFormat("{0} IP Ping Error!", Name);
            //    }
            //}
        }

        public string VNAIP
        {
            get
            {
                return (string)EntryData.GetPropertyValue($"IPTo{BaseName}");
            }
            //private set
            //{
            //    if (IPAddress.TryParse(value, out IPAddress address))
            //    {
            //        IPAddress = address;
            //        _ip = address.ToString();
            //    }
            //    else
            //    {
            //        MessageBox.Show($"{Name} IP Formal Error!");
            //        Log.log.ErrorFormat("{0} IP Formal Error!", Name);
            //    }
            //    Ping ping = new Ping();
            //    var pingReply = ping.Send(address);
            //    if (pingReply.Status != IPStatus.Success)
            //    {
            //        MessageBox.Show($"{Name} IP Ping Error!");
            //        Log.log.ErrorFormat("{0} IP Ping Error!", Name);
            //    }
            //}
        }

        public List<IPAddress> IPAddress
        {
            get
            {
                return IP.Select(i => Net.IPAddress.Parse(i)).ToList();
            }
            private set { }
        }

        public int PortNum { get => (int)EntryData.GetPropertyValue($"PortNumTo{Name}"); }

        public virtual void Connect()
        {
            foreach (var ip in IPAddress)
            {
                Connect(ip, PortNum);
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