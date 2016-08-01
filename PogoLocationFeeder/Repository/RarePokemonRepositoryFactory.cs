using PoGo.LocationFeeder.Settings;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Repository
{
   public class RarePokemonRepositoryFactory
    {

        public static List<RarePokemonRepository> createRepositories(GlobalSettings globalSettings)
        {
            List<RarePokemonRepository> rarePokemonRepositories = new List<RarePokemonRepository>();
            List<PokemonId> pokemonIds = RarePokemonsFactory.createRarePokemonList();
            if (globalSettings.usePokeSnipers)
            {
                rarePokemonRepositories.Add(new PokeSniperRarePokemonRepository(pokemonIds));
            }
            if (globalSettings.useTrackemon)
            {
                rarePokemonRepositories.Add(new TrackemonRarePokemonRepository(pokemonIds));
            }
            return rarePokemonRepositories;
        }

       


    }
}
