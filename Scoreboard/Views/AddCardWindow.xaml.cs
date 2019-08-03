using Avalonia;
using Avalonia.Controls;
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
    }
}
