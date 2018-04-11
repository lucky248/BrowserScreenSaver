using System;
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

        public MainWindow()
        {
            InitializeComponent();

            var rowDefinitions = this.MainGrid.RowDefinitions;
            var topPanelHeight = (int)(Settings.Default.HorizontalSplitter * 100);
            rowDefinitions[0].Height = new GridLength(value: topPanelHeight, type: GridUnitType.Star);
            rowDefinitions[2].Height = new GridLength(value: 100 - topPanelHeight, type: GridUnitType.Star);

            var topColumnsDefinitions = this.TopGrid.ColumnDefinitions;
            var topLeftPanelWidth = (int)(Settings.Default.TopVerticalSplitter * 100);
            topColumnsDefinitions[0].Width = new GridLength(value: topLeftPanelWidth, type: GridUnitType.Star);
            topColumnsDefinitions[2].Width = new GridLength(value: 100 - topLeftPanelWidth, type: GridUnitType.Star);

            var bottomColumnsDefinitions = this.BottomGrid.ColumnDefinitions;
            var bottomLeftPanelWidth = (int)(Settings.Default.BottomVerticalSplitter * 100);
            bottomColumnsDefinitions[0].Width = new GridLength(value: bottomLeftPanelWidth, type: GridUnitType.Star);
            bottomColumnsDefinitions[2].Width = new GridLength(value: 100 - bottomLeftPanelWidth, type: GridUnitType.Star);

            InitializePanel(this.TopLeftBrowser);
            InitializePanel(this.TopRightBrowser);
            InitializePanel(this.BottomLeftBrowser);
            InitializePanel(this.BottomRightBrowser);

            if (Settings.Default.NavigationEnabledByUtc > DateTime.UtcNow)
            {
                var timer = new DispatcherTimer();
                var savedBackground = this.MainGrid.Background;
                timer.Tick += (sender, args) =>
                {
                    var utcNow = DateTime.UtcNow;
                    var maxSeconds = SettingsWindow.EnableNavigationTimeSpan.TotalSeconds;
                    var clippedRemainingSeconds = Math.Min(maxSeconds, (Settings.Default.NavigationEnabledByUtc - utcNow).TotalSeconds);
                    var blendAmmount = clippedRemainingSeconds > 0 ? clippedRemainingSeconds / maxSeconds : 0;
                    this.MainGrid.Background = Colors.LightPink.CreateBlendBrush(Colors.Red, blendAmmount);
                    if (clippedRemainingSeconds <=0 )
                    {
                        this.MainGrid.Background = savedBackground;
                        timer.Stop();
                    }
                };
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Start();
            }
        }

        private void InitializePanel(BrowserPanel panel)
        {
            SetPanelScale(panel);
            panel.RefreshFrequencyMin = (int)Settings.Default[this.GetRefreshFrequencyPropertyName(panel)];

            panel.ScaleChanged += Browser_OnScaleChanged;
            panel.RefreshFrequencyChanged += Browser_OnRefreshFrequencyChanged;
            panel.MaximizationChanged += Browser_OnMaximizationChanged;
            Uri uri;
            if (Uri.TryCreate((string)Settings.Default[this.GetUriPropertyName(panel)], UriKind.Absolute, out uri))
            {
                panel.Navigate(uri);
            }
        }

        private void Browser_OnScaleChanged(object sender, EventArgs eventArgs)
        {
            var panel = (BrowserPanel)sender;
            Settings.Default[this.GetScalePropertyName(panel)] = panel.Scale;
        }

        private void Browser_OnRefreshFrequencyChanged(object sender, EventArgs eventArgs)
        {
            var panel = (BrowserPanel)sender;
            Settings.Default[this.GetRefreshFrequencyPropertyName(panel)] = panel.RefreshFrequencyMin;
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
                SetPanelScale(formerMaximizedPanel);
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
                SetPanelScale(newPanel);
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
            Settings.Default.HorizontalSplitter = topHeight / (topHeight + this.BottomLeftBrowser.ActualHeight);
            Settings.Default.TopVerticalSplitter = topLeftWidth / (topLeftWidth + this.TopRightBrowser.ActualWidth);
            Settings.Default.BottomVerticalSplitter = bottomLeftWidth / (bottomLeftWidth + this.BottomRightBrowser.ActualWidth);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
            if (!this.IsPreviewMode && Settings.Default.OnResumeDisplayLogon)
            {
                Session.List().ForEach(s => s.Disconnect());
            }
        }

        private void SetPanelScale(BrowserPanel panel)
        {
            panel.Scale = (int)Settings.Default[GetScalePropertyName(panel)];
        }

        private string GetUriPropertyName(BrowserPanel panel)
        {
            string propertyName = null;
            DispatchPanel(
                panel: panel,
                topLeft: delegate { propertyName = nameof(Settings.Default.Uri1); },
                topRight: delegate { propertyName = nameof(Settings.Default.Uri2); },
                bottomLeft: delegate { propertyName = nameof(Settings.Default.Uri3); },
                bottomRight: delegate { propertyName = nameof(Settings.Default.Uri4); });

            return propertyName;
        }

        private string GetRefreshFrequencyPropertyName(BrowserPanel panel)
        {
            string propertyName = null;
            DispatchPanel(
                panel: panel,
                topLeft: delegate { propertyName = nameof(Settings.Default.RefreshFreq1); },
                topRight: delegate { propertyName = nameof(Settings.Default.RefreshFreq2); },
                bottomLeft: delegate { propertyName = nameof(Settings.Default.RefreshFreq3); },
                bottomRight: delegate { propertyName = nameof(Settings.Default.RefreshFreq4); });

            return propertyName;
        }

        private string GetScalePropertyName(BrowserPanel panel)
        {
            string propertyName = null;
            if (panel.IsMaximized)
            {
                DispatchPanel(
                    panel: panel,
                    topLeft: delegate { propertyName = nameof(Settings.Default.Scale1Maximized); },
                    topRight: delegate { propertyName = nameof(Settings.Default.Scale2Maximized); },
                    bottomLeft: delegate { propertyName = nameof(Settings.Default.Scale3Maximized); },
                    bottomRight: delegate { propertyName = nameof(Settings.Default.Scale4Maximized); });
            }
            else
            {
                DispatchPanel(
                    panel: panel,
                    topLeft: delegate { propertyName = nameof(Settings.Default.Scale1); },
                    topRight: delegate { propertyName = nameof(Settings.Default.Scale2); },
                    bottomLeft: delegate { propertyName = nameof(Settings.Default.Scale3); },
                    bottomRight: delegate { propertyName = nameof(Settings.Default.Scale4); });
            }

            return propertyName;
        }

        private void DispatchPanel(BrowserPanel panel, Action topLeft, Action topRight, Action bottomLeft, Action bottomRight)
        {
            if (ReferenceEquals(panel, this.TopLeftBrowser))
            {
                topLeft();
            }
            else if (ReferenceEquals(panel, this.TopRightBrowser))
            {
                topRight();
            }
            else if (ReferenceEquals(panel, this.BottomLeftBrowser))
            {
                bottomLeft();
            }
            else if (ReferenceEquals(panel, this.BottomRightBrowser))
            {
                bottomRight();
            }
            else
            {
                throw new InvalidOperationException("Unexpected panel");
            }
        }
    }
}
