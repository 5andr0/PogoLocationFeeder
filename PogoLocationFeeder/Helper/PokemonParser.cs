using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Helper
{
    public class PokemonParser
    {
        private static readonly List<PokemonAlternativeNames> PokemonAlternativeNamesList = new List
            <PokemonAlternativeNames>
        {
            PokemonAlternativeNames.CreateAllMatches(PokemonId.Farfetchd, new HashSet<string> {"Farfetch'd"},
                new HashSet<string> {"Farfetch"}),
            PokemonAlternativeNames.CreateAllMatches(PokemonId.MrMime, new HashSet<string> {"mr.Mime", "mr mime","Mr.Maleime"},
                new HashSet<string> {"Mime"}),
            PokemonAlternativeNames.CreateAllMatches(PokemonId.NidoranMale, new HashSet<string> {"nidoranm"}, new HashSet<string>()),
            PokemonAlternativeNames.CreateAllMatches(PokemonId.NidoranFemale, new HashSet<string> {"nidoranf"}, new HashSet<string>())

        };

        public static List<PokemonId> ParsePokemons(List<string> inputs)
        {
            List<PokemonId> newPokemonIds = new List<PokemonId>();
            foreach (var input in inputs)
            {
                try
                {
                    newPokemonIds.Add(ParsePokemon(input, false, true));
                }catch(Exception e) { }
                
            }
            return newPokemonIds;
        }
        public static PokemonId ParsePokemon(string input, bool showError = false, bool throwException = false)
        {
            foreach (var name in Enum.GetNames(typeof(PokemonId)))
            {
                if (MatchesPokemonNameExactly(input, name))
                {
                    return (PokemonId) Enum.Parse(typeof(PokemonId), name);
                }
            }
            foreach (var pokemonAlternativeNames in PokemonAlternativeNamesList)
            {
                if (pokemonAlternativeNames.ExactMatches != null)
                {
                    foreach (var exactMatch in pokemonAlternativeNames.ExactMatches)
                    {
                        if (MatchesPokemonNameExactly(input, exactMatch))
                        {
                            return pokemonAlternativeNames.PokemonId;
                        }
                    }
                }
                if (pokemonAlternativeNames.PartialMatches != null)
                {
                    foreach (var partialMatch in pokemonAlternativeNames.PartialMatches)
                    {
                        if (MatchesPokemonNamePartially(input, partialMatch))
                        {
                            return pokemonAlternativeNames.PokemonId;
                        }
                    }
                }
            }

            if (showError)
            {
                Log.Error($"No pokemon found with name {input}");
            }
            if (throwException)
            {
                throw new Exception($"No pokemon found with name {input}");
            }
            return PokemonId.Missingno;
        }

        private static bool MatchesPokemonNameExactly(string input, string name)
        {
            return Regex.IsMatch(input, @"(?i)\b" + name + @"\b");
        }

        private static bool MatchesPokemonNamePartially(string input, string name)
        {
            return Regex.IsMatch(input, @"(?i)" + name);
        }

        public static PokemonId ParseById(long pokemonId)
        {
            return (PokemonId) pokemonId;
        }

        internal class PokemonAlternativeNames
        {
            internal PokemonAlternativeNames(PokemonId pokemonId, ISet<string> exactMatches, ISet<string> partialMatches)
            {
                this.PokemonId = pokemonId;
                this.ExactMatches = exactMatches;
                this.PartialMatches = partialMatches;
            }

            internal PokemonId PokemonId { get; }
            internal ISet<string> ExactMatches { get; }
            internal ISet<string> PartialMatches { get; }

            internal static PokemonAlternativeNames CreateAllMatches(PokemonId pokemonId, ISet<string> exactMatches,
                ISet<string> partialMatches)
            {
                return new PokemonAlternativeNames(pokemonId, exactMatches, partialMatches);
            }

            internal static PokemonAlternativeNames CreatePartialMatches(PokemonId pokemonId,
                ISet<string> partialMatches)
            {
                return new PokemonAlternativeNames(pokemonId, null, partialMatches);
            }

            internal static PokemonAlternativeNames CreateExactMatches(PokemonId pokemonId, ISet<string> exactMatches)
            {
                return new PokemonAlternativeNames(pokemonId, exactMatches, null);
            }
        }
    }
}