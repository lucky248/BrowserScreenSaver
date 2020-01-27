using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using BrowserScreenSaver.Extensions;

namespace BrowserScreenSaver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isPreviewMode;
        private AppConfiguration.SharedPanelConfiguration sharedPanelConfig;
        private AppConfiguration.SharedWindowConfiguration sharedWindowConfig;
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

        public void InitializeConfig(AppConfiguration.WindowConfiguration windowConfiguration, AppConfiguration.SharedWindowConfiguration sharedWindowConfig, AppConfiguration.SharedPanelConfiguration sharedPanelConfig)
        {
            if (this.sharedPanelConfig != null)
            {
                throw new InvalidOperationException("Window is already initialized");
            }

            this.sharedPanelConfig = sharedPanelConfig;
            this.sharedWindowConfig = sharedWindowConfig;
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

            CheckShowStartupDelayScreen(
                completionAction: delegate
                    {
                        this.Dispatcher.DelayInvoke(TimeSpan.FromSeconds(1), () => InitializePanel(this.TopLeftBrowser, windowConfiguration.Panes[0]));
                        this.Dispatcher.DelayInvoke(TimeSpan.FromSeconds(5), () => InitializePanel(this.TopRightBrowser, windowConfiguration.Panes[1]));
                        this.Dispatcher.DelayInvoke(TimeSpan.FromSeconds(10), () => InitializePanel(this.BottomLeftBrowser, windowConfiguration.Panes[2]));
                        this.Dispatcher.DelayInvoke(TimeSpan.FromSeconds(15), () => InitializePanel(this.BottomRightBrowser, windowConfiguration.Panes[3]));
                    });

            CheckStartBackgroundAnimation();
            this.sharedPanelConfig.Changed += delegate
            {
                CheckStartBackgroundAnimation();
            };
        }

        void CheckShowStartupDelayScreen(Action completionAction)
        {
            uint startupDelaySec = this.sharedWindowConfig.StartupDelaySec;
            if(startupDelaySec == 0)
            {
                completionAction();
                return;
            }
            
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            var finishTime = DateTime.UtcNow.AddSeconds(startupDelaySec);
            timer.Tick += delegate
            {
                if (this.PopupMainGrid.Children.Count == 0)
                {
                    var grid = new Grid()
                    {
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Height = this.ActualHeight * 0.5,
                        Width = this.ActualWidth * 0.5,
                    };
                    grid.Children.Add(
                        new TextBlock()
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        });
                    this.PopupMainGrid.Children.Add(grid);

                    this.Popup.Height = this.ActualHeight;
                    this.Popup.Width = this.ActualWidth;
                }

                var secondsLeft = (finishTime - DateTime.UtcNow).TotalSeconds;
                var gridContainer = (Grid)this.PopupMainGrid.Children[0];
                var textBlock = (TextBlock)gridContainer.Children[0];
                textBlock.Text = $"Holding on initailization for {(int)secondsLeft} more seconds.";
                if (secondsLeft < 0.2)
                {
                    timer.Stop();
                    this.PopupMainGrid.Children.Clear();
                    this.Popup.IsOpen = false;
                    completionAction();
                }
            };
            this.Popup.IsOpen = true;
            timer.Start();
        }

        void CheckStartBackgroundAnimation()
        {
            var utcNow = DateTime.UtcNow;
            var maxSeconds = AppConfigurationWindow.EnableNavigationTimeSpan.TotalSeconds;
            var clippedRemainingSeconds = Math.Min(maxSeconds, (this.sharedPanelConfig.NavigationEnabledByUtc - utcNow).TotalSeconds);
            if (clippedRemainingSeconds > 0)
            {
                var storyboard = (Storyboard)this.Resources["BackgroundStoryboard"];
                var animation = (ColorAnimation)storyboard.Children[0];
                animation.Duration = TimeSpan.FromSeconds(clippedRemainingSeconds);
                storyboard.Begin();
            }
        }

        private void InitializePanel(BrowserPanel panel, AppConfiguration.PanelConfiguration paneConfiguration)
        {
            panel.Scale = paneConfiguration.Scale;
            panel.RefreshFrequencyMin = paneConfiguration.RefreshFreq;
            panel.SharedPanelConfiguration = sharedPanelConfig;

            panel.ScaleChanged += delegate
            {
                if (panel.IsMaximized)
                {
                    paneConfiguration.ScaleMaximized = panel.Scale;
                }
                else
                {
                    paneConfiguration.Scale = panel.Scale;
                }
                this.ConfigurationChanged?.Invoke(this, EventArgs.Empty);
            };
            panel.RefreshFrequencyChanged += delegate
            {
                paneConfiguration.RefreshFreq = panel.RefreshFrequencyMin;
                this.ConfigurationChanged?.Invoke(this, EventArgs.Empty);
            };
            panel.MaximizationChanged += delegate { OnPanelMaximizationChanged(paneConfiguration, panel); };
            Uri uri;
            if (Uri.TryCreate(paneConfiguration.Uri, UriKind.Absolute, out uri))
            {
                panel.Navigate(uri);
            }
        }

        private void OnPanelMaximizationChanged(AppConfiguration.PanelConfiguration paneConfiguration, BrowserPanel panel)
        {
            // Restore currently maximized window (if any) first
            if (this.PopupMainGrid.Children.Count > 0)
            {
                var formerParentGrid = (Grid)panel.Tag;
                this.PopupMainGrid.Children.Clear();
                formerParentGrid.Children.Add(panel);

                // Re-enter this same method with a former panel (should be a no-op)
                panel.IsMaximized = false;
                panel.Scale = paneConfiguration.Scale;
            }

            // Maximize if needed
            if (panel.IsMaximized)
            {
                var currentParentGrid = (Grid)panel.Parent;
                currentParentGrid.Children.Remove(panel);
                panel.Tag = currentParentGrid;
                this.PopupMainGrid.Children.Add(panel);
                this.Popup.Height = this.ActualHeight * 0.75 - this.PopupMainGrid.Margin.Top - this.PopupMainGrid.Margin.Bottom;
                this.Popup.Width = this.ActualWidth - this.PopupMainGrid.Margin.Left - this.PopupMainGrid.Margin.Right;
                this.Popup.IsOpen = true;
                panel.Scale = paneConfiguration.ScaleMaximized;
            }
            else
            {
                this.Popup.IsOpen = false;
                panel.Scale = paneConfiguration.Scale;
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
            this.ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ConfigurationChanged?.Invoke(this, EventArgs.Empty);
            if (!this.IsPreviewMode && this.sharedWindowConfig.OnResumeDisplayLogon)
            {
                Session.List().Where(s => s.State == NativeMethods.WTS_CONNECTSTATE_CLASS.WTSActive).ToList().ForEach(s => s.Disconnect());
            }
        }
    }
}
