using System.Windows;
using System.Windows.Controls;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Properties;

namespace PogoLocationFeeder.GUI.Views
{
    /// <summary>
    ///     Interaktionslogik für MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView() {
            InitializeComponent();
                col.Visibility = Visibility.Visible;
            } else {
                col.Visibility = GlobalSettings.isOneClickSnipeSupported() ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}