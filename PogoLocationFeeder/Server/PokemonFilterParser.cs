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
using POGOProtos.Enums;
using System.Collections.Generic;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Server
{
    public class PokemonFilterParser
    {
        private static readonly int pokemonSize = Enum.GetValues(typeof(PokemonId)).Length;

        public static List<PokemonId> ParseBinary(string binairy)
        {
            if (binairy.Length != pokemonSize)
            {
                throw new Exception("Needs to be at least 3 times as big");
            }
            List<PokemonId> pokemonId = new List<PokemonId>();
            var bins = binairy.ToCharArray();

            for (int i = 0; i < pokemonSize; i++)
            {
                if (bins[i] =='1')
                {
                    pokemonId.Add(PokemonParser.ParseById(i));
                }
            }
            return pokemonId;
        }
    }
}