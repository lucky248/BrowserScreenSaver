using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using BrowserScreenSaver.Extensions;
using BrowserScreenSaver.Properties;

namespace BrowserScreenSaver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isPreviewMode;
        private AppConfiguration.SharedConfiguration sharedConfig;
        private AppConfiguration.WindowConfiguration windowConfig;

        public bool IsPreviewMode
        {
            get { return this.isPreviewMode; }
            set
            {
                this.isPreviewMode = value;

                // Moving GridSplitter kills the preview mode for some reason. Disabling since it is unnecessary in the preview mode.
                this.TopGridSplitter.IsEnabled = !value;
                this.MiddleGridSplitter.IsEnabled = !value;
                this.BottomGridSplitter.IsEnabled = !value;
                this.TopLeftBrowser.IsPreviewMode = value;
                this.TopRightBrowser.IsPreviewMode = value;
                this.BottomLeftBrowser.IsPreviewMode = value;
                this.BottomRightBrowser.IsPreviewMode = value;
            }
        }

        public event EventHandler ConfigurationChanged;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void InitializeConfig(AppConfiguration.SharedConfiguration sharedConfig, AppConfiguration.WindowConfiguration windowConfiguration)
        {
            if(this.sharedConfig != null)
            {
                throw new InvalidOperationException("Window is already initialized");
            }

            this.sharedConfig = sharedConfig;
            this.windowConfig = windowConfiguration;
            var rowDefinitions = this.MainGrid.RowDefinitions;
            var topPanelHeight = (int)(windowConfiguration.HorizontalSplitter * 100);
            rowDefinitions[0].Height = new GridLength(value: topPanelHeight, type: GridUnitType.Star);
            rowDefinitions[2].Height = new GridLength(value: 100 - topPanelHeight, type: GridUnitType.Star);

            var topColumnsDefinitions = this.TopGrid.ColumnDefinitions;
            var topLeftPanelWidth = (int)(windowConfiguration.TopVerticalSplitter * 100);
            topColumnsDefinitions[0].Width = new GridLength(value: topLeftPanelWidth, type: GridUnitType.Star);
            topColumnsDefinitions[2].Width = new GridLength(value: 100 - topLeftPanelWidth, type: GridUnitType.Star);

            var bottomColumnsDefinitions = this.BottomGrid.ColumnDefinitions;
            var bottomLeftPanelWidth = (int)(windowConfiguration.BottomVerticalSplitter * 100);
            bottomColumnsDefinitions[0].Width = new GridLength(value: bottomLeftPanelWidth, type: GridUnitType.Star);
            bottomColumnsDefinitions[2].Width = new GridLength(value: 100 - bottomLeftPanelWidth, type: GridUnitType.Star);

            InitializePanel(this.TopLeftBrowser, windowConfiguration.Panes[0]);
            InitializePanel(this.TopRightBrowser, windowConfiguration.Panes[1]);
            InitializePanel(this.BottomLeftBrowser, windowConfiguration.Panes[2]);
            InitializePanel(this.BottomRightBrowser, windowConfiguration.Panes[3]);

            if (sharedConfig.NavigationEnabledByUtc > DateTime.UtcNow)
            {
                var timer = new DispatcherTimer();
                var savedBackground = this.MainGrid.Background;
                timer.Tick += (sender, args) =>
                {
                    var utcNow = DateTime.UtcNow;
                    var maxSeconds = SettingsWindow.EnableNavigationTimeSpan.TotalSeconds;
                    var clippedRemainingSeconds = Math.Min(maxSeconds, (this.sharedConfig.NavigationEnabledByUtc - utcNow).TotalSeconds);
                    var blendAmmount = clippedRemainingSeconds > 0 ? clippedRemainingSeconds / maxSeconds : 0;
                    this.MainGrid.Background = Colors.LightPink.CreateBlendBrush(Colors.Red, blendAmmount);
                    if (clippedRemainingSeconds <= 0)
                    {
                        this.MainGrid.Background = savedBackground;
                        timer.Stop();
                    }
                };
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Start();
            }
        }

        private void InitializePanel(BrowserPanel panel, AppConfiguration.PaneConfiguration paneConfiguration)
        {
            panel.Tag = paneConfiguration;
            panel.Scale = paneConfiguration.Scale;
            panel.RefreshFrequencyMin = paneConfiguration.RefreshFreq;
            panel.NavigationEnabledByUtc = sharedConfig.NavigationEnabledByUtc;
            panel.SafeUris = sharedConfig.SafeUris;

            panel.ScaleChanged += delegate
            {
                paneConfiguration.Scale = panel.Scale;
                this.ConfigurationChanged.Invoke(this, EventArgs.Empty);
            };
            panel.RefreshFrequencyChanged += delegate
            {
                paneConfiguration.RefreshFreq = panel.RefreshFrequencyMin;
                this.ConfigurationChanged.Invoke(this, EventArgs.Empty);
            };
            panel.MaximizationChanged += Browser_OnMaximizationChanged;
            panel.SafeUriAdded += Browser_SafeUriAdded;
            Uri uri;
            if (Uri.TryCreate(paneConfiguration.Uri, UriKind.Absolute, out uri))
            {
                panel.Navigate(uri);
            }
        }

        private void Browser_SafeUriAdded(object sender, List<Uri> safeUris)
        {
            foreach(var uri in safeUris)
            {
                if(!this.sharedConfig.SafeUris.Contains(uri))
                {
                    this.sharedConfig.SafeUris.Add(uri);
                }
            }

            this.ConfigurationChanged.Invoke(this, EventArgs.Empty);
        }

        private void Browser_OnMaximizationChanged(object sender, EventArgs eventArgs)
        {
            // Restore currently maximized window (if any) first
            if (this.PopupMainGrid.Children.Count > 0)
            {
                var formerMaximizedPanel = (BrowserPanel)this.PopupMainGrid.Children[0];
                var formerParentGrid = (Grid)formerMaximizedPanel.Tag;
                this.PopupMainGrid.Children.Clear();
                formerParentGrid.Children.Add(formerMaximizedPanel);

                // Re-enter this same method with a former panel (should be a no-op)
                formerMaximizedPanel.IsMaximized = false;
                var formerPaneConfiguration = (AppConfiguration.PaneConfiguration)formerMaximizedPanel.Tag;
                formerMaximizedPanel.Scale = formerPaneConfiguration.Scale;
            }

            // Maximize if needed
            var newPanel = (BrowserPanel)sender;
            if (newPanel.IsMaximized)
            {
                var currentParentGrid = (Grid)newPanel.Parent;
                currentParentGrid.Children.Remove(newPanel);
                newPanel.Tag = currentParentGrid;
                this.PopupMainGrid.Children.Add(newPanel);
                this.Popup.Height = this.ActualHeight * 0.75 - this.PopupMainGrid.Margin.Top - this.PopupMainGrid.Margin.Bottom;
                this.Popup.Width = this.ActualWidth - this.PopupMainGrid.Margin.Left - this.PopupMainGrid.Margin.Right;
                this.Popup.IsOpen = true;
                var newPaneConfiguration = (AppConfiguration.PaneConfiguration)newPanel.Tag;
                newPanel.Scale = newPaneConfiguration.Scale;
            }
            else
            {
                this.Popup.IsOpen = false;
            }
        }

        private void TopBrowser_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var topHeight = this.TopLeftBrowser.ActualHeight;
            var topLeftWidth = this.TopLeftBrowser.ActualWidth;
            var bottomLeftWidth = this.BottomLeftBrowser.ActualWidth;
            this.windowConfig.HorizontalSplitter = topHeight / (topHeight + this.BottomLeftBrowser.ActualHeight);
            this.windowConfig.TopVerticalSplitter = topLeftWidth / (topLeftWidth + this.TopRightBrowser.ActualWidth);
            this.windowConfig.BottomVerticalSplitter = bottomLeftWidth / (bottomLeftWidth + this.BottomRightBrowser.ActualWidth);
            this.ConfigurationChanged.Invoke(this, EventArgs.Empty);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ConfigurationChanged.Invoke(this, EventArgs.Empty);
            if (!this.IsPreviewMode && this.sharedConfig.OnResumeDisplayLogon)
            {
                Session.List().ForEach(s => s.Disconnect());
            }
        }
    }
}
