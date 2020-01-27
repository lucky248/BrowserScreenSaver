using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace BrowserScreenSaver.Extensions
{
    internal static class DispatcherExtensions
    {
        public static void DelayInvoke (this Dispatcher dispatcher, TimeSpan startDelay, Action action)
        {
            var dt = new DispatcherTimer(DispatcherPriority.Send);
            dt.Tick += delegate
            {
                dt.Stop();
                dispatcher.Invoke(action);
            };
            dt.Interval = startDelay;
            dt.Start();
        }
    }
}
