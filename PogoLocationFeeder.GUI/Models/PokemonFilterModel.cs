using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using PropertyChanged;

namespace PogoLocationFeeder.GUI.Models {
    [ImplementPropertyChanged]
    public class PokemonFilterModel {
        [JsonIgnore]
        public BitmapImage Image { get; set; }
        public string Name { get; set; }

        public PokemonFilterModel(string name, BitmapImage img, bool filtered) {
            Image = img;
            Name = name;
        }
    }
}
