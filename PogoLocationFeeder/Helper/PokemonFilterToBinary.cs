using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Helper
{
    public class PokemonFilterToBinary
    {

        public static string ToBinary(List<PokemonId> pokemonIds)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PokemonId pokemonId in Enum.GetValues(typeof(PokemonId)))
            {
                stringBuilder.Append(pokemonIds.Contains(pokemonId) ? "1": "0");
            }
            return stringBuilder.ToString();
        }

        public static string ToBinary()
        {
            throw new NotImplementedException();
        }
    }
}
