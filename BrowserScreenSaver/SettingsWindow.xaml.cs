using System;
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

        public SettingsWindow()
        {
            InitializeComponent();
            this.Uri1.Text = Properties.Settings.Default.Uri1;
            this.Uri2.Text = Properties.Settings.Default.Uri2;
            this.Uri3.Text = Properties.Settings.Default.Uri3;
            this.Uri4.Text = Properties.Settings.Default.Uri4;
            this.OnResume.IsChecked = Properties.Settings.Default.OnResumeDisplayLogon;
            this.AllowPopups.IsChecked = Properties.Settings.Default.AllowPopups;
            this.UpdateNavigationEnabledText();
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Uri1 = SettingsWindow.GetUriSetting(this.Uri1.Text) ?? Properties.Settings.Default.Uri1;
            Properties.Settings.Default.Uri2 = SettingsWindow.GetUriSetting(this.Uri2.Text) ?? Properties.Settings.Default.Uri2;
            Properties.Settings.Default.Uri3 = SettingsWindow.GetUriSetting(this.Uri3.Text) ?? Properties.Settings.Default.Uri3;
            Properties.Settings.Default.Uri4 = SettingsWindow.GetUriSetting(this.Uri4.Text) ?? Properties.Settings.Default.Uri4;
            Properties.Settings.Default.OnResumeDisplayLogon = this.OnResume.IsChecked ?? true;
            Properties.Settings.Default.AllowPopups = this.AllowPopups.IsChecked ?? true;
            Properties.Settings.Default.Save();
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
            Properties.Settings.Default.NavigationEnabledByUtc = DateTime.UtcNow + SettingsWindow.EnableNavigationTimeSpan;
            UpdateNavigationEnabledText();
        }

        private void UpdateNavigationEnabledText()
        {
            this.NavigationEnabledTextBlock.Text = Properties.Settings.Default.NavigationEnabledByUtc > DateTime.UtcNow
                ? $"Enabled until {Properties.Settings.Default.NavigationEnabledByUtc.ToLocalTime()}"
                : "Browser navigation is disabled";
        }

        private void Uri_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var border = (Border)textBox.Parent;

            border.BorderBrush = SettingsWindow.GetUriSetting(textBox.Text) == null ? Brushes.Red : Brushes.Transparent;
        }
    }
}
