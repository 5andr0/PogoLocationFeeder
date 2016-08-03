using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Properties;
using UserControl = System.Windows.Controls.UserControl;

namespace PogoLocationFeeder.GUI.Views
{
    /// <summary>
    ///     Interaktionslogik für SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private string OpenFolderDialog() {
            using(var f = new FolderBrowserDialog()) {
                return f.ShowDialog() == DialogResult.OK ? f.SelectedPath : null;
            }
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var p = OpenFolderDialog();
            if (Directory.Exists(p)) {
                if (File.Exists(Path.Combine(p, "PokeSniper2.exe"))) {
                    path.Text = Path.Combine(p, "PokeSniper2.exe");
                    GlobalSettings.PokeSnipers2exe = Path.Combine(p, "PokeSniper2.exe");
                    Settings.Default.Sniper2Path = Path.Combine(p, "PokeSniper2.exe");
                    Settings.Default.Save();
                    //GlobalSettings.Settings.Save();
                    return;
                }
            }
        }
    }
}