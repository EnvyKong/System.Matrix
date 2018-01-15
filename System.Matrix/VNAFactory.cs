using System.Reflection;

namespace System.Matrix
{
    public static class VNAFactory
    {
        public static IVectorNetworkAnalyzer GetVNA(DeviceData deviceData)
        {
            string typeStr = deviceData.VNAType.ToString();
            if (typeStr.Contains("E5072A") | typeStr.Contains("E5071C") | typeStr.Contains("E5070B"))
            {
                typeStr = "VNAE5061B";
            }
            else if (typeStr.Contains("ZNB8"))
            {
                typeStr = "VNARSZVB8";
            }
            else if (typeStr.Contains("N5225A"))
            {
                typeStr = "VNAN5225A";
            }
            else
            {
                throw new NotImplementedException("VNA type error.");
            }
            var vnaType = Type.GetType($"{MethodBase.GetCurrentMethod().DeclaringType.Namespace}.{typeStr}");
            return Activator.CreateInstance(vnaType, deviceData) as IVectorNetworkAnalyzer;
        }
    }
}
