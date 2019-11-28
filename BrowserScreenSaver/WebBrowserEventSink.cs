using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using static BrowserScreenSaver.NativeMethods;

namespace BrowserScreenSaver
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof(DWebBrowserEvents2))]
    public class WebBrowserEventSink : DWebBrowserEvents2
    {
        System.Runtime.InteropServices.ComTypes.IConnectionPoint _sinkCP = null;
        int _sinkCookie = int.MaxValue;
        private bool IsNewWindowEnabled { get; }

        public WebBrowserEventSink(bool isNewWindowEnabled)
        {
            this.IsNewWindowEnabled = isNewWindowEnabled;
        }

        public void Connect(WebBrowser webBrowser)
        {
            if (_sinkCookie != int.MaxValue)
                throw new InvalidOperationException();

            var activeXInstance = webBrowser.GetType().InvokeMember("ActiveXInstance",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, webBrowser, new object[] { }); // as SHDocVw.WebBrowser;

            var cpc = (System.Runtime.InteropServices.ComTypes.IConnectionPointContainer)activeXInstance;
            var guid = typeof(DWebBrowserEvents2).GUID;
            System.Runtime.InteropServices.ComTypes.IConnectionPoint _sinkCP;
            cpc.FindConnectionPoint(ref guid, out _sinkCP);
            _sinkCP.Advise(this, out _sinkCookie);
        }

        public void Disconnect()
        {
            if (_sinkCookie == int.MaxValue)
                throw new InvalidOperationException();
            _sinkCP.Unadvise(_sinkCookie);
            _sinkCookie = int.MaxValue;
            _sinkCP = null;
        }

        #region SHDocVw.DWebBrowserEvents2

        public void StatusTextChange(string Text)
        {
        }

        public void ProgressChange(int Progress, int ProgressMax)
        {
        }

        public void CommandStateChange(int Command, bool Enable)
        {
        }

        public void DownloadBegin()
        {
        }

        public void DownloadComplete()
        {
        }

        public void TitleChange(string Text)
        {
        }

        public void PropertyChange(string szProperty)
        {
        }

        public void BeforeNavigate2(object pDisp, ref object URL, ref object Flags, ref object TargetFrameName, ref object PostData, ref object Headers, ref bool Cancel)
        {
        }

        public void NewWindow2(ref object ppDisp, ref bool Cancel)
        {
        }

        public void NavigateComplete2(object pDisp, ref object URL)
        {
        }

        public void DocumentComplete(object pDisp, ref object URL)
        {
        }

        public void OnQuit()
        {
        }

        public void OnVisible(bool Visible)
        {
        }

        public void OnToolBar(bool ToolBar)
        {
        }

        public void OnMenuBar(bool MenuBar)
        {
        }

        public void OnStatusBar(bool StatusBar)
        {
        }

        public void OnFullScreen(bool FullScreen)
        {
        }

        public void OnTheaterMode(bool TheaterMode)
        {
        }

        public void WindowSetResizable(bool Resizable)
        {
        }

        public void WindowSetLeft(int Left)
        {
        }

        public void WindowSetTop(int Top)
        {
        }

        public void WindowSetWidth(int Width)
        {
        }

        public void WindowSetHeight(int Height)
        {
        }

        public void WindowClosing(bool IsChildWindow, ref bool Cancel)
        {
        }

        public void ClientToHostWindow(ref int CX, ref int CY)
        {
        }

        public void SetSecureLockIcon(int SecureLockIcon)
        {
        }

        public void FileDownload(bool ActiveDocument, ref bool Cancel)
        {
        }

        public void NavigateError(object pDisp, ref object URL, ref object Frame, ref object StatusCode, ref bool Cancel)
        {
        }

        public void PrintTemplateInstantiation(object pDisp)
        {
        }

        public void PrintTemplateTeardown(object pDisp)
        {
        }

        public void UpdatePageStatus(object pDisp, ref object nPage, ref object fDone)
        {
        }

        public void PrivacyImpactedStateChange(bool bImpacted)
        {
        }

        public void NewWindow3(ref object ppDisp, ref bool Cancel, uint dwFlags, string bstrUrlContext, string bstrUrl)
        {
        }

        public void SetPhishingFilterStatus(int PhishingFilterStatus)
        {
        }

        public void WindowStateChanged(uint dwWindowStateFlags, uint dwValidFlagsMask)
        {
        }

        public void NewProcess(int lCauseFlag, object pWB2, ref bool Cancel)
        {
        }

        public void ThirdPartyUrlBlocked(ref object URL, uint dwCount)
        {
        }

        public void RedirectXDomainBlocked(object pDisp, ref object StartURL, ref object RedirectURL, ref object Frame, ref object StatusCode)
        {
        }

        public void BeforeScriptExecute(object pDispWindow)
        {
        }

        public void WebWorkerStarted(uint dwUniqueID, string bstrWorkerLabel)
        {
        }

        public void WebWorkerFinsihed(uint dwUniqueID)
        {
        }

        void DWebBrowserEvents2.CommandStateChange(long command, bool enable)
        {
        }

        void DWebBrowserEvents2.DocumentComplete(object pDisp, ref object URL)
        {
        }

        void DWebBrowserEvents2.NewWindow2(ref object pDisp, ref bool cancel)
        {
            cancel = !this.IsNewWindowEnabled;
        }

        #endregion
    }

    //[ComImport, Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E"), TypeLibType(TypeLibTypeFlags.FOleAutomation | TypeLibTypeFlags.FDual | TypeLibTypeFlags.FHidden)]
    //public interface IWebBrowser2
    //{
    //    [DispId(100)]
    //    void GoBack();
    //    [DispId(0x65)]
    //    void GoForward();
    //    [DispId(0x66)]
    //    void GoHome();
    //    [DispId(0x67)]
    //    void GoSearch();
    //    [DispId(0x68)]
    //    void Navigate([In] string Url, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers);
    //    [DispId(-550)]
    //    void Refresh();
    //    [DispId(0x69)]
    //    void Refresh2([In] ref object level);
    //    [DispId(0x6a)]
    //    void Stop();
    //    [DispId(200)]
    //    object Application { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
    //    [DispId(0xc9)]
    //    object Parent { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
    //    [DispId(0xca)]
    //    object Container { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
    //    [DispId(0xcb)]
    //    object Document { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
    //    [DispId(0xcc)]
    //    bool TopLevelContainer { get; }
    //    [DispId(0xcd)]
    //    string Type { get; }
    //    [DispId(0xce)]
    //    int Left { get; set; }
    //    [DispId(0xcf)]
    //    int Top { get; set; }
    //    [DispId(0xd0)]
    //    int Width { get; set; }
    //    [DispId(0xd1)]
    //    int Height { get; set; }
    //    [DispId(210)]
    //    string LocationName { get; }
    //    [DispId(0xd3)]
    //    string LocationURL { get; }
    //    [DispId(0xd4)]
    //    bool Busy { get; }
    //    [DispId(300)]
    //    void Quit();
    //    [DispId(0x12d)]
    //    void ClientToWindow(out int pcx, out int pcy);
    //    [DispId(0x12e)]
    //    void PutProperty([In] string property, [In] object vtValue);
    //    [DispId(0x12f)]
    //    object GetProperty([In] string property);
    //    [DispId(0)]
    //    string Name { get; }
    //    [DispId(-515)]
    //    int HWND { get; }
    //    [DispId(400)]
    //    string FullName { get; }
    //    [DispId(0x191)]
    //    string Path { get; }
    //    [DispId(0x192)]
    //    bool Visible { get; set; }
    //    [DispId(0x193)]
    //    bool StatusBar { get; set; }
    //    [DispId(0x194)]
    //    string StatusText { get; set; }
    //    [DispId(0x195)]
    //    int ToolBar { get; set; }
    //    [DispId(0x196)]
    //    bool MenuBar { get; set; }
    //    [DispId(0x197)]
    //    bool FullScreen { get; set; }
    //    [DispId(500)]
    //    void Navigate2([In] ref object URL, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers);
    //    [DispId(0x1f7)]
    //    void ShowBrowserBar([In] ref object pvaClsid, [In] ref object pvarShow, [In] ref object pvarSize);
    //    [DispId(-525)]
    //    WebBrowserReadyState ReadyState { get; }
    //    [DispId(550)]
    //    bool Offline { get; set; }
    //    [DispId(0x227)]
    //    bool Silent { get; set; }
    //    [DispId(0x228)]
    //    bool RegisterAsBrowser { get; set; }
    //    [DispId(0x229)]
    //    bool RegisterAsDropTarget { get; set; }
    //    [DispId(0x22a)]
    //    bool TheaterMode { get; set; }
    //    [DispId(0x22b)]
    //    bool AddressBar { get; set; }
    //    [DispId(0x22c)]
    //    bool Resizable { get; set; }
    //}
}
