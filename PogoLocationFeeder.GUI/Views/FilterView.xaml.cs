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
