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

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for ScoreboardServerWindow.xaml
    /// </summary>
    public partial class ScoreboardServerWindow : Window
    {
        private Score _score;
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

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            Score.StartStopServer();
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
