using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrowserScreenSaver
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public static TimeSpan EnableNavigationTimeSpan { get;  } = TimeSpan.FromMinutes(5);
        
        private AppConfiguration.SharedConfiguration sharedConfig;
        
        private IReadOnlyList<AppConfiguration.PaneConfiguration> paneConfig;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public void InitializeConfig(AppConfiguration.SharedConfiguration sharedConfig, IReadOnlyList<AppConfiguration.PaneConfiguration> paneConfig)
        {
            this.paneConfig = paneConfig;
            this.sharedConfig = sharedConfig;
            this.Uri1.Text = paneConfig[0].Uri;
            this.Uri2.Text = paneConfig[1].Uri;
            this.Uri3.Text = paneConfig[2].Uri;
            this.Uri4.Text = paneConfig[3].Uri;
            this.OnResume.IsChecked = sharedConfig.OnResumeDisplayLogon;
            this.SafeUris.Text = string.Join(Environment.NewLine, sharedConfig.SafeUris.Select(u => u.ToString()).ToArray());
            this.UpdateNavigationEnabledText();
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Only persist values back upon "OK"
            paneConfig[0].Uri = SettingsWindow.GetUriSetting(this.Uri1.Text) ?? paneConfig[0].Uri;
            paneConfig[1].Uri = SettingsWindow.GetUriSetting(this.Uri2.Text) ?? paneConfig[1].Uri;
            paneConfig[2].Uri = SettingsWindow.GetUriSetting(this.Uri3.Text) ?? paneConfig[2].Uri;
            paneConfig[3].Uri = SettingsWindow.GetUriSetting(this.Uri4.Text) ?? paneConfig[3].Uri;

            sharedConfig.OnResumeDisplayLogon = this.OnResume.IsChecked ?? true;
            sharedConfig.SafeUris.Clear();
            var uris = this.SafeUris.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => new Uri(s, UriKind.Absolute)).ToArray();
            sharedConfig.SafeUris.AddRange(uris);
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
            this.sharedConfig.NavigationEnabledByUtc = DateTime.UtcNow + SettingsWindow.EnableNavigationTimeSpan;
            UpdateNavigationEnabledText();
        }

        private void UpdateNavigationEnabledText()
        {
            this.NavigationEnabledTextBlock.Text = this.sharedConfig.NavigationEnabledByUtc > DateTime.UtcNow
                ? $"Enabled until {this.sharedConfig.NavigationEnabledByUtc.ToLocalTime()}"
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
                errorFound |= SettingsWindow.GetUriSetting(uri) == null;
            }

            // Validate if specified URI is valid
            border.BorderBrush = errorFound ? Brushes.Red : Brushes.Transparent;
        }

        private void Uri_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var border = (Border)textBox.Parent;

            // Validate if specified URI is valid
            border.BorderBrush = SettingsWindow.GetUriSetting(textBox.Text) == null ? Brushes.Red : Brushes.Transparent;
        }
    }
}
