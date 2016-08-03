using System.Collections.ObjectModel;
using PogoLocationFeeder.Common.Models;

namespace PogoLocationFeeder.GUI
{
    public static class GlobalVariables
    {
        public static ObservableCollection<SniperInfoModel> PokemonsInternal =
            new ObservableCollection<SniperInfoModel>();
    }
}