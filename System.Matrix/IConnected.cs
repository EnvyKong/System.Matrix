namespace System.Matrix
{
    public interface IConnected
    {
        bool Connected { get; set; }
        string Name { get; }
        void Close();
        void Connect();
    }
}
