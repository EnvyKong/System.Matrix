using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace System.Matrix
{
    public class CalBoxData
    {
        public List<PortData> APortDataList = new List<PortData>();
        public List<PortData> BPortDataList = new List<PortData>();

        public PortData this[Port port, int id]
        {
            get
            {
                switch (port)
                {
                    case Port.A:
                        return APortDataList[id - 1];
                    case Port.B:
                        return BPortDataList[id - 1];
                    default:
                        return null;
                }
            }
        }
    }

    public class PortData
    {
        public double Attenuation { set; get; }

        private double _phase;
        public double Phase
        {
            get { return _phase; }
            set
            {
                var p = value;
                while (p < 0)
                {
                    p = 360 + p;
                }
                _phase = p % 360;
            }
        }
    }

    public enum Port
    {
        A, B
    }

    public class Scene
    {
        private List<Frame> _frameList;
        public ReadOnlyCollection<Frame> Frames
        {
            get { return _frameList.AsReadOnly(); }
        }

        public void LoadSceneData(string path, Matrix owner)
        {
            _frameList = new List<Frame>();
            var allFrames = File.ReadAllLines(path);
            var frameLength = allFrames[0].Split(',').Length;

            for (int row = 1; row < allFrames.Length; row++)
            {
                var channelToMatrixList = new List<Channel>();
                var frame = new Frame(channelToMatrixList);
                frame.TimeDelay = allFrames[row].Split(',')[0].ToDouble();
                for (int column = 1; column < frameLength; column++)
                {
                    var abPort = allFrames[0].Split(',')[column].Split('A', 'B');
                    var aID = abPort[1].ToInt32();
                    var bID = abPort[2].ToInt32();
                    channelToMatrixList.Add(new Channel(aID, bID)
                    {
                        Owner = owner,
                        PhaOffset = allFrames[row].Split(',')[column].ToDouble()
                    });
                }
                _frameList.Add(frame);
            }
        }
    }

    public class Frame
    {
        public Frame(List<Channel> channelToMatrixList)
        {
            _channelsToMatrix = channelToMatrixList;
        }

        private List<Channel> _channelsToMatrix;
        public ReadOnlyCollection<Channel> ChannelToMatrixCollection
        {
            get { return _channelsToMatrix.AsReadOnly(); }
        }

        public double TimeDelay { get; set; }
    }

    public class Channel
    {
        public Channel(int aPortNum, int bPortNum)
        {
            Index = $"{aPortNum}:{bPortNum}";
            APortID = aPortNum;
            BPortID = bPortNum;
        }

        public string Index { get; }
        public int APortID { get; }
        public int BPortID { get; }

        public Matrix Owner { get; set; }

        public int AttStdCode { get; set; }
        public int PhaStdCode { get; set; }

        public int AttCode
        {
            get
            {
                return Owner.ChannelList.Find(c => c.APortID == APortID & c.BPortID == BPortID).AttStdCode + Math.Round(AttOffset / Owner.AttenuationStep).ToInt32();
            }
        }

        public int PhaCode
        {
            get
            {
                return Owner.ChannelList.Find(c => c.APortID == APortID & c.BPortID == BPortID).PhaStdCode + Math.Round(PhaOffset / Owner.PhaseStep).ToInt32();
            }
        }


        public bool HasAttValue { get; private set; }
        public bool HasPhaValue { get; private set; }

        private double _attOffset;
        public double AttOffset
        {
            get { return _attOffset; }
            set
            {
                _attOffset = value;
                HasAttOffsetValue = true;
            }
        }

        private double _phaOffset;
        public double PhaOffset
        {
            get { return _phaOffset; }
            set
            {
                var p = value;
                while (p < 0)
                {
                    p = 360 + p;
                }
                _phaOffset = p % 360;
                HasPhaOffsetValue = true;
            }
        }

        public bool HasAttOffsetValue { get; private set; }
        public bool HasPhaOffsetValue { get; private set; }
    }

    public enum UpDown
    {
        UP,
        DOWN
    }

    public enum PhaseStepShiftDirection
    {
        Clockwise,
        Anticlockwise,
        Default
    }

    public class SignalPath
    {
        public SignalPath(CalBoxData calBoxData, DeviceData data)
        {
            _calBoxData = calBoxData;
            _data = data;
        }

        private CalBoxData _calBoxData;
        private DeviceData _data;

        public static bool HasAttStandardValue { get; private set; }
        public static bool HasPhaStandardValue { get; private set; }

        private static double _expectAttStandard;
        public static double ExpectAttStandard
        {
            get { return _expectAttStandard; }
            set
            {
                HasAttStandardValue = true;
                _expectAttStandard = value;
            }
        }

        private static double _expectPhaStandard;
        public static double ExpectPhaStandard
        {
            get { return _expectPhaStandard; }
            set
            {
                HasPhaStandardValue = true;
                _expectPhaStandard = value;
            }
        }

        public int ID { get => -1;/*_aPortID + (_bPortID - 1) * 64 + (_cPortID - 1) * 1024;*/  }

        public string Index { get => $"{APortID}:{BPortID}:{CPortID}"; }

        public string SignalPathIDIndex { get => $"A{APortID}B{BPortID}C{BPortID}D{CPortID}"; }

        public int APortID { get; set; }

        public int BPortID { get; set; }

        public int CPortID { get; set; }

        private double _attenuation;
        public double Attenuation
        {
            get { return _attenuation; }
            set
            {
                _attenuation = value - _calBoxData[Port.A, APortID].Attenuation - _calBoxData[Port.B, BPortID].Attenuation;
            }
        }

        private double _phase;
        public double Phase
        {
            get { return _phase; }
            set
            {
                var p = value - _calBoxData[Port.A, APortID].Phase - _calBoxData[Port.B, BPortID].Phase;
                while (p < 0)
                {
                    p = 360 + p;
                }
                _phase = p % 360;
            }
        }

        public Channel ChannelToMatrix
        {
            get
            {
                if (_data.PhaseStepShiftDirection == PhaseStepShiftDirection.Anticlockwise)
                {
                    return new Channel(APortID, BPortID) { AttOffset = Attenuation - ExpectAttStandard, PhaOffset = Phase - ExpectPhaStandard };
                }
                else
                {
                    return new Channel(APortID, BPortID) { AttOffset = Attenuation - ExpectAttStandard, PhaOffset = ExpectPhaStandard - Phase };
                }
            }
        }
    }

    public enum VNAType
    {
        Default,
        Kesight_E5072A,
        Kesight_E5071C,
        Kesight_E5070B,
        Kesight_N522A,
        RS_ZNB8
    }
}