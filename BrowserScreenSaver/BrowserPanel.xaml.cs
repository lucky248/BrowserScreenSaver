using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using BrowserScreenSaver.Extensions;

namespace BrowserScreenSaver
{
    /// <summary>
    /// Interaction logic for BrowserPanel.xaml
    /// </summary>
    public partial class BrowserPanel : UserControl
    {
        private bool isPreviewMode;
        private int? scale;
        private bool loadCompleted;
        private bool isMaximized;
        private int refreshFrequencyMins;
        private readonly DispatcherTimer timer;
        private Uri baselineUri;


        public event EventHandler ScaleChanged;
        public event EventHandler MaximizationChanged;
        public event EventHandler RefreshFrequencyChanged;

        public bool IsMaximized
        {
            get { return this.isMaximized; }
            set
            {
                this.isMaximized = value;
                this.MaximizeToggle.Content = this.isMaximized ? "Restore" : "Full screen";
                this.MaximizationChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsPreviewMode
        {
            get { return this.isPreviewMode; }
            set
            {
                this.isPreviewMode = value;
                this.TitleBarPanel.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public int Scale { get { return this.scale ?? 100; }
            set
            {
                bool changed = this.scale.HasValue && this.scale.Value != value;
                this.scale = value; 
                ConfigureBrowser();
                if (changed)
                {
                    this.Slider.Value = value;
                    this.ScaleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int RefreshFrequencyMin
        {
            get { return this.refreshFrequencyMins; }
            set
            {
                if (this.refreshFrequencyMins != value)
                {
                    this.refreshFrequencyMins = value;
                    this.RefreshFrequencyChanged?.Invoke(this, EventArgs.Empty);
                    var item = this.RefreshFrequency.Items
                        .Cast<KeyValuePair<int, string>>()
                        .FirstOrDefault(i => i.Key == value);
                    this.RefreshFrequency.SelectedItem = item;

                    timer.IsEnabled = false;
                    if (value > 0)
                    {
                        timer.Interval = TimeSpan.FromMinutes(value);
                        timer.IsEnabled = true;
                    }
                }
            }
        }

        public BrowserPanel()
        {
            InitializeComponent();

            this.IsMaximized = false;
            this.RefreshFrequency.Items.Add(new KeyValuePair<int, string>(-1, "never"));
            this.RefreshFrequency.Items.Add(new KeyValuePair<int, string>(1, "1 min"));
            this.RefreshFrequency.Items.Add(new KeyValuePair<int, string>(15, "15 mins"));
            this.RefreshFrequency.Items.Add(new KeyValuePair<int, string>(60, "1 hour"));
            this.RefreshFrequency.Items.Add(new KeyValuePair<int, string>(60 * 4, "4 hours"));
            this.RefreshFrequency.Items.Add(new KeyValuePair<int, string>(60 * 8, "8 hours"));

            this.timer = new DispatcherTimer();
            this.timer.Tick += (sender, args) =>
            {
                RefreshBrowser();
            };
        }

        public void Navigate(Uri source)
        {
            this.baselineUri = source;
            this.Address.Text = source.ToString();
            this.WebBrowser.Navigate(source);
        }

        private void WebBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            this.loadCompleted = true;
            ConfigureBrowser();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Scale = (int)e.NewValue;
        }

        private void MaximizeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsMaximized = !this.IsMaximized;
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshBrowser();
        }

        private void RefreshFrequency_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RefreshFrequencyMin = ((KeyValuePair<int, string>)e.AddedItems[0]).Key;
        }

        private void RefreshBrowser()
        {
            if (this.WebBrowser.Document != null)
            {
                try
                {
                    this.WebBrowser.Refresh();
                    this.RefreshBorder.BorderBrush = Brushes.Transparent;
                }
                catch (COMException)
                {
                    this.RefreshBorder.BorderBrush = Brushes.Red;
                }
            }
        }

        private void ConfigureBrowser()
        {
            if (!this.loadCompleted)
            {
                return;
            }

            int scaleLevel = this.IsPreviewMode ? Math.Max(10, this.Scale / 4) : this.Scale; // Between 10 and 1000
            this.WebBrowser.SetScale(scaleLevel: scaleLevel);
            this.WebBrowser.SetScrollBarVisibility(isVisible: this.IsMaximized);
            this.WebBrowser.SetSilent(silent: true);

            // Block any further navigation
            this.WebBrowser.Navigating += delegate(object sender, NavigatingCancelEventArgs args)
            {
                var navigationEnabled = Properties.Settings.Default.NavigationEnabledByUtc > DateTime.UtcNow;

                //MessageBox.Show($"{args.Uri.Host}, {this.baselineUri.Host}, {args.Uri.AbsolutePath}, {this.baselineUri.AbsolutePath}");
                var isSafelySimilarUri = string.Equals(args.Uri.Host, baselineUri.Host, StringComparison.OrdinalIgnoreCase)
                                    && (string.Equals(args.Uri.AbsolutePath, baselineUri.AbsolutePath, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(args.Uri.AbsolutePath, "/", StringComparison.OrdinalIgnoreCase));

                if (navigationEnabled || isSafelySimilarUri)
                {
                    args.Cancel = false;
                    this.Address.Text = args.Uri.ToString();
                }
                else
                {
                    args.Cancel = true;
                }
            };
        }
    }
}
