using System.Windows;
using System.Windows.Controls;
using PogoLocationFeeder.Config;

namespace PogoLocationFeeder.GUI.Views
{
    /// <summary>
    ///     Interaktionslogik für MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            if (GlobalSettings.isOneClickSnipeSupported())
            {
                col.Visibility = Visibility.Visible;
            }
            else
            {
                col.Visibility = Visibility.Collapsed;
            }
        }
    }
}