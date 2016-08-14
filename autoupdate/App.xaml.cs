using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using autoupdate.ViewModels;

namespace autoupdate {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {
        private void AppStartup(object sender, StartupEventArgs args) {
            if(Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) {
                MessageBox.Show(AppDomain.CurrentDomain.FriendlyName + " is already running. Application will now close.", "Application running!", MessageBoxButton.OK, MessageBoxImage.Stop);
                Current.Shutdown();
            } else {
                //Waiting for the settings to be loaded
                Thread.Sleep(1000);
                var mainWindow = new MainWindow {
                    DataContext = new MainWindowViewModel()
                };
                mainWindow.Show();
            }
        }
    }
}
