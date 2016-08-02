using System.Collections.ObjectModel;
using PogoLocationFeeder.GUI.Models;

namespace PogoLocationFeeder.GUI {
    public static class GlobalVariables {
        public static ObservableCollection<SniperInfoModel> PokemonsInternal =
            new ObservableCollection<SniperInfoModel>();
    }
}