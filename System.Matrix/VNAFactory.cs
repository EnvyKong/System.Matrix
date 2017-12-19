using System.Reflection;

namespace System.Matrix
{
    public static class VNAFactory
    {
        public static IVectorNetworkAnalyzer GetVNA(IEntryData data)
        {
            string typeStr = data.GetPropertyValue("VNAType").ToString();
            if (typeStr.Contains("E5072A") | typeStr.Contains("E5071C") | typeStr.Contains("E5070B"))
            {
                typeStr = "VNAE5061B";
            }
            else if (typeStr.Contains("RSZVB8"))
            {
                typeStr = "VNARSZVB8";
            }
            else if (typeStr.Contains("N5225A"))
            {
                typeStr = "VNAN5225A";
            }
            else
            {
                throw new NotImplementedException("VNA Type error.");
            }
            var vnaType = Type.GetType($"{MethodBase.GetCurrentMethod().DeclaringType.Namespace}.{typeStr}");
            return Activator.CreateInstance(vnaType, data) as IVectorNetworkAnalyzer;
        }
    }
}
