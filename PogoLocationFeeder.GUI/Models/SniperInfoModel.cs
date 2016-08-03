using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MangaChecker.ViewModels;
using PoGo.LocationFeeder.Settings;

namespace PogoLocationFeeder.GUI.Models {
    public class SniperInfoModel {
        private SniperInfo _info;
        public BitmapImage Icon { get; set; }
        public string Server { get; set; }
        public string Channel { get; set; }
        public bool SniperVisibility { get; set; }
        public SniperInfo Info {
            get { return _info; }
            set {
                _info = value;
                Date = Info.ExpirationTimestamp.Equals(default(DateTime)) ? "Unknown" : Info.ExpirationTimestamp.ToString();
                IV = Info.IV.Equals(0) ? "??" : Info.IV.ToString();
            }
        }

        public string Date { get; set; }

        public string IV { get; set; }

        public SniperInfoModel() {
            copyCoordsCommand = new ActionCommand(CopyCoords);
            PokeSnipersCommand = new ActionCommand(PokeSnipers);
            SniperVisibility = GlobalSettings.SniperVisibility;
        }

        public void CopyCoords() {
            Clipboard.SetText($"{Info.Latitude}, {Info.Longitude.ToString(CultureInfo.InvariantCulture)}");
        }
        public void PokeSnipers() {
            Process.Start($"pokesniper2://{Info.Id}/{Info.Latitude.ToString(CultureInfo.InvariantCulture)},{Info.Longitude.ToString(CultureInfo.InvariantCulture)}");
        }

        public ICommand copyCoordsCommand { get; }
        public ICommand PokeSnipersCommand { get; }
    }
}