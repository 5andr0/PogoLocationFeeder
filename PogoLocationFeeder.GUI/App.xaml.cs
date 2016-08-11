using System.Threading;
using System.Windows;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.ViewModels;

namespace PogoLocationFeeder.GUI
{
    /// <summary>
    ///     Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void AppStartup(object sender, StartupEventArgs args)
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