using System.Threading;
using System.Globalization;

namespace System.Matrix
{
    public static class ThreadHelper<TDevice> where TDevice : class
    {
        private static Thread _thread;

        public static void Start(Action action)
        {

        }

        public static void Init(string name, CultureInfo cultureInfo, bool isBackground, ThreadStart action)
        {
            _thread = new Thread(action)
            {
                Name = name,
                CurrentCulture = cultureInfo,
                IsBackground = isBackground
            };
        }

        public static void Join()
        {
        }
    }
}
