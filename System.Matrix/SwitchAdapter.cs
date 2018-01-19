namespace System.Matrix
{
    public class SwitchAdapter<T> : ISwitch where T : ISwitch
    {
        private readonly T _calBoxToMatrix;
        private readonly T _calBoxToVertex;
        private readonly T _calBoxWhole;

        public SwitchAdapter(T calBoxToMatrix, T calBoxToVertex, T calBoxWhole)
        {
            _calBoxToMatrix = calBoxToMatrix;
            _calBoxToVertex = calBoxToVertex;
            _calBoxWhole = calBoxWhole;
        }

        public bool Connected
        {
            get
            {
                if (_calBoxToMatrix.Connected & _calBoxToVertex.Connected)
                {
                    return true;
                }
                else if (_calBoxWhole.Connected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public CalBoxData CalBoxData { get; set; }

        public void DoSwitch(int aPort, int bPort)
        {
            if (_calBoxToMatrix.Connected & _calBoxToVertex.Connected)
            {
                _calBoxToMatrix.DoSwitch(aPort, bPort);
                _calBoxToVertex.DoSwitch(aPort, bPort);
            }
            else if (_calBoxWhole.Connected)
            {
                _calBoxWhole.DoSwitch(aPort, bPort);
            }
            else
            {
                throw new Exception("Switch connect error.");
            }
        }

        public void GetCalBoxData()
        {
            CalBoxData = new CalBoxData();
            if (_calBoxToMatrix.Connected & _calBoxToVertex.Connected)
            {
                _calBoxToMatrix.GetCalBoxData();
                _calBoxToVertex.GetCalBoxData();
            }
            else if (_calBoxWhole.Connected)
            {
                _calBoxWhole.GetCalBoxData();
                CalBoxData = _calBoxWhole.CalBoxData;
            }
            else
            {
                throw new Exception("Switch connect error.");
            }
        }
    }
}
