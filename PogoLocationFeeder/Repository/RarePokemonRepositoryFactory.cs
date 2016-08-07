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
            if (GlobalSettings.UsePokezz)
            {
                rarePokemonRepositories.Add(new PokezzRarePokemonRepository(pokemonIds));
            }
            if (GlobalSettings.UsePokeSpawns)
            {
                rarePokemonRepositories.Add(new PokeSpawnsRarePokemonRepository(pokemonIds));
            }
            if (GlobalSettings.UsePokewatchers)
            {
                rarePokemonRepositories.Add(new PokewatchersRarePokemonRepository(pokemonIds));
            }
            return rarePokemonRepositories;
        }
    }
}