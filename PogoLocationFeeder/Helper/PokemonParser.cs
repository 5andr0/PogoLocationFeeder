/*
PogoLocationFeeder gathers pokemon data from various sources and serves it to connected clients
Copyright (C) 2016  PogoLocationFeeder Development Team <admin@pokefeeder.live>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
                    return (PokemonId) Enum.Parse(typeof(PokemonId), name, true);
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
