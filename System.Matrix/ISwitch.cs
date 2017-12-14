namespace System.Matrix
{
    public interface ISwitch
    {
        void DoSwitch(int aPort, int bPort);
        bool Connected { get; }
        CalBoxData CalBoxData { get; set; }
    }
}
