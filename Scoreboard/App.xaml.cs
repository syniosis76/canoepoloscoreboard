using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Avalonia;
using Avalonia.Logging.Serilog;
using Application = System.Windows.Application;

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

            // Initialise Avalonia
            AppBuilder.Configure<Avalonia.Application>()                
                .UseWin32()
                .UseDirect2D1()                
                .LogToDebug(Avalonia.Logging.LogEventLevel.Verbose)
                .SetupWithoutStarting();

            Score score = new Score();
            try
            {                
                score.Initialise();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            MainWindow window = new MainWindow();
            window.Score = score;            
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
