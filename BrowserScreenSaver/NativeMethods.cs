﻿using System;
using System.Runtime.InteropServices;

namespace BrowserScreenSaver
{
    public static class NativeMethods
    {
        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive, WTSConnected, WTSConnectQuery, WTSShadow, WTSDisconnected, WTSIdle, WTSListen, WTSReset, WTSDown, WTSInit
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WTS_SESSION_INFO
        {
            public Int32 SessionID;
            [MarshalAs(UnmanagedType.LPStr)]
            public String pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        [DllImport("wtsapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr WTSOpenServer(string pServerName);

        [DllImport("wtsapi32.dll")]
        public static extern void WTSCloseServer(IntPtr hServer);

        public static IntPtr WTS_CURRENT_SERVER_HANDLE = (IntPtr)null;

        [DllImport("wtsapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WTSDisconnectSession(
            IntPtr hServer,
            int sessionId,
            [MarshalAs(UnmanagedType.Bool)] bool bWait);

        [DllImport("wtsapi32.dll")]
        public static extern Int32 WTSEnumerateSessions(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] Int32 reserved,
            [MarshalAs(UnmanagedType.U4)] Int32 version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

        [DllImport("wtsapi32.dll")]
        public static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, ref Rect lpRect);


        //[Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public abstract class WindowStyles
        {
            public const uint WS_OVERLAPPED = 0x00000000;
            public const uint WS_POPUP = 0x80000000;
            public const uint WS_CHILD = 0x40000000;
            public const uint WS_MINIMIZE = 0x20000000;
            public const uint WS_VISIBLE = 0x10000000;
            public const uint WS_DISABLED = 0x08000000;
            public const uint WS_CLIPSIBLINGS = 0x04000000;
            public const uint WS_CLIPCHILDREN = 0x02000000;
            public const uint WS_MAXIMIZE = 0x01000000;
            public const uint WS_CAPTION = 0x00C00000; /* WS_BORDER | WS_DLGFRAME  */
            public const uint WS_BORDER = 0x00800000;
            public const uint WS_DLGFRAME = 0x00400000;
            public const uint WS_VSCROLL = 0x00200000;
            public const uint WS_HSCROLL = 0x00100000;
            public const uint WS_SYSMENU = 0x00080000;
            public const uint WS_THICKFRAME = 0x00040000;
            public const uint WS_GROUP = 0x00020000;
            public const uint WS_TABSTOP = 0x00010000;

            public const uint WS_MINIMIZEBOX = 0x00020000;
            public const uint WS_MAXIMIZEBOX = 0x00010000;

            public const uint WS_TILED = WS_OVERLAPPED;
            public const uint WS_ICONIC = WS_MINIMIZE;
            public const uint WS_SIZEBOX = WS_THICKFRAME;
            public const uint WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;

            // Common Window Styles

            public const uint WS_OVERLAPPEDWINDOW =
                (WS_OVERLAPPED |
                 WS_CAPTION |
                 WS_SYSMENU |
                 WS_THICKFRAME |
                 WS_MINIMIZEBOX |
                 WS_MAXIMIZEBOX);

            public const uint WS_POPUPWINDOW =
                (WS_POPUP |
                 WS_BORDER |
                 WS_SYSMENU);

            public const uint WS_CHILDWINDOW = WS_CHILD;

            //Extended Window Styles

            public const uint WS_EX_DLGMODALFRAME = 0x00000001;
            public const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
            public const uint WS_EX_TOPMOST = 0x00000008;
            public const uint WS_EX_ACCEPTFILES = 0x00000010;
            public const uint WS_EX_TRANSPARENT = 0x00000020;

            //#if(WINVER >= 0x0400)
            public const uint WS_EX_MDICHILD = 0x00000040;
            public const uint WS_EX_TOOLWINDOW = 0x00000080;
            public const uint WS_EX_WINDOWEDGE = 0x00000100;
            public const uint WS_EX_CLIENTEDGE = 0x00000200;
            public const uint WS_EX_CONTEXTHELP = 0x00000400;

            public const uint WS_EX_RIGHT = 0x00001000;
            public const uint WS_EX_LEFT = 0x00000000;
            public const uint WS_EX_RTLREADING = 0x00002000;
            public const uint WS_EX_LTRREADING = 0x00000000;
            public const uint WS_EX_LEFTSCROLLBAR = 0x00004000;
            public const uint WS_EX_RIGHTSCROLLBAR = 0x00000000;

            public const uint WS_EX_CONTROLPARENT = 0x00010000;
            public const uint WS_EX_STATICEDGE = 0x00020000;
            public const uint WS_EX_APPWINDOW = 0x00040000;

            public const uint WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
            public const uint WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
            //#endif /* WINVER >= 0x0400 */

            //#if(_WIN32_WINNT >= 0x0500)
            public const uint WS_EX_LAYERED = 0x00080000;
            //#endif /* _WIN32_WINNT >= 0x0500 */

            //#if(WINVER >= 0x0500)
            public const uint WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
            public const uint WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring
            //#endif /* WINVER >= 0x0500 */

            //#if(_WIN32_WINNT >= 0x0500)
            public const uint WS_EX_COMPOSITED = 0x02000000;
            public const uint WS_EX_NOACTIVATE = 0x08000000;
            //#endif /* _WIN32_WINNT >= 0x0500 */
        }

        public enum OleCommandId
        {
            OLECMDID_OPTICAL_ZOOM = 63,
        }

        public enum OleCommandExecuteOptions
        {
            OLECMDEXECOPT_DONTPROMPTUSER = 2,
        }

        [ComImport, Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDocHostUIHandler
        {
            [PreserveSig]
            uint ShowContextMenu(int dwID, POINT pt, [MarshalAs(UnmanagedType.Interface)] object pcmdtReserved, [MarshalAs(UnmanagedType.Interface)] object pdispReserved);

            [PreserveSig]
            uint GetHostInfo(ref DOCHOSTUIINFO info);

            [PreserveSig]
            uint ShowUI(int dwID, [MarshalAs(UnmanagedType.Interface)] object activeObject, [MarshalAs(UnmanagedType.Interface)] object commandTarget, [MarshalAs(UnmanagedType.Interface)] object frame, [MarshalAs(UnmanagedType.Interface)] object doc);

            [PreserveSig]
            uint HideUI();

            [PreserveSig]
            uint UpdateUI();

            [PreserveSig]
            uint EnableModeless(bool fEnable);

            [PreserveSig]
            uint OnDocWindowActivate(bool fActivate);

            [PreserveSig]
            uint OnFrameWindowActivate(bool fActivate);

            [PreserveSig]
            uint ResizeBorder(COMRECT rect, [MarshalAs(UnmanagedType.Interface)] object doc, bool fFrameWindow);

            [PreserveSig]
            uint TranslateAccelerator(ref System.Windows.Forms.Message msg, ref Guid group, int nCmdID);

            [PreserveSig]
            uint GetOptionKeyPath([Out, MarshalAs(UnmanagedType.LPArray)] string[] pbstrKey, int dw);

            [PreserveSig]
            uint GetDropTarget([In, MarshalAs(UnmanagedType.Interface)] object pDropTarget, [MarshalAs(UnmanagedType.Interface)] out object ppDropTarget);

            [PreserveSig]
            uint GetExternal([MarshalAs(UnmanagedType.IDispatch)] out object ppDispatch);

            [PreserveSig]
            uint TranslateUrl(int dwTranslate, [MarshalAs(UnmanagedType.LPWStr)] string strURLIn, [MarshalAs(UnmanagedType.LPWStr)] out string pstrURLOut);

            [PreserveSig]
            uint FilterDataObject(System.Runtime.InteropServices.ComTypes.IDataObject pDO, out System.Runtime.InteropServices.ComTypes.IDataObject ppDORet);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DOCHOSTUIINFO
        {
            public int cbSize;
            public int dwFlags;
            public int dwDoubleClick;
            public IntPtr dwReserved1;
            public IntPtr dwReserved2;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COMRECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class POINT
        {
            public int x;
            public int y;
        }

        [ComImport, Guid("3050F3F0-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICustomDoc
        {
            [PreserveSig]
            int SetUIHandler(IDocHostUIHandler pUIHandler);
        }

        [Flags]
        public enum HostUIFlags
        {
            DIALOG = 0x00000001,
            DISABLE_HELP_MENU = 0x00000002,
            NO3DBORDER = 0x00000004,
            SCROLL_NO = 0x00000008,
            DISABLE_SCRIPT_INACTIVE = 0x00000010,
            OPENNEWWIN = 0x00000020,
            DISABLE_OFFSCREEN = 0x00000040,
            FLAT_SCROLLBAR = 0x00000080,
            DIV_BLOCKDEFAULT = 0x00000100,
            ACTIVATE_CLIENTHIT_ONLY = 0x00000200,
            OVERRIDEBEHAVIORFACTORY = 0x00000400,
            CODEPAGELINKEDFONTS = 0x00000800,
            URL_ENCODING_DISABLE_UTF8 = 0x00001000,
            URL_ENCODING_ENABLE_UTF8 = 0x00002000,
            ENABLE_FORMS_AUTOCOMPLETE = 0x00004000,
            ENABLE_INPLACE_NAVIGATION = 0x00010000,
            IME_ENABLE_RECONVERSION = 0x00020000,
            THEME = 0x00040000,
            NOTHEME = 0x00080000,
            NOPICS = 0x00100000,
            NO3DOUTERBORDER = 0x00200000,
            DISABLE_EDIT_NS_FIXUP = 0x00400000,
            LOCAL_MACHINE_ACCESS_CHECK = 0x00800000,
            DISABLE_UNTRUSTEDPROTOCOL = 0x01000000,
            HOST_NAVIGATES = 0x02000000,
            ENABLE_REDIRECT_NOTIFICATION = 0x04000000,
            USE_WINDOWLESS_SELECTCONTROL = 0x08000000,
            USE_WINDOWED_SELECTCONTROL = 0x10000000,
            ENABLE_ACTIVEX_INACTIVATE_MODE = 0x20000000,
            DPI_AWARE = 0x40000000
        }

        [ComImport, Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden)]
        public interface DWebBrowserEvents2
        {
            [DispId(0x69)]
            void CommandStateChange([In] long command, [In] bool enable);
            [DispId(0x103)]
            void DocumentComplete([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In] ref object URL);
            [DispId(0xfb)]
            void NewWindow2([In, Out, MarshalAs(UnmanagedType.IDispatch)] ref object pDisp, [In, Out] ref bool cancel);
        }
    }
}