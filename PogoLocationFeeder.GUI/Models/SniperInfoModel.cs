using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MangaChecker.ViewModels;

namespace PogoLocationFeeder.GUI.Models {
    public class SniperInfoModel {
        private SniperInfo _info;
        public BitmapImage Icon { get; set; }
        public string Source { get; set; }

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
        }

        public void CopyCoords() {
            Clipboard.SetText($"{Info.Latitude}, {Info.Longitude}");
        }

        public ICommand copyCoordsCommand { get; }
    }
}