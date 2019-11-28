using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Controls;
using Microsoft.Win32;

namespace BrowserScreenSaver
{
    class DocHostUIHandler : NativeMethods.IDocHostUIHandler
    {
        private const uint E_NOTIMPL = 0x80004001;
        private const uint S_OK = 0;
        private const uint S_FALSE = 1;

        private WebBrowser WebBrowser { get; }
        private bool IsBrowserContextMenuEnabled { get; }
        private NativeMethods.HostUIFlags Flags { get; set; }

        public DocHostUIHandler(WebBrowser webBrowser, bool isBrowserContextMenuEnabled)
        {
            this.WebBrowser = webBrowser;
            this.IsBrowserContextMenuEnabled = isBrowserContextMenuEnabled;
            this.Flags = NativeMethods.HostUIFlags.ENABLE_REDIRECT_NOTIFICATION;
        }

        uint NativeMethods.IDocHostUIHandler.ShowContextMenu(int dwID, NativeMethods.POINT pt, object pcmdtReserved, object pdispReserved)
        {
            return this.IsBrowserContextMenuEnabled ? S_FALSE : S_OK;
        }

        uint NativeMethods.IDocHostUIHandler.GetHostInfo(ref NativeMethods.DOCHOSTUIINFO info)
        {
            info.dwFlags = (int)Flags;
            info.dwDoubleClick = 0;
            return S_OK;
        }

        uint NativeMethods.IDocHostUIHandler.ShowUI(int dwID, object activeObject, object commandTarget, object frame, object doc)
        {
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.HideUI()
        {
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.UpdateUI()
        {
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.EnableModeless(bool fEnable)
        {
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.OnDocWindowActivate(bool fActivate)
        {
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.OnFrameWindowActivate(bool fActivate)
        {
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.ResizeBorder(NativeMethods.COMRECT rect, object doc, bool fFrameWindow)
        {
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.TranslateAccelerator(ref System.Windows.Forms.Message msg, ref Guid group, int nCmdID)
        {
            return S_FALSE;
        }

        uint NativeMethods.IDocHostUIHandler.GetOptionKeyPath(string[] pbstrKey, int dw)
        {
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.GetDropTarget(object pDropTarget, out object ppDropTarget)
        {
            ppDropTarget = null;
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.GetExternal(out object ppDispatch)
        {
            ppDispatch = this.WebBrowser.ObjectForScripting;
            return S_OK;
        }

        uint NativeMethods.IDocHostUIHandler.TranslateUrl(int dwTranslate, string strURLIn, out string pstrURLOut)
        {
            pstrURLOut = null;
            return E_NOTIMPL;
        }

        uint NativeMethods.IDocHostUIHandler.FilterDataObject(System.Runtime.InteropServices.ComTypes.IDataObject pDO, out System.Runtime.InteropServices.ComTypes.IDataObject ppDORet)
        {
            ppDORet = null;
            return E_NOTIMPL;
        }
    }
}
