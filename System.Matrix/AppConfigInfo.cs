using System.Configuration;

namespace TopYoung.MV
{
    public static class AppConfigInfo
    {
        public static int AttMarkPoint { get => int.Parse(ConfigurationManager.AppSettings["Att Mark Point"]); }
        public static int PhaMarkPoint { get => int.Parse(ConfigurationManager.AppSettings["Pha Mark Point"]); }
        public static int MatrixQuantity { get => int.Parse(ConfigurationManager.AppSettings["Matrix Quantity"]); }
        public static int MatrixANum { get => int.Parse(ConfigurationManager.AppSettings["Matrix APort Number"]); }
        public static int MatrixBNum { get => int.Parse(ConfigurationManager.AppSettings["Matrix BPort Number"]); }
        public static int MatrixAConnectNum { get => int.Parse(ConfigurationManager.AppSettings["Matrix APort Connect Number"]); }
        public static int MatrixBConnectNum { get => int.Parse(ConfigurationManager.AppSettings["Matrix BPort Connect Number"]); }
        public static int VertexQuantity { get => int.Parse(ConfigurationManager.AppSettings["Vertex Quantity"]); }
        public static int VertexANum { get => int.Parse(ConfigurationManager.AppSettings["Vertex APort Number"]); }
        public static int VertexBNum { get => int.Parse(ConfigurationManager.AppSettings["Vertex BPort Number"]); }
        public static int VertexAConnectNum { get => int.Parse(ConfigurationManager.AppSettings["Vertex APort Connect Number"]); }
        public static int VertexBConnectNum { get => int.Parse(ConfigurationManager.AppSettings["Vertex BPort Connect Number"]); }
        public static int CalBoxToMatrixQuantity { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToMatrix Quantity"]); }
        public static int CalBoxToMatrixANum { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToMatrix APort Number"]); }
        public static int CalBoxToMatrixBNum { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToMatrix BPort Number"]); }
        public static int CalBoxToMatrixAConnectNum { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToMatrix APort Connect Number"]); }
        public static int CalBoxToMatrixBConnectNum { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToMatrix BPort Connect Number"]); }
        public static int CalBoxToVertexQuantity { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToVertex Quantity"]); }
        public static int CalBoxToVertexANum { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToVertex APort Number"]); }
        public static int CalBoxToVertexBNum { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToVertex BPort Number"]); }
        public static int CalBoxToVertexAConnectNum { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToVertex APort Connect Number"]); }
        public static int CalBoxToVertexBConnectNum { get => int.Parse(ConfigurationManager.AppSettings["CalBoxToVertex BPort Connect Number"]); }
        public static int AttCalFre { get => int.Parse(ConfigurationManager.AppSettings["Attenuation Calibration Frequency"]); }
        public static int PhaCalFre { get => int.Parse(ConfigurationManager.AppSettings["Phase Calibration Frequency"]); }
        public static string VNAType { get => ConfigurationManager.AppSettings["VNA Type"]; }
    }
}