using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace BrowserScreenSaver
{
    public class AppConfiguration
    {
        public const int SupportedWindowCount = 3; // Config UX is hardcoded to this number of windows

        public class SharedConfiguration
        {
            private bool onResumeDisplayLogon = false;
            private DateTime navigationEnabledByUtc = new DateTime(year: 2000, month: 1, day: 1);

            public event EventHandler Changed;

            public bool OnResumeDisplayLogon
            {
                get
                {
                    return this.onResumeDisplayLogon;
                }
                set
                {
                    this.onResumeDisplayLogon = true;
                    this.Changed?.Invoke(this, EventArgs.Empty);
                }

            }
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

            public SharedConfiguration()
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

        public class PaneConfiguration
        {
            public string Uri { get; set; }
            public int RefreshFreq { get; set; }
            public int Scale { get; set; }
            public int ScaleMaximized { get; set; }

            public PaneConfiguration(string uri=null, int refreshFreq=-1, int scale=100, int scaleMaximized=100)
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
            public IReadOnlyList<PaneConfiguration> Panes{ get; }

            public WindowConfiguration(
                double topVerticalSplitter = 0.5, 
                double bottomVerticalSplitter = 0.5, 
                double horizontalSplitter = 0.5,
                PaneConfiguration[] panes = null)
            {
                this.TopVerticalSplitter = topVerticalSplitter;
                this.BottomVerticalSplitter = bottomVerticalSplitter;
                this.HorizontalSplitter = horizontalSplitter;

                panes = panes ?? new PaneConfiguration[4];
                if(panes.Length != 4)
                {
                    throw new InvalidOperationException($"Expected 4 panes, but constructed with {panes.Length}.");
                }

                for (int i = 0; i < panes.Length; i++)
                {
                    panes[i] = panes[i] ?? new PaneConfiguration();
                }

                this.Panes = panes;
            }
        }

        public SharedConfiguration SharedConfig { get; } = new SharedConfiguration();

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
            sb.AppendLine(this.SharedConfig.OnResumeDisplayLogon.ToString());
            sb.AppendLine(this.SharedConfig.NavigationEnabledByUtc.ToString());
            sb.AppendLine(this.SharedConfig.SafeUris.Count.ToString());
            foreach(var uri in this.SharedConfig.SafeUris)
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
            config.SharedConfig.OnResumeDisplayLogon = bool.Parse(values[valueIndex++]);
            config.SharedConfig.NavigationEnabledByUtc = DateTime.Parse(values[valueIndex++]);
            var uriCount = int.Parse(values[valueIndex++]);
            for (int i = 0; i < uriCount; i++)
            {
                var safeUri = new Uri(Encoding.Unicode.GetString(Convert.FromBase64String(values[valueIndex++])), UriKind.Absolute);
                config.SharedConfig.AddSafeUris(new[] { safeUri });
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

                var panes = new PaneConfiguration[paneCount];
                for (int j = 0; j < panes.Length; j++)
                {
                    var uri = Encoding.Unicode.GetString(Convert.FromBase64String(values[valueIndex++]));
                    var refreshFreq = int.Parse(values[valueIndex++]);
                    var scale = int.Parse(values[valueIndex++]);
                    var scaleMaximized = int.Parse(values[valueIndex++]);
                    panes[j] = new PaneConfiguration(uri: uri, refreshFreq: refreshFreq, scale: scale, scaleMaximized: scaleMaximized);
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
