using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Window = Avalonia.Controls.Window;

namespace Scoreboard.Views
{
    public class AddCardWindow : Window
    {
        public AddCardWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OkButtonClick(object sender, RoutedEventArgs args)
        {
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs args)
        {
            Close();
        }
    }
}
