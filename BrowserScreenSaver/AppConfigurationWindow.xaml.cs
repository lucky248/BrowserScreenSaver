using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrowserScreenSaver
{
    /// <summary>
    /// Interaction logic for AppConfigurationWindow.xaml
    /// </summary>
    public partial class AppConfigurationWindow : Window
    {
        private AppConfiguration config;
        private TextBox[][] monitorUris;

        public static TimeSpan EnableNavigationTimeSpan { get;  } = TimeSpan.FromMinutes(5);

        public AppConfigurationWindow()
        {
            InitializeComponent();
            monitorUris = new []
            {
                new[] { this.Monitor1Uri1, this.Monitor1Uri2, this.Monitor1Uri3, this.Monitor1Uri4},
                new[] { this.Monitor2Uri1, this.Monitor2Uri2, this.Monitor2Uri3, this.Monitor2Uri4},
                new[] { this.Monitor3Uri1, this.Monitor3Uri2, this.Monitor3Uri3, this.Monitor3Uri4},
            };
        }

        public void InitializeConfig(AppConfiguration config)
        {
            this.config = config;

            if(AppConfiguration.SupportedWindowCount != monitorUris.Length || AppConfiguration.SupportedWindowCount != config.Windows.Count)
            {
                MessageBox.Show($"Invalid configuration initialization. Expected {AppConfiguration.SupportedWindowCount} tabs (monitors)");
            }

            for(int monitorIdx = 0; monitorIdx< monitorUris.Length && monitorIdx < config.Windows.Count; monitorIdx++)
            {
                for(int i=0; i< 4; i++)
                {
                    monitorUris[monitorIdx][i].Text = config.Windows[monitorIdx].Panes[i].Uri;
                }
            }

            this.OnResume.IsChecked = config.SharedConfig.OnResumeDisplayLogon;
            this.SafeUris.Text = string.Join(Environment.NewLine, config.SharedConfig.SafeUris.Select(u => u.ToString()).ToArray());
            this.UpdateNavigationEnabledText();
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Only persist values back upon "OK"
            for (int monitorIdx = 0; monitorIdx < monitorUris.Length && monitorIdx < config.Windows.Count; monitorIdx++)
            {
                for (int i = 0; i < 4; i++)
                {
                    config.Windows[monitorIdx].Panes[i].Uri = AppConfigurationWindow.GetUriSetting(monitorUris[monitorIdx][i].Text) ?? config.Windows[monitorIdx].Panes[i].Uri;
                }
            }
            this.config.SharedConfig.OnResumeDisplayLogon = this.OnResume.IsChecked ?? true;
            this.config.SharedConfig.SafeUris.Clear();
            var uris = this.SafeUris.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => new Uri(s, UriKind.Absolute)).ToArray();
            this.config.SharedConfig.AddSafeUris(uris);
            this.Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private static string GetUriSetting(string uriText)
        {
            if (string.IsNullOrWhiteSpace(uriText))
            {
                return string.Empty;
            }

            Uri uri;
            if (Uri.TryCreate(uriText, UriKind.Absolute, out uri))
            {
                return uri.ToString();
            }

            return null;
        }

        private void EnableNavigationBtn_Click(object sender, RoutedEventArgs e)
        {
            this.config.SharedConfig.NavigationEnabledByUtc = DateTime.UtcNow + AppConfigurationWindow.EnableNavigationTimeSpan;
            UpdateNavigationEnabledText();
        }

        private void UpdateNavigationEnabledText()
        {
            this.NavigationEnabledTextBlock.Text = this.config.SharedConfig.NavigationEnabledByUtc > DateTime.UtcNow
                ? $"Enabled until {this.config.SharedConfig.NavigationEnabledByUtc.ToLocalTime()}"
                : "Browser navigation is disabled. Use 'Safe site prefixes' or temporary enable navigation to handle login pages.";
        }

        private void SafeUri_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var border = (Border)textBox.Parent;

            var uris = textBox.Text.Split(new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var errorFound = false;
            foreach(var uri in uris)
            {
                errorFound |= AppConfigurationWindow.GetUriSetting(uri) == null;
            }

            // Validate if specified URI is valid
            border.BorderBrush = errorFound ? Brushes.Red : Brushes.Transparent;
        }

        private void Uri_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var border = (Border)textBox.Parent;

            // Validate if specified URI is valid
            border.BorderBrush = AppConfigurationWindow.GetUriSetting(textBox.Text) == null ? Brushes.Red : Brushes.Transparent;
        }
    }
}
