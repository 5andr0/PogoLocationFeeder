using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using POGOProtos.Enums;
using PropertyChanged;

namespace PogoLocationFeeder.GUI.Models {
    [ImplementPropertyChanged]
    public class PokemonFilterModel {
        [JsonIgnore]
        public BitmapImage Image { get; set; }
        public PokemonId Id { get; set; }

        public PokemonFilterModel(PokemonId id, BitmapImage img) {
            Image = img;
            Id = id;
        }
    }
}
