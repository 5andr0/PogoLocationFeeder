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
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Properties;
using UserControl = System.Windows.Controls.UserControl;

namespace PogoLocationFeeder.GUI.Views
{
    /// <summary>
    ///     Interaktionslogik für MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private DataGridColumn _sortableColumn;
        private ListSortDirection _direction;

        public MainView() {
            InitializeComponent();
        }

        private void dataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            _sortableColumn = e.Column;
            _direction = (e.Column.SortDirection != ListSortDirection.Ascending) ?
                                ListSortDirection.Ascending : ListSortDirection.Descending;

        }

        private void DataGridView1_CellValueChanged(
    object sender, DataGridViewCellEventArgs e)
        {
            if (_sortableColumn != null && _direction != null)
            {
                _sortableColumn.SortDirection = _direction;
            }
        }

    }
}
