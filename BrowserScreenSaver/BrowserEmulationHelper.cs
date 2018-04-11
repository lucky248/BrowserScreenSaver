using System;
using System.IO;
using System.Security;
using Microsoft.Win32;

namespace BrowserScreenSaver
{
    class BrowserEmulationHelper
    {
        enum BrowserEmulationVersion
        {
            Default = 0,
            Version7 = 7000,
            Version8 = 8000,
            Version8Standards = 8888,
            Version9 = 9000,
            Version9Standards = 9999,
            Version10 = 10000,
            Version10Standards = 10001,
            Version11 = 11000,
            Version11Edge = 11001
        }

        private const string BrowserEmulationKey = InternetExplorerRootKey + @"\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
        private const string InternetExplorerRootKey = @"Software\Microsoft\Internet Explorer";

        public static bool IsBrowserEmulationSet()
        {
            return GetBrowserEmulationVersion() != BrowserEmulationVersion.Default;
        }

        public static bool SetBrowserEmulationVersion()
        {
            BrowserEmulationVersion emulationCode;
            var ieVersion = GetInternetExplorerMajorVersion();

            if (ieVersion >= 11)
            {
                emulationCode = BrowserEmulationVersion.Version11;
            }
            else
            {
                switch (ieVersion)
                {
                    case 10:
                        emulationCode = BrowserEmulationVersion.Version10;
                        break;
                    case 9:
                        emulationCode = BrowserEmulationVersion.Version9;
                        break;
                    case 8:
                        emulationCode = BrowserEmulationVersion.Version8;
                        break;
                    default:
                        emulationCode = BrowserEmulationVersion.Version7;
                        break;
                }
            }

            return SetBrowserEmulationVersion(emulationCode);
        }

        private static int GetInternetExplorerMajorVersion()
        {
            var result = 0;

            try
            {
                var key = Registry.LocalMachine.OpenSubKey(InternetExplorerRootKey);

                if (key != null)
                {
                    var value = key.GetValue("svcVersion", null) ?? key.GetValue("Version", null);

                    if (value != null)
                    {
                        var version = value.ToString();
                        var separator = version.IndexOf('.');
                        if (separator != -1)
                        {
                            int.TryParse(version.Substring(0, separator), out result);
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                // The user does not have the permissions required to read from the registry key.
            }
            catch (UnauthorizedAccessException)
            {
                // The user does not have the necessary registry rights.
            }

            return result;
        }

        private static BrowserEmulationVersion GetBrowserEmulationVersion()
        {
            var result = BrowserEmulationVersion.Default;

            try
            {
                var key = Registry.CurrentUser.OpenSubKey(BrowserEmulationKey, true);
                if (key != null)
                {
                    var programName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
                    var value = key.GetValue(programName, null);

                    if (value != null)
                    {
                        result = (BrowserEmulationVersion)Convert.ToInt32(value);
                    }
                }
            }
            catch (SecurityException)
            {
                // The user does not have the permissions required to read from the registry key.
            }
            catch (UnauthorizedAccessException)
            {
                // The user does not have the necessary registry rights.
            }

            return result;
        }

        private static bool SetBrowserEmulationVersion(BrowserEmulationVersion browserEmulationVersion)
        {
            var result = false;

            try
            {
                var key = Registry.CurrentUser.OpenSubKey(BrowserEmulationKey, true);

                if (key != null)
                {
                    var programName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

                    if (browserEmulationVersion != BrowserEmulationVersion.Default)
                    {
                        // if it's a valid value, update or create the value
                        key.SetValue(programName, (int)browserEmulationVersion, RegistryValueKind.DWord);
                    }
                    else
                    {
                        // otherwise, remove the existing value
                        key.DeleteValue(programName, false);
                    }

                    result = true;
                }
            }
            catch (SecurityException)
            {
                // The user does not have the permissions required to read from the registry key.
            }
            catch (UnauthorizedAccessException)
            {
                // The user does not have the necessary registry rights.
            }

            return result;
        }
    }
}
