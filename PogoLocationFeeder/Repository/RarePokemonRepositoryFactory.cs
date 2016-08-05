using System.Collections.Generic;
using PogoLocationFeeder.Config;

namespace PogoLocationFeeder.Repository
{
    public class RarePokemonRepositoryFactory
    {
        public static List<IRarePokemonRepository> CreateRepositories(GlobalSettings globalSettings)
        {
            var rarePokemonRepositories = new List<IRarePokemonRepository>();
            var pokemonIds = RarePokemonsFactory.createRarePokemonList();
            if (GlobalSettings.UsePokeSnipers)
            {
                rarePokemonRepositories.Add(new PokeSniperRarePokemonRepository(pokemonIds));
            }
            if (GlobalSettings.UseTrackemon)
            {
                rarePokemonRepositories.Add(new TrackemonRarePokemonRepository(pokemonIds));
            }
            return rarePokemonRepositories;
        }
    }
}