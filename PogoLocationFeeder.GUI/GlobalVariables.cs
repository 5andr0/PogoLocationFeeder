using System.Collections.ObjectModel;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.GUI.Common;
using PogoLocationFeeder.GUI.Models;

namespace PogoLocationFeeder.GUI
{
    public enum SortMode
    {
        AlphabeticalAsc,
        AlphabeticalDesc,
        IdAsc,
        IdDesc
    }
    public static class GlobalVariables
    {
        public static SortMode SortMode = SortMode.IdAsc;

        public static ObservableCollection<SniperInfoModel> PokemonsInternal =
            new ObservableCollection<SniperInfoModel>();
        public static ObservableCollection<PokemonFilterModel> AllPokemonsInternal =
            new ObservableCollection<PokemonFilterModel>(PokemonFilter.GetPokemons());
        public static ObservableCollection<PokemonFilterModel> PokemonToFeedFilterInternal =
            new ObservableCollection<PokemonFilterModel>();
    }
}