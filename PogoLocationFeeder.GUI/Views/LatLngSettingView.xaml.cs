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
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Config;
using UserControl = System.Windows.Controls.UserControl;

namespace PogoLocationFeeder.GUI.Views
{
    /// <summary>
    /// Interaction logic for LatLngSettingView.xaml
    /// </summary>
    public partial class LatLngSettingView : UserControl
    {
        public LatLngSettingView()
        {
            InitializeComponent();
        }

        private void DoublelValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            double result;
            e.Handled = double.TryParse(e.Text, out result);
        }

        private void SetupObjectForScripting(object sender, EventArgs e)
        {

        }

        private void OnShow(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                WebBrowser1.NavigateToString(Properties.Resources.map);
            }
            catch (Exception)
            {
                //hmmm
            }

            WebBrowser1.ObjectForScripting = new HtmlInteropInternalClass();

        }

        // Object used for communication from JS -> WPF
        [System.Runtime.InteropServices.ComVisibleAttribute(true)]
        public class HtmlInteropInternalClass
        {
            public double defaultlat;
            public double defaultlng;
            public double swLat;
            public double swLng;
            public double neLat;
            public double neLng;

            public HtmlInteropInternalClass()
            {
                swLat = GlobalSettings.GeoLocationBounds?.SouthWest?.Latitude ?? 40.71461026176555;
                swLng = GlobalSettings.GeoLocationBounds?.SouthWest?.Longitude ?? -74.033173578125;
                neLat = GlobalSettings.GeoLocationBounds?.NorthEast?.Latitude ?? 40.750381950874413;
                neLng = GlobalSettings.GeoLocationBounds?.NorthEast?.Longitude ?? -73.981846826416017;
            }


            public void setMapInfo(double swLat, double swLng, double neLat, double neLng)
            {
                var sw = new GeoCoordinates(swLat, swLng);
                var ne = new GeoCoordinates(neLat, neLng);
                ViewModels.MainWindowViewModel.Instance.LocationBoundsSettingToSave = new LatLngBounds(sw, ne);
            }
        }
    }
}
