using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace BrowserScreenSaver.Extensions
{
    public static class WebBrowserExtensions
    {
        public static void SetScrollBarVisibility(this WebBrowser webBrowser, bool isVisible)
        {
            var script = isVisible ? "document.body.style.overflow ='visible'" : "document.body.style.overflow ='hidden'";
            webBrowser.InvokeScript("execScript", script, "JavaScript");
        }

        public static void SetScale(this WebBrowser webBrowser, int scaleLevel)
        {
            if (scaleLevel < 10 || scaleLevel > 1000)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(scaleLevel), message: $"Specified value of '{scaleLevel}' is invalid zoom level for a browser control.");
            }

            var axIWebBrowser2 = (dynamic)webBrowser
                .GetType()
                .GetField(
                    name: "_axIWebBrowser2",
                    bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(webBrowser);

            // Set browser zoom
            axIWebBrowser2?.ExecWB(NativeMethods.OleCommandId.OLECMDID_OPTICAL_ZOOM, NativeMethods.OleCommandExecuteOptions.OLECMDEXECOPT_DONTPROMPTUSER, scaleLevel, IntPtr.Zero);
        }

        public static void SetSilent(this WebBrowser browser, bool silent)
        {
            if (browser == null)
            {
                throw new ArgumentNullException(paramName: nameof(browser));
            }

            // get an IWebBrowser2 from the document
            var serviceProvider = browser.Document as IOleServiceProvider;
            if (serviceProvider != null)
            {
                var IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                var IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser2;
                serviceProvider.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser2);
                webBrowser2?.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser2, new object[] { silent });
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }
}
