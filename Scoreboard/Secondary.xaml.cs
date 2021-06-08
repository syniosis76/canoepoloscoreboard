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
    /// Interaction logic for Secondary.xaml
    /// </summary>
    public partial class Secondary : Window
    {
        public KeyEventHandler SecondaryKeyUp;

        public Secondary()
        {
            InitializeComponent();
        }

        private void _closeLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SecondaryKeyUp?.Invoke(sender, e);
        }
    }
}
