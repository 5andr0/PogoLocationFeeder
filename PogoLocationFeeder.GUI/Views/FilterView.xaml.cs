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
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Common;
using PogoLocationFeeder.GUI.Models;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.GUI.Views {
    /// <summary>
    /// Interaktionslogik für FilterView.xaml
    /// </summary>
    public partial class FilterView : UserControl {
        public FilterView() {
            InitializeComponent();
        }

        private void AllPokes_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AllPokes.SelectedIndex == -1) return;

            var right = GlobalVariables.PokemonToFeedFilterInternal;
            var left = GlobalVariables.AllPokemonsInternal;

            var selected = (PokemonFilterModel)AllPokes.SelectedItem;
            GlobalSettings.PokekomsToFeedFilter.Add(selected.Id.ToString());

            int i = right.Count(x => (GlobalVariables.SortMode == SortMode.AlphabeticalAsc) ?
                x.Id.ToString().CompareTo(selected.Id.ToString()) < 0 :
                x.Id < selected.Id);
            right.Insert(i, selected);
            left.Remove(selected);

        }

        private void FilterPokes_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FilterPokes.SelectedIndex == -1) return;

            var right = GlobalVariables.PokemonToFeedFilterInternal;
            var left = GlobalVariables.AllPokemonsInternal;

            var selected = (PokemonFilterModel)FilterPokes.SelectedItem;
            GlobalSettings.PokekomsToFeedFilter.Remove(selected.Id.ToString());

            int i = left.Count(x => (GlobalVariables.SortMode == SortMode.AlphabeticalAsc) ? 
                x.Id.ToString().CompareTo(selected.Id.ToString()) < 0 : 
                x.Id < selected.Id);
            left.Insert(i, selected);
            right.Remove(selected);
        }
    }
}
