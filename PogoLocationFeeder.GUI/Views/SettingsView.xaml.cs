/*
PogoLocationFeeder gathers pokemon data from various sources and serves it to connected clients
Copyright (C) 2016  PogoLocationFeeder Development Team <admin@pokefeeder.live>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
                    GlobalSettings.PokeSnipers2Exe = Path.Combine(p, "PokeSniper2.exe");
                    GlobalSettings.Save();
                    return;
                }
            }
        }

        private void DoubleValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            double result;
            e.Handled = double.TryParse(e.Text, out result);
        }
    }
}
