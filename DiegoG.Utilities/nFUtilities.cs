using System;
using System.Threading;
using Windows.Devices.WiFi;

namespace DiegoG.Utilities
{
    public static class nFUtilities
    {
        /// <summary>
        /// Synchronously scans the available WiFi Networks, and returns the matching WiFiNetworkReport
        /// </summary>
        /// <param name="adap"></param>
        /// <returns></returns>
        public static WiFiNetworkReport ScanWait(this WiFiAdapter adap)
            => adap.ScanWait(Timeout.Infinite);
        /// <summary>
        /// Synchronously scans the available WiFi Networks, and returns the matching WiFiNetworkReport
        /// </summary>
        /// <param name="adap"></param>
        /// <returns></returns>
        public static WiFiNetworkReport ScanWait(this WiFiAdapter adap, int timeoutMs)
        {
            var mre = new ManualResetEvent(false);
            adap.ScanAsync();
            adap.AvailableNetworksChanged += ScanWaitEventHandler;
            mre.WaitOne(timeoutMs, false);
            adap.AvailableNetworksChanged -= ScanWaitEventHandler;
            return adap.NetworkReport;

            void ScanWaitEventHandler(WiFiAdapter sender, object e) => mre.Set();
        }
    }
}
