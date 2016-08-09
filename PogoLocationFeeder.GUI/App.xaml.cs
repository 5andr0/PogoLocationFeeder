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
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            GlobalSettings.Load();
            mainWindow.Show();
        }
    }
}