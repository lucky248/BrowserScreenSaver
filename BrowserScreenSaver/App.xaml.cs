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
        private HwndSource previewWindowContainer;
        private MainWindow previewWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
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

            if (e.Args[0].ToLower().StartsWith("/p"))
            {
                var previewHandle = Convert.ToInt32(e.Args[1]);
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
                    this.previewWindowContainer = new HwndSource(sourceParams);
                }

                this.previewWindow = new MainWindow { IsPreviewMode = true };
                //var config = MainWindowConfiguration.FromString(Settings.Default.Window1Config);
                //this.previewWindow.InitializeConfig(config.SharedConfig, new[] { config.Panes[0], config.Panes[1], config.Panes[2], config.Panes[3] });
                this.previewWindowContainer.RootVisual = (Visual)previewWindow.Content;
                this.previewWindowContainer.Disposed += WinWpfContentOnDisposed;
            }
            else if (e.Args[0].ToLower().StartsWith("/s"))
            {
                var win = new MainWindow { WindowState = WindowState.Maximized };
                win.ConfigurationChanged += delegate
                {
                    var newConfig = config.ToString();
                    if (!string.Equals(Settings.Default.Config, newConfig, StringComparison.Ordinal))
                    {
                        Settings.Default.Save();
                    }
                };
                win.InitializeConfig(config.SharedConfig, new[] { config.Panes[0], config.Panes[1], config.Panes[2], config.Panes[3] });
                win.Show();
            }
            else if (e.Args[0].ToLower().StartsWith("/c"))
            {
                var win = new SettingsWindow();
                win.InitializeConfig(config.SharedConfig, config.Panes);
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
        }

        private void WinWpfContentOnDisposed(object sender, EventArgs eventArgs)
        {
            this.previewWindow.Close();
        }
    }
}
