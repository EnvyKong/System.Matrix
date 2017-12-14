namespace System.Matrix
{
    public interface IMatrixSystem
    {
        void CalibrateByFile(string path);
        void ConnectAll();
        void DisConnectAll();
        void OutputResult(string savePath);
        void SetValueDynamic(string path);
        void SetValueStatic(string path);
    }
}