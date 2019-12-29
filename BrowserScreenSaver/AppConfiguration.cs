using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BrowserScreenSaver
{
    public class AppConfiguration
    {
        public const int SupportedWindowCount = 3; // Config UX is hardcoded to this number of windows

        public class SharedWindowConfiguration
        {
            public bool OnResumeDisplayLogon { get; set; } = true;
        }

        public class SharedPanelConfiguration
        {
            private DateTime navigationEnabledByUtc = new DateTime(year: 2000, month: 1, day: 1);

            public event EventHandler Changed;

            public DateTime NavigationEnabledByUtc
            {
                get
                { 
                    return this.navigationEnabledByUtc; 
                }
                set
                {
                    this.navigationEnabledByUtc = value;
                    this.Changed?.Invoke(this, EventArgs.Empty);
                }
            }

            public IList<Uri> SafeUris { get; }

            public SharedPanelConfiguration()
            {
                var observableSafeUris = new ObservableCollection<Uri>();
                observableSafeUris.CollectionChanged += delegate
                  {
                      this.Changed?.Invoke(this, EventArgs.Empty);
                  };
                this.SafeUris = observableSafeUris;
            }

            public void AddSafeUris(IEnumerable<Uri> uris)
            {
                foreach (var uri in uris)
                {
                    if (!this.SafeUris.Contains(uri))
                    {
                        this.SafeUris.Add(uri);
                    }
                }
            }
        }

        public class PanelConfiguration
        {
            public string Uri { get; set; }
            public int RefreshFreq { get; set; }
            public int Scale { get; set; }
            public int ScaleMaximized { get; set; }

            public PanelConfiguration(string uri=null, int refreshFreq=-1, int scale=100, int scaleMaximized=100)
            {
                this.Uri = uri;
                this.RefreshFreq = refreshFreq;
                this.Scale = scale;
                this.ScaleMaximized = scaleMaximized;
            }
        }

        public class WindowConfiguration
        {
            public double TopVerticalSplitter { get; set; }
            public double BottomVerticalSplitter { get; set; }
            public double HorizontalSplitter { get; set; }
            public IReadOnlyList<PanelConfiguration> Panes{ get; }

            public WindowConfiguration(
                double topVerticalSplitter = 0.5, 
                double bottomVerticalSplitter = 0.5, 
                double horizontalSplitter = 0.5,
                PanelConfiguration[] panes = null)
            {
                this.TopVerticalSplitter = topVerticalSplitter;
                this.BottomVerticalSplitter = bottomVerticalSplitter;
                this.HorizontalSplitter = horizontalSplitter;

                panes = panes ?? new PanelConfiguration[4];
                if(panes.Length != 4)
                {
                    throw new InvalidOperationException($"Expected 4 panes, but constructed with {panes.Length}.");
                }

                for (int i = 0; i < panes.Length; i++)
                {
                    panes[i] = panes[i] ?? new PanelConfiguration();
                }

                this.Panes = panes;
            }
        }

        public SharedPanelConfiguration SharedPanelConfig { get; } = new SharedPanelConfiguration();
        public SharedWindowConfiguration SharedWindowConfig { get; } = new SharedWindowConfiguration();

        public List<WindowConfiguration> Windows { get; }

        public AppConfiguration()
        {
            var windows = new List<WindowConfiguration>();
            for (int i = 0; i < AppConfiguration.SupportedWindowCount; i++)
            {
                windows.Add(new WindowConfiguration());
            }
            this.Windows = windows;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("v1");
            sb.AppendLine(this.SharedWindowConfig.OnResumeDisplayLogon.ToString());
            sb.AppendLine(this.SharedPanelConfig.NavigationEnabledByUtc.ToString());
            sb.AppendLine(this.SharedPanelConfig.SafeUris.Count.ToString());
            foreach(var uri in this.SharedPanelConfig.SafeUris)
            {
                sb.AppendLine(Convert.ToBase64String(Encoding.Unicode.GetBytes(uri.ToString())));
            }

            sb.AppendLine(this.Windows.Count.ToString());
            foreach (var window in this.Windows)
            {
                sb.AppendLine(window.TopVerticalSplitter.ToString());
                sb.AppendLine(window.BottomVerticalSplitter.ToString());
                sb.AppendLine(window.HorizontalSplitter.ToString());
                sb.AppendLine(window.Panes.Count.ToString());
                foreach (var pane in window.Panes)
                {
                    sb.AppendLine(Convert.ToBase64String(Encoding.Unicode.GetBytes(pane.Uri ?? string.Empty)));
                    sb.AppendLine(pane.RefreshFreq.ToString());
                    sb.AppendLine(pane.Scale.ToString());
                    sb.AppendLine(pane.ScaleMaximized.ToString());
                }
            }
            return sb.ToString();
        }

        public static AppConfiguration FromString(string value)
        {
            var config = new AppConfiguration();
            if (string.IsNullOrEmpty(value))
            {
                return config; // Return defaults
            }

            var values = value.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var valueIndex = 1;
            config.SharedWindowConfig.OnResumeDisplayLogon = bool.Parse(values[valueIndex++]);
            config.SharedPanelConfig.NavigationEnabledByUtc = DateTime.Parse(values[valueIndex++]);
            var uriCount = int.Parse(values[valueIndex++]);
            for (int i = 0; i < uriCount; i++)
            {
                var safeUri = new Uri(Encoding.Unicode.GetString(Convert.FromBase64String(values[valueIndex++])), UriKind.Absolute);
                config.SharedPanelConfig.AddSafeUris(new[] { safeUri });
            }

            int windowCount = int.Parse(values[valueIndex++]);
            if (windowCount != AppConfiguration.SupportedWindowCount)
            {
                throw new InvalidOperationException($"Expected {AppConfiguration.SupportedWindowCount} windows, but found {windowCount}");
            }

            var windows = new WindowConfiguration[windowCount];
            for (int i= 0; i < windowCount; i++)
            {
                var topVerticalSplitter = double.Parse(values[valueIndex++]);
                var bottomVerticalSplitter = double.Parse(values[valueIndex++]);
                var horizontalSplitter = double.Parse(values[valueIndex++]);

                int paneCount = int.Parse(values[valueIndex++]);
                if(paneCount != 4)
                {
                    throw new InvalidOperationException($"Expected 4 panes, but found {paneCount}");
                }

                var panes = new PanelConfiguration[paneCount];
                for (int j = 0; j < panes.Length; j++)
                {
                    var uri = Encoding.Unicode.GetString(Convert.FromBase64String(values[valueIndex++]));
                    var refreshFreq = int.Parse(values[valueIndex++]);
                    var scale = int.Parse(values[valueIndex++]);
                    var scaleMaximized = int.Parse(values[valueIndex++]);
                    panes[j] = new PanelConfiguration(uri: uri, refreshFreq: refreshFreq, scale: scale, scaleMaximized: scaleMaximized);
                }

                windows[i] = new WindowConfiguration(
                    topVerticalSplitter: topVerticalSplitter, 
                    bottomVerticalSplitter: bottomVerticalSplitter, 
                    horizontalSplitter: horizontalSplitter,
                    panes: panes);
            }
            config.Windows.Clear();
            config.Windows.AddRange(windows);
            return config;
        }
    }
}
