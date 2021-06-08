using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        private string _statistics;
        public string Statistics { get { return _statistics; } }

        public StatisticsWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static void ShowStatistics(Window owner, string statistics)
        {
            StatisticsWindow window = new StatisticsWindow
            {
                Owner = owner,
                _statistics = statistics
            };
            window.ShowDialog();
        }

        private void CopyToClipboardClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Statistics);
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
