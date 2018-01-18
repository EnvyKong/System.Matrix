using Ivi.Visa.Interop;
using System.Windows.Forms;

namespace System.Matrix
{
    class VNARSZVB8 : VNA, IVectorNetworkAnalyzer
    {
        public VNARSZVB8(DeviceData deviceData) : base(deviceData)
        {

        }

        public override bool Connected { get; set; }

        public int PhaMarkPoint => _deviceData.PhaMarkPoint;

        public int AttMarkPoint => _deviceData.AttMarkPoint;

        public override void Connect()
        {
            try
            {
                messageBased = new FormattedIO488();
                ResourceManager grm = new ResourceManager();//TCPIP0::192.168.8.219::inst0::INSTR
                messageBased.IO = (IMessage)grm.Open("TCPIP0::" + IP + "::inst0::INSTR", AccessMode.NO_LOCK, 2000, "");
                messageBased.IO.Timeout = 200000;
                messageBased.IO.SendEndEnabled = !messageBased.IO.SendEndEnabled;
                messageBased.IO.TerminationCharacterEnabled = !messageBased.IO.TerminationCharacterEnabled;
                if (ReadIDN(out string IDN))
                {
                    Connected = true;
                }
                else
                {
                    Connected = false;
                }
            }
            catch (Exception ex)
            {
                Connected = false;
                Close();
                MessageBox.Show(ex.ToString());
            }
        }

        public void ActiveSegmentFreq()
        {
            throw new NotImplementedException();
        }

        public double GetFreqMax()
        {
            throw new NotImplementedException();
        }

        public double GetFreqMin()
        {
            throw new NotImplementedException();
        }

        public double GetMarkerY(int trace)
        {
            string strCmd = "";
            string sY;
            double dY;
            try
            {
                SelTrace("Trc" + trace);
                SetSingleSweepMode();
                strCmd = string.Format("CALC:MARK:Y?", trace);
                sY = QueryString(strCmd);
                dY = Convert.ToDouble(sY.Split(',')[0]);
                return dY;
            }
            catch
            {
                Connected = false;
            }
            return 0;
        }

        private void SelTrace(string trace)
        {
            string strCmd = "CALC1:PAR:SEL '" + trace + "'";
            messageBased.WriteString(strCmd);
        }

        public double GetMarkerY()
        {
            throw new NotImplementedException();
        }

        public double[] GetMarkerY(double[] dy)
        {
            throw new NotImplementedException();
        }

        public void LoadFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public double[] ReadFrq()
        {
            throw new NotImplementedException();
        }

        public string ReadIFBW(string channel)
        {
            throw new NotImplementedException();
        }

        public string ReadPower(string channel)
        {
            throw new NotImplementedException();
        }

        public string[] ReadStimulus()
        {
            throw new NotImplementedException();
        }

        public string[] ReadTraces(string trace, string format)
        {
            throw new NotImplementedException();
        }

        public void ReSetAnalyzer()
        {
            throw new NotImplementedException();
        }

        public void SelectFormat(string format)
        {
            throw new NotImplementedException();
        }

        public void SetAGC_Auto()
        {
            throw new NotImplementedException();
        }

        public void SetAGC_LNO()
        {
            throw new NotImplementedException();
        }

        public void SetAGC_MANual()
        {
            throw new NotImplementedException();
        }

        public void SetIFBW(int IfbwValue)
        {
            throw new NotImplementedException();
        }

        public void SetMarkerActive()
        {
            //激活后自动display marker点
            try
            {
                string strCmd = "CALC:MARK:ON";
                Write(strCmd);
            }
            catch
            {
                Connected = false;
            }
        }

        public void SetMarkerState(bool display)
        {
            throw new NotImplementedException();
        }

        public void SetMarkerX(int trace, long x)
        {
            throw new NotImplementedException();
        }

        public void SetMarkerX(long x)
        {
            string strCmd = "";
            try
            {
                strCmd = "CALC:MARK:X " + x.ToString();
                Write(strCmd);
            }
            catch
            {
                Connected = false;
            }
        }

        public void SetPower(string power)
        {
            throw new NotImplementedException();
        }

        public void SetSegmentFreqIns(string StartFreq, string StopFreq, int Points, string Power, string SegmentTime, string Unused, string MeasBandwidth)
        {
            throw new NotImplementedException();
        }

        public void SetSegmentPoint(int Points)
        {
            throw new NotImplementedException();
        }

        public string SetSingleSweepMode()
        {
            if (messageBased != null)
            {
                string strCmd = "INIT:CONT OFF; :INIT; *OPC?";
                messageBased.WriteString(strCmd);
                string value = messageBased.ReadString();
                return value;
            }
            return null;
        }

        public void SetStartFreq(string freq)
        {
            throw new NotImplementedException();
        }

        public void SetStopFreq(string freq)
        {
            throw new NotImplementedException();
        }

        public void SetTrace(string trace, string sParameter)
        {
            throw new NotImplementedException();
        }

        public void SetTraceNumber(int channel, int traceNum)
        {
            throw new NotImplementedException();
        }

        public void StoreFile(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
