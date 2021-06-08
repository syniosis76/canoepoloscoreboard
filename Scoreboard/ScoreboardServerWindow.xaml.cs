using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Runtime.InteropServices;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for ScoreboardServerWindow.xaml
    /// </summary>
    public partial class ScoreboardServerWindow : Window
    {
        private readonly Score _score;
        public Score Score
        {
            get { return _score; }
        }

        public ScoreboardServerWindow(Score score)
        {
            _score = score;
            DataContext = Score.ServerOptions;
            InitializeComponent();
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Score.StartStopServer();
        }
    }
}
