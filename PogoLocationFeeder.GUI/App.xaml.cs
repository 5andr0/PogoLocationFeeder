using System.Threading;
using System.Windows;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.ViewModels;
using System.Diagnostics;
using System;

namespace PogoLocationFeeder.GUI
{
    /// <summary>
    ///     Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void AppStartup(object sender, StartupEventArgs args)
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                MessageBox.Show(AppDomain.CurrentDomain.FriendlyName + " is already running. Application will now close.", "Application running!", MessageBoxButton.OK, MessageBoxImage.Stop);
                Application.Current.Shutdown();
            }
            else
            {
                //Waiting for the settings to be loaded
                Thread.Sleep(1000);
                var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };
                mainWindow.Show();
            }
        }
    }
}