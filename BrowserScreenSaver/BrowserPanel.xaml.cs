using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool browserConfigured;
        private bool loadCompleted;
        private bool isMaximized;
        private int refreshFrequencyMins;
        private readonly DispatcherTimer timer;
        private Uri baselineUri;


        public event EventHandler ScaleChanged;
        public event EventHandler MaximizationChanged;
        public event EventHandler RefreshFrequencyChanged;
        public event EventHandler<List<Uri>> SafeUriAdded;

        private bool IsNavigationEnabled => Properties.Settings.Default.NavigationEnabledByUtc > DateTime.UtcNow;

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

                if (changed)
                {
                    if (this.loadCompleted)
                    {
                        this.WebBrowser.SetScale(scaleLevel: this.EffectiveScale);
                    }

                    this.Slider.Value = value;
                    this.ScaleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int EffectiveScale => this.IsPreviewMode ? Math.Max(10, this.Scale / 4) : this.Scale; // Between 10 and 1000

        public int RefreshFrequencyMin
        {
            get { return this.refreshFrequencyMins; }
            set
            {
                if (value <= 0)
                {
                    timer.IsEnabled = false;
                    this.refreshFrequencyMins = 0;
                    return;
                }

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
                    this.ErrorMessage.Tag = null;
                    //this.WebBrowser.Navigate(this.WebBrowser.Source);
                    //this.WebBrowser.Refresh();
                    this.WebBrowser.Navigate(this.baselineUri);
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
            if (!this.loadCompleted || this.browserConfigured)
            {
                return;
            }

            this.browserConfigured = true;
            this.WebBrowser.SetScale(scaleLevel: this.EffectiveScale);
            this.WebBrowser.SetScrollBarVisibility(isVisible: this.IsMaximized);
            this.WebBrowser.SetSilent(silent: !Properties.Settings.Default.AllowPopups);

            // Disable context menu
            NativeMethods.ICustomDoc doc = this.WebBrowser.Document as NativeMethods.ICustomDoc;
            doc.SetUIHandler(new DocHostUIHandler(this.WebBrowser, isBrowserContextMenuEnabled: false));

            // Block opening new windows (test -> navigate to msn.com and click on OUTLOOK button)
            var eventSink = new WebBrowserEventSink(isNewWindowEnabled: false);
            eventSink.Connect(this.WebBrowser);

            this.WebBrowser.MouseDown += delegate (object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                e.Handled = !this.IsNavigationEnabled;
            };

            this.WebBrowser.KeyDown += delegate (object sender, System.Windows.Input.KeyEventArgs e)
            {
                e.Handled = !IsNavigationEnabled;
            };

            // Block any further navigation
            this.WebBrowser.Navigating += delegate(object sender, NavigatingCancelEventArgs args)
            {
                var isSafelySimilarUri = string.Equals(args.Uri.Host, baselineUri.Host, StringComparison.OrdinalIgnoreCase)
                                    && (string.Equals(args.Uri.AbsolutePath, baselineUri.AbsolutePath, StringComparison.OrdinalIgnoreCase));

                var safeUriPrefixes = Properties.Settings.Default.SafeUris.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var isSafeUri = false;
                foreach (var safeUriPrefixText in safeUriPrefixes)
                {
                    var safePrefixUri = new Uri(safeUriPrefixText);
                    if (args.Uri.ToString().StartsWith(safePrefixUri.ToString()))
                    {
                        isSafeUri = true;
                    }
                }

                // Append safe URIs if navigation is enabled
                if (this.IsNavigationEnabled || isSafelySimilarUri || isSafeUri)
                {
                    if (this.IsNavigationEnabled && !isSafelySimilarUri && !isSafeUri)
                    {
                        this.AppendTemporaryAllowedUri(args.Uri);
                    }

                    args.Cancel = false;
                    this.Address.Text = args.Uri.ToString();
                }
                else
                {
                    this.ErrorMessage.Text = $"Blocked navigation to URI: {args.Uri}";
                    this.ErrorMessage.Tag = args.Uri;
                    args.Cancel = true;
                }

                // Enable Add button if navigation is enabled
                if (this.ErrorMessage.Tag == null)
                {
                    this.ErrorPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.AddSafeUri.Visibility = this.IsNavigationEnabled ? Visibility.Visible : Visibility.Collapsed;
                    this.ErrorPanel.Visibility = Visibility.Visible;
                }

                SetTimerOnNavigation(args.Uri);
            };

            this.WebBrowser.Navigated += delegate(object sender, NavigationEventArgs args)
            {
                try
                {
                    var uri = ((dynamic)this.WebBrowser.Document).url;
                    SetTimerOnNavigation(new Uri(uri));
                }
                catch (Exception)
                {
                }
            };
        }

        void ClearSkipUri()
        {
            this.ErrorPanel.Visibility = Visibility.Collapsed;
            this.ErrorMessage.Text = string.Empty;
            this.ErrorMessage.Tag = null;
        }

        void AppendTemporaryAllowedUri(Uri uri)
        {
            var uriList = this.ErrorMessage.Tag as List<Uri>;
            if(uriList == null)
            {
                uriList = new List<Uri>();
                this.ErrorMessage.Tag = uriList;
            }

            // Don't add duplicates
            if(!uriList.Contains(uri))
            {
                uriList.Add(uri);
            }

            if (uriList.Count > 0)
            {
                this.ErrorMessage.Text = $"Temporarily allowed navigation to URI: {uriList[0]}";
                for (int i = 1; i < uriList.Count; i++)
                {
                    this.ErrorMessage.Text += Environment.NewLine + uriList[i];
                }
            }
        }

        void SetTimerOnNavigation(Uri uri)
        {
            timer.IsEnabled = false;
            if (string.Equals(uri.Scheme, "res", StringComparison.OrdinalIgnoreCase))
            {
                timer.Interval = TimeSpan.FromSeconds(30);
                timer.IsEnabled = true;
            }
            else if (this.RefreshFrequencyMin > 0)
            {
                timer.Interval = TimeSpan.FromMinutes(this.refreshFrequencyMins);
                timer.IsEnabled = true;
            }
        }

        private void AddToSafeUriButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = this.ErrorMessage.Tag as List<Uri>;
            this.ErrorMessage.Tag = null;
            this.ErrorMessage.Text = string.Empty;
            this.ErrorPanel.Visibility = Visibility.Hidden;
            if (tag != null)
            {
                this.SafeUriAdded?.Invoke(this, tag);
                this.RefreshBrowser();
            }
        }
    }
}
