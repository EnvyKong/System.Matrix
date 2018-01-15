using System.Threading;

namespace System.Matrix
{
    public static class MVExtension
    {
        public static void WaitCompleted(this string response, Func<string, string> func, string cmd)
        {
            while (true)
            {
                if (response.Contains("Vertex>"))
                {
                    break;
                }
                Thread.Sleep(1);
                response = func(cmd);
            }
        }
    }
}
