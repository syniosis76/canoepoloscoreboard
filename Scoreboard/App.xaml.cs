using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Threading;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;

            Score score = new Score();
            try
            {                
                score.Initialise();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            MainWindow window = new MainWindow
            {
                Score = score
            };
            window.Show();           
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string message = e.Exception.Message + "\n\n" + e.Exception.StackTrace.ToString();

            MessageBox.Show(message);  

            // Prevent default unhandled exception processing
            e.Handled = true;
        }
    }
}
