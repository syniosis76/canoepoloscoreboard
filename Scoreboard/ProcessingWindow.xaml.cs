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

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for ProcessingWindow.xaml
    /// </summary>
    public partial class ProcessingWindow: Window
    {
        private static ProcessingWindow _processingWindow;

        public ProcessingWindow()
        {
            InitializeComponent();
        }

        public static void ShowProcessing(Window owner, string message)
        {
            if (_processingWindow == null)
            {
                _processingWindow = new ProcessingWindow();
            }
            _processingWindow.Owner = owner;
            _processingWindow.SelectLabel.Content = message;
            _processingWindow.Show();
        }

        public static void HideProcessing()
        {
            if (_processingWindow != null)
            {
                _processingWindow.Hide();
            }
        }
    }
}
