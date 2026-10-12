using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Navigation;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly Score _score;
        public Score Score
        {
            get { return _score; }
        }

        public SettingsWindow(Score score)
        {
            _score = score;
            DataContext = Score;
            InitializeComponent();
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GrantPermissionClick(object sender, RoutedEventArgs e)
        {
            Score.GrantServerPermission();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string url = e.Uri.AbsoluteUri.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            e.Handled = true;
        }
    }
}
