using BrowserScreenSaver.Properties;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace BrowserScreenSaver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs startupEventArgs)
        {
            AppConfiguration config;
            try
            {
                config = AppConfiguration.FromString(Settings.Default.Config);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read config: {ex.ToString()}");
                config = new AppConfiguration();
            }

            if (!BrowserEmulationHelper.IsBrowserEmulationSet())
            {
                BrowserEmulationHelper.SetBrowserEmulationVersion();
            }

            if (startupEventArgs.Args[0].ToLower().StartsWith("/p"))
            {
                var previewHandle = Convert.ToInt32(startupEventArgs.Args[1]);
                LaunchPreviewWinodw(previewHandle: previewHandle, config: config);
            }
            else if (startupEventArgs.Args[0].ToLower().StartsWith("/s"))
            {
                LaunchScreensaverWindows(config);
            }
            else if (startupEventArgs.Args[0].ToLower().StartsWith("/c"))
            {
                var win = new AppConfigurationWindow();
                win.InitializeConfig(config);
                win.Closed += delegate
                {
                    Settings.Default.Config = config.ToString();
                    Settings.Default.Save();
                };
                win.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }

            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private static void LaunchPreviewWinodw(int previewHandle, AppConfiguration config)
        {
            var pPreviewHnd = new IntPtr(previewHandle);
            var lpRect = new NativeMethods.Rect();
            if (NativeMethods.GetClientRect(pPreviewHnd, ref lpRect))
            {
                var sourceParams = new HwndSourceParameters(name: "sourceParams")
                {
                    PositionX = 0,
                    PositionY = 0,
                    Height = lpRect.Bottom - lpRect.Top,
                    Width = lpRect.Right - lpRect.Left,
                    ParentWindow = pPreviewHnd,
                    WindowStyle = (int)(
                        NativeMethods.WindowStyles.WS_VISIBLE
                        | NativeMethods.WindowStyles.WS_CHILD
                        | NativeMethods.WindowStyles.WS_CLIPCHILDREN)
                };
                var previewWindowContainer = new HwndSource(sourceParams);

                var previewWindow = new MainWindow { IsPreviewMode = true };
                previewWindow.InitializeConfig(config.Windows[0], config.SharedWindowConfig, config.SharedPanelConfig);
                previewWindowContainer.RootVisual = (Visual)previewWindow.Content;
                previewWindowContainer.Disposed += delegate
                {
                    previewWindow.Close();
                };
            }
        }

        private static void LaunchScreensaverWindows(AppConfiguration config)
        {
            config.SharedPanelConfig.Changed += delegate 
            { 
                SaveConfiguration(config); 
            };

            var ratio = Math.Max(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.PrimaryScreenWidth,
                            System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / SystemParameters.PrimaryScreenHeight);
            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length && i < config.Windows.Count; i++)
            {
                var screen = System.Windows.Forms.Screen.AllScreens[i];
                var window = new MainWindow();
                window.Left = screen.WorkingArea.Left / ratio;
                window.Top = screen.WorkingArea.Top / ratio;
                window.Width = screen.WorkingArea.Width / ratio;
                window.Height = screen.WorkingArea.Height / ratio;
                window.ConfigurationChanged += delegate { SaveConfiguration(config); };
                window.InitializeConfig(config.Windows[i], config.SharedWindowConfig, config.SharedPanelConfig);
                window.Show();
                window.WindowState = WindowState.Maximized; // Change WindowState after calling Show
                if (screen.Primary)
                {
                    Current.MainWindow = window;
                }
            }
        }

        private static void SaveConfiguration(AppConfiguration config)
        {
            // Lots of false-positives on resize events. Go easy on Settings.Default.Save()
            var newConfig = config.ToString();
            if (!string.Equals(Settings.Default.Config, newConfig, StringComparison.Ordinal))
            {
                Settings.Default.Save();
            }
        }
    }
}
