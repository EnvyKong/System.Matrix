namespace System.Matrix
{
    public class SwitchAdapter : ISwitch
    {
        private ISwitch _aSwitch;
        private ISwitch _bSwitch;

        public SwitchAdapter(ISwitch aSwitch, ISwitch bSwitch)
        {
            _aSwitch = aSwitch;
            _bSwitch = bSwitch;
        }

        public bool Connected => throw new NotImplementedException();

        public CalBoxData CalBoxData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void DoSwitch(int aPort, int bPort)
        {
            if (_aSwitch.Connected & _bSwitch.Connected)
            {
                _aSwitch.DoSwitch(aPort, bPort);
                _bSwitch.DoSwitch(aPort, bPort);
            }
            else if (_aSwitch.Connected & (!_bSwitch.Connected))
            {
                _bSwitch.DoSwitch(aPort, bPort);
            }
            else
            {
                throw new Exception("Switch connect error.");
            }
        }
    }
}
