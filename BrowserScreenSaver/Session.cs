using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BrowserScreenSaver
{
    class Session
    {
        public int SessionId { get; set; }
        public string Station { get; set; }

        public NativeMethods.WTS_CONNECTSTATE_CLASS State { get; set; }

        public void Disconnect()
        {
            bool result = NativeMethods.WTSDisconnectSession(NativeMethods.WTS_CURRENT_SERVER_HANDLE, this.SessionId, false);
        }

        public static List<Session> List(string serverName = null)
        {
            var sessions = new List<Session>();

            IntPtr sessionInfoPtr = IntPtr.Zero;
            Int32 sessionCount = 0;
            Int32 retVal = NativeMethods.WTSEnumerateSessions(NativeMethods.WTS_CURRENT_SERVER_HANDLE, 0, 1, ref sessionInfoPtr, ref sessionCount);
            Int32 dataSize = Marshal.SizeOf(typeof(NativeMethods.WTS_SESSION_INFO));

            if (retVal != 0)
            {
                var currentSessionInfoPtr = sessionInfoPtr;
                for (int i = 0; i < sessionCount; i++)
                {
                    NativeMethods.WTS_SESSION_INFO si =
                        (NativeMethods.WTS_SESSION_INFO)Marshal.PtrToStructure(currentSessionInfoPtr, typeof(NativeMethods.WTS_SESSION_INFO));
                    Session session = new Session()
                    {
                        SessionId = si.SessionID,
                        Station = si.pWinStationName,
                        State = si.State,
                    };
                    sessions.Add(session);
                    currentSessionInfoPtr += dataSize;
                }

                NativeMethods.WTSFreeMemory(sessionInfoPtr);
            }

            return sessions;
        }
    }
}
