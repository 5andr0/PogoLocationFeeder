using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PogoLocationFeeder.GUI.Common;
using PogoLocationFeeder.GUI.Models;

namespace PogoLocationFeeder.GUI.Views {
    /// <summary>
    /// Interaktionslogik für FilterView.xaml
    /// </summary>
    public partial class FilterView : UserControl {
        public FilterView() {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var filter = GlobalVariables.PokemonToFilterInternal;
            if(AllPokes.SelectedIndex != -1 && !filter.Contains((PokemonFilterModel)AllPokes.SelectedItem)) {
                filter.Add((PokemonFilterModel)AllPokes.SelectedItem);
                PokemonFilter.Save();
            }
        }

        private void ListBox_MouseDoubleClick_1(object sender, MouseButtonEventArgs e) {
            if(FilterPokes.SelectedIndex != -1) {
                GlobalVariables.PokemonToFilterInternal.Remove((PokemonFilterModel)FilterPokes.SelectedItem);
                PokemonFilter.Save();
            }
        }
    }
}
