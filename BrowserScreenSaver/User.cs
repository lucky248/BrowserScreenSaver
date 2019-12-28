using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace BrowserScreenSaver
{
    public static class User
    {
        public static string ConfirmCurrentUserCredentials(Window parentWindow, string caption, string message)
        {
            string userName;
            string password;
            string domainName;

            CREDUI_INFO info = new CREDUI_INFO();
            info.pszCaptionText = caption;
            info.pszMessageText = message;

            CREDUI_FLAGS flags = CREDUI_FLAGS.GENERIC_CREDENTIALS
                                            | CREDUI_FLAGS.ALWAYS_SHOW_UI
                                            | CREDUI_FLAGS.DO_NOT_PERSIST
                                            ////| CREDUI_FLAGS.VALIDATE_USERNAME
                                            | CREDUI_FLAGS.KEEP_USERNAME
                                            | CREDUI_FLAGS.PASSWORD_ONLY_OK; // Populate the combo box with the password only. Do not allow a user name to be entered.
                                            ////| CREDUI_FLAGS.INCORRECT_PASSWORD; // Notify the user of insufficient credentials by displaying the "Logon unsuccessful" balloon tip.

            var targetName = Environment.MachineName;
            var saveSettings = false;
            var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent();
            CredUIReturnCodes result = PromptForCredentials(
                parentWindow, 
                ref info, 
                targetName, 
                0, 
                initialUserNameValue: currentUser.Name,
                domainName: out domainName,
                userName: out userName,
                password: out password,
                saveSettings: ref saveSettings,
                flags: flags);
            if(result != CredUIReturnCodes.NO_ERROR)
            {
                return $"Failed to get user credentials. Error code: {result}.";
            }

            //var principal = new System.Security.Principal.WindowsPrincipal(currentUser);
            //isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

            var logonResult = CheckUserLogon(username: userName, password: password, domain_fqdn: domainName);
            if (logonResult != 0)
            {
                return $"Failed to login user. Error code: {logonResult.ToString()}.";
            }

            return null; // Success
        }

        #region CredUI Win32 methods

        [Flags]
        private enum CREDUI_FLAGS
        {
            INCORRECT_PASSWORD = 0x1, // Notify the user of insufficient credentials by displaying the "Logon unsuccessful" balloon tip.
            DO_NOT_PERSIST = 0x2,
            REQUEST_ADMINISTRATOR = 0x4,
            EXCLUDE_CERTIFICATES = 0x8,
            REQUIRE_CERTIFICATE = 0x10,
            SHOW_SAVE_CHECK_BOX = 0x40,
            ALWAYS_SHOW_UI = 0x80,
            REQUIRE_SMARTCARD = 0x100,
            PASSWORD_ONLY_OK = 0x200,
            VALIDATE_USERNAME = 0x400,
            COMPLETE_USERNAME = 0x800,
            PERSIST = 0x1000,
            SERVER_CREDENTIAL = 0x4000,
            EXPECT_CONFIRMATION = 0x20000,
            GENERIC_CREDENTIALS = 0x40000,
            USERNAME_TARGET_CREDENTIALS = 0x80000,
            KEEP_USERNAME = 0x100000,
        }


        public enum CredUIReturnCodes
        {
            NO_ERROR = 0,
            ERROR_CANCELLED = 1223,
            ERROR_NO_SUCH_LOGON_SESSION = 1312,
            ERROR_NOT_FOUND = 1168,
            ERROR_INVALID_ACCOUNT_NAME = 1315,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_INVALID_FLAGS = 1004,
            ERROR_BAD_ARGUMENTS = 160
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        [DllImport("credui", EntryPoint = "CredUIPromptForCredentialsW", CharSet = CharSet.Unicode)]
        private static extern CredUIReturnCodes CredUIPromptForCredentials(ref CREDUI_INFO creditUR, string targetName, IntPtr reserved1, int iError, StringBuilder userName, int maxUserName, StringBuilder password, int maxPassword, [MarshalAs(UnmanagedType.Bool)] ref bool pfSave, CREDUI_FLAGS flags);

        [DllImport("credui", EntryPoint = "CredUIParseUserNameW", CharSet = CharSet.Unicode)]
        private static extern CredUIReturnCodes CredUIParseUserName(string userName, StringBuilder user, int userMaxChars, StringBuilder domain, int domainMaxChars);

        private static CredUIReturnCodes PromptForCredentials(Window parentWindow, ref CREDUI_INFO creditUI, string targetName, int netError, string initialUserNameValue, out string domainName, out string userName, out string password, ref bool saveSettings, CREDUI_FLAGS flags)
        {
            const int MAX_USER_NAME = 100;
            const int MAX_PASSWORD = 100;

            userName = String.Empty;
            domainName = String.Empty;
            password = String.Empty;

            creditUI.cbSize = Marshal.SizeOf(creditUI);
            creditUI.hwndParent = new WindowInteropHelper(parentWindow).Handle;

            StringBuilder user = new StringBuilder(initialUserNameValue, MAX_USER_NAME);
            StringBuilder pwd = new StringBuilder(MAX_PASSWORD);
            CredUIReturnCodes result = CredUIPromptForCredentials(ref creditUI, targetName, IntPtr.Zero, netError, user, MAX_USER_NAME, pwd, MAX_PASSWORD, ref saveSettings, flags);
            if (result == CredUIReturnCodes.NO_ERROR)
            {
                string tempUserName = user.ToString();
                string tempPassword = pwd.ToString();

                StringBuilder userBuilder = new StringBuilder();
                StringBuilder domainBuilder = new StringBuilder();

                CredUIReturnCodes returnCode = CredUIParseUserName(tempUserName, userBuilder, int.MaxValue, domainBuilder, int.MaxValue);
                switch (returnCode)
                {
                    case CredUIReturnCodes.NO_ERROR:
                        userName = userBuilder.ToString();
                        domainName = domainBuilder.ToString();
                        password = tempPassword;
                        return returnCode;

                    case CredUIReturnCodes.ERROR_INVALID_ACCOUNT_NAME:
                        userName = tempUserName;
                        domainName = String.Empty;
                        password = tempPassword;
                        return returnCode;

                    default:
                        return returnCode;
                }
            }

            return result;
        }
        #endregion

        #region LogonUser
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LogonUser(string principal, string authority, string password, LogonTypes logonType, LogonProviders logonProvider, out IntPtr token);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);
        enum LogonProviders : uint
        {
            Default = 0, // default for platform (use this!)
            WinNT35,     // sends smoke signals to authority
            WinNT40,     // uses NTLM
            WinNT50      // negotiates Kerb or NTLM
        }
        enum LogonTypes : uint
        {
            Interactive = 2,
            Network = 3,
            Batch = 4,
            Service = 5,
            Unlock = 7,
            NetworkCleartext = 8,
            NewCredentials = 9
        }

        enum LogonResult
        {
            NO_ERROR = 0,
            ERROR_PASSWORD_MUST_CHANGE = 1907,
            ERROR_LOGON_FAILURE = 1326,
            ERROR_ACCOUNT_RESTRICTION = 1327,
            ERROR_ACCOUNT_DISABLED = 1331,
            ERROR_INVALID_LOGON_HOURS = 1328,
            ERROR_NO_LOGON_SERVERS = 1311,
            ERROR_INVALID_WORKSTATION = 1329,
            ERROR_ACCOUNT_LOCKED_OUT = 1909,      //It gives this error if the account is locked, REGARDLESS OF WHETHER VALID CREDENTIALS WERE PROVIDED!!!
            ERROR_ACCOUNT_EXPIRED = 1793,
            ERROR_PASSWORD_EXPIRED = 1330,
        }

        private static LogonResult CheckUserLogon(string username, string password, string domain_fqdn)
        {
            var logonResult = LogonResult.NO_ERROR;
            IntPtr token = new IntPtr();
            if (LogonUser(username, domain_fqdn, password, LogonTypes.Network, LogonProviders.Default, out token))
            {
                CloseHandle(token);
            }
            else
            {
                logonResult = (LogonResult)Marshal.GetLastWin32Error();
            }
         
            return logonResult;
        }
        #endregion
    }
}
