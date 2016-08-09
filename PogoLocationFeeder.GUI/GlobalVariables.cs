using System.Collections.ObjectModel;
using PogoLocationFeeder.GUI.Common;
using PogoLocationFeeder.GUI.Models;

namespace PogoLocationFeeder.GUI
{
    public static class GlobalVariables
    {
        public static ObservableCollection<SniperInfoModel> PokemonsInternal =
            new ObservableCollection<SniperInfoModel>();
        public static ObservableCollection<PokemonFilterModel> PokemonFilterInternal =
            new ObservableCollection<PokemonFilterModel>(PokemonFilter.GetPokemons());
        public static ObservableCollection<PokemonFilterModel> PokemonToFilterInternal =
            new ObservableCollection<PokemonFilterModel>();
    }
}