using System.Windows.Media.Imaging;

namespace PogoLocationFeeder.GUI.Models {
    public class SniperInfoModel {
        public BitmapImage Icon { get; set; }
        public string Source { get; set; }
        public SniperInfo Info { get; set; }
    }
}