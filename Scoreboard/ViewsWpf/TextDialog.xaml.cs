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
using System.Threading;
using System.Windows.Threading;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for TextDialog.xaml
    /// </summary>
    public partial class TextDialog : Window
    {
        public Action ActionDelegate { get; set; }
        public string ActionMessage { get; set; }

        public static bool ShowTextDialog(Window owner, string caption, string label, ref string text)
        {
            TextDialog textDialog = new TextDialog(caption, label, text);
            textDialog.Owner = owner;
            if (textDialog.ShowDialog() == true)
            {
                text = textDialog.Text;
                return true;
            }
            return false;
        }

        public TextDialog(string caption, string label, string text)
        {
            InitializeComponent();
            Title = caption;
            _textLabel.Content = label;
            Text = text;
            _actionMessageLabel.Content = String.Empty;
            _textTextBox.Focus();
        }

        public string Text
        {
            get
            {
                return _textTextBox.Text;
            }
            set
            {
                _textTextBox.Text = value;
            }
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            if (ActionDelegate != null)
            {
                IsEnabled = false;
                _actionMessageLabel.Content = ActionMessage;

                ThreadPool.QueueUserWorkItem(delegate
                {
                    Thread.Sleep(10);
                    Dispatcher.Invoke(DispatcherPriority.Send, (Action)delegate
                    {                        
                        ActionDelegate();
                        DialogResult = true;
                    });
                }, null);                               
            }
            else
            {
                DialogResult = true;
            }
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
