using System;
using MaterialDesignThemes.Wpf;
using PogoLocationFeeder.Config;

namespace PogoLocationFeeder.GUI
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, System.Windows.RoutedEventArgs e) {

            try {
                switch(GlobalSettings.AppTheme.ToLower()) {
                    case "light":
                        new PaletteHelper().SetLightDark(false);
                        break;
                    case "dark":
                        new PaletteHelper().SetLightDark(true);
                        break;

                }

            } catch(Exception) {

            }
        }
    }
}