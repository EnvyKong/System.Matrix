using System.Reflection;
using TopYoung.MV.Core;

namespace System.Matrix
{
    public static class VNAFactory
    {
        public static IVectorNetworkAnalyzer GetVNA(IEntryData data)
        {
            var vnaType = Type.GetType($"{MethodBase.GetCurrentMethod().DeclaringType.Namespace}.{AppConfigInfo.VNAType}");
            return Activator.CreateInstance(vnaType, data) as IVectorNetworkAnalyzer;
        }
    }
}
