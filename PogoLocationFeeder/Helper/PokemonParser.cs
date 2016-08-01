using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder
{

    public class PokemonParser
    {
        static List<PokemonAlternativeNames> pokemonAlternativeNamesList = new List<PokemonAlternativeNames>()
        {
            { PokemonAlternativeNames.createAllMatches(PokemonId.Farfetchd, new HashSet<String> { "Farfetch'd" }, new HashSet<String> { "Farfetch" })},
            { PokemonAlternativeNames.createAllMatches(PokemonId.MrMime,  new HashSet<String> { "mr.Mime", "mr mime" }, new HashSet<String> { "Mime" })},
        };

        public static PokemonId parsePokemon(String input)
        {
            foreach (string name in Enum.GetNames(typeof(PokemonId)))
            {
                if (matchesPokemonNameExactly(input, name))
                {
                    return (PokemonId)Enum.Parse(typeof(PokemonId), name);
                }
            }
            foreach (PokemonAlternativeNames pokemonAlternativeNames in pokemonAlternativeNamesList)
            {
                if(pokemonAlternativeNames.exactMatches != null)
                {
                    foreach(String exactMatch in pokemonAlternativeNames.exactMatches)
                    {
                        if(matchesPokemonNameExactly(input, exactMatch))
                        {
                            return pokemonAlternativeNames.pokemonId;
                        }
                    }
                }
                if (pokemonAlternativeNames.partialMatches != null)
                {
                    foreach (String partialMatch in pokemonAlternativeNames.partialMatches)
                    {
                        if (matchesPokemonNamePartially(input, partialMatch))
                        {
                            return pokemonAlternativeNames.pokemonId;
                        }
                    }
                }
            }


            return PokemonId.Missingno;
        }

        private static bool matchesPokemonNameExactly(String input, String name)
        {
            return Regex.IsMatch(input, @"(?i)\b" + name + @"\b");
        }

        private static bool matchesPokemonNamePartially(String input, String name)
        {
            return Regex.IsMatch(input, @"(?i)" + name);
        }

        internal class PokemonAlternativeNames
        {
            internal PokemonId pokemonId { get; }
            internal ISet<String> exactMatches { get; }
            internal ISet<String> partialMatches { get; }

            internal PokemonAlternativeNames(PokemonId pokemonId, ISet<String> exactMatches, ISet<String> partialMatches)
            {
                this.pokemonId = pokemonId;
                this.exactMatches = exactMatches;
                this.partialMatches = partialMatches;
            }

            internal static PokemonAlternativeNames createAllMatches(PokemonId pokemonId, ISet<String> exactMatches, ISet<String> partialMatches)
            {
                return new PokemonAlternativeNames(pokemonId, exactMatches, partialMatches);
            }
            internal static PokemonAlternativeNames createPartialMatches(PokemonId pokemonId, ISet<String> partialMatches)
            {
                return new PokemonAlternativeNames(pokemonId, null, partialMatches);
            }

            internal static PokemonAlternativeNames createExactMatches(PokemonId pokemonId, ISet<String> exactMatches)
            {
                return new PokemonAlternativeNames(pokemonId, exactMatches, null);
            }
        }
    }
}
