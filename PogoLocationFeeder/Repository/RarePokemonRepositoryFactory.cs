using System.Collections.Generic;
using PogoLocationFeeder.Config;

namespace PogoLocationFeeder.Repository
{
    public class RarePokemonRepositoryFactory
    {
        public static List<IRarePokemonRepository> CreateRepositories(GlobalSettings globalSettings)
        {
            var rarePokemonRepositories = new List<IRarePokemonRepository>();
            if (GlobalSettings.UsePokeSnipers)
            {
                rarePokemonRepositories.Add(new PokeSniperRarePokemonRepository());
            }
            if (GlobalSettings.UseTrackemon)
            {
                rarePokemonRepositories.Add(new TrackemonRarePokemonRepository());
            }
            if (GlobalSettings.UsePokezz)
            {
                rarePokemonRepositories.Add(new PokezzRarePokemonRepository());
            }
            if (GlobalSettings.UsePokeSpawns)
            {
                rarePokemonRepositories.Add(new PokeSpawnsRarePokemonRepository());
            }
            if (GlobalSettings.UsePokewatchers)
            {
                rarePokemonRepositories.Add(new PokewatchersRarePokemonRepository());
            }
            if (GlobalSettings.UseSkiplagged)
            {
                rarePokemonRepositories.Add(new SkiplaggedPokemonRepository(pokemonIds));
            }

            return rarePokemonRepositories;
        }
    }
}