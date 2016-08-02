using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PogoLocationFeeder.GUI.ViewModels;

namespace PogoLocationFeeder.GUI {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application {
        private void AppStartup(object sender, StartupEventArgs args) {
            var mainWindow = new MainWindow {
                DataContext = new MainWindowViewModel()
            };
            mainWindow.Show();
        }
    }
}
