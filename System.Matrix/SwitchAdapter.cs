namespace System.Matrix
{
    public class SwitchAdapter<TSwitch> : ISwitch where TSwitch : ISwitch
    {
        private TSwitch _calBoxToMatrix;
        private TSwitch _calBoxToVertex;
        private TSwitch _calBoxWhole;

        public SwitchAdapter(TSwitch calBoxToMatrix, TSwitch calBoxToVertex, TSwitch calBoxWhole)
        {
            _calBoxToMatrix = calBoxToMatrix;
            _calBoxToVertex = calBoxToVertex;
            _calBoxWhole = calBoxWhole;
        }

        public bool Connected => throw new NotImplementedException();

        public CalBoxData CalBoxData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public CalBoxData GetCalBoxData()
        {
            if (_calBoxToMatrix.Connected & _calBoxToVertex.Connected)
            {
                _calBoxToMatrix.GetCalBoxData();
                _calBoxToVertex.GetCalBoxData();

                return CalBoxData;
            }
            else if (_calBoxWhole.Connected)
            {
                CalBoxData = _calBoxWhole.GetCalBoxData();

                return CalBoxData;
            }
            else
            {
                throw new Exception("Switch connect error.");
            }
        }
    }
}
