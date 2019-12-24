using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace BrowserScreenSaver
{
    public class AppConfiguration
    {
        public class SharedConfiguration
        {
            public double TopVerticalSplitter { get; set; } = 0.5;
            public double BottomVerticalSplitter { get; set; } = 0.5;
            public double HorizontalSplitter { get; set; } = 0.5;
            public bool OnResumeDisplayLogon { get; set; } = true;
            public DateTime NavigationEnabledByUtc { get; set; } = new DateTime(year: 2000, month: 1, day: 1);
            public List<Uri> SafeUris { get; } = new List<Uri>();
        }

        public class PaneConfiguration
        {
            public string Uri { get; set; }
            public int RefreshFreq { get; set; } = -1;
            public int Scale { get; set; } = 100;
            public int ScaleMaximized { get; set; } = 100;
        }

        public SharedConfiguration SharedConfig { get; } = new SharedConfiguration();
        public IReadOnlyList<PaneConfiguration> Panes { get; }

        public AppConfiguration()
        {
            var panes = new PaneConfiguration[8];
            for(int i=0; i< panes.Length; i++)
            {
                panes[i] = new PaneConfiguration();
            }

            this.Panes = panes;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("v1");
            sb.AppendLine(this.SharedConfig.TopVerticalSplitter.ToString());
            sb.AppendLine(this.SharedConfig.BottomVerticalSplitter.ToString());
            sb.AppendLine(this.SharedConfig.HorizontalSplitter.ToString());
            sb.AppendLine(this.SharedConfig.OnResumeDisplayLogon.ToString());
            sb.AppendLine(this.SharedConfig.NavigationEnabledByUtc.ToString());
            sb.AppendLine(this.SharedConfig.SafeUris.Count.ToString());
            foreach(var uri in this.SharedConfig.SafeUris)
            {
                sb.AppendLine(Convert.ToBase64String(Encoding.Unicode.GetBytes(uri.ToString())));
            }

            foreach (var pane in this.Panes)
            {
                sb.AppendLine(Convert.ToBase64String(Encoding.Unicode.GetBytes(pane.Uri ?? string.Empty)));
                sb.AppendLine(pane.RefreshFreq.ToString());
                sb.AppendLine(pane.Scale.ToString());
                sb.AppendLine(pane.ScaleMaximized.ToString());
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
            config.SharedConfig.TopVerticalSplitter = double.Parse(values[valueIndex++]);
            config.SharedConfig.BottomVerticalSplitter = double.Parse(values[valueIndex++]);
            config.SharedConfig.HorizontalSplitter = double.Parse(values[valueIndex++]);
            config.SharedConfig.OnResumeDisplayLogon = bool.Parse(values[valueIndex++]);
            config.SharedConfig.NavigationEnabledByUtc = DateTime.Parse(values[valueIndex++]);
            var uriCount = int.Parse(values[valueIndex++]);
            for (int i = 0; i < uriCount; i++)
            {
                var safeUri = new Uri(Encoding.Unicode.GetString(Convert.FromBase64String(values[valueIndex++])), UriKind.Absolute);
                config.SharedConfig.SafeUris.Add(safeUri);
            }

            foreach (var pane in config.Panes)
            {
                pane.Uri = Encoding.Unicode.GetString(Convert.FromBase64String(values[valueIndex++]));
                pane.RefreshFreq = int.Parse(values[valueIndex++]);
                pane.Scale = int.Parse(values[valueIndex++]);
                pane.ScaleMaximized = int.Parse(values[valueIndex++]);
            }
         
            return config;
        }
    }
}
