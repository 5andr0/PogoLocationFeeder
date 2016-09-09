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

using System.Collections.Generic;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Repository
{
    public class RarePokemonsFactory
    {
        public static List<PokemonId> createRarePokemonList()
        {
            var rarePokemonIds = new List<PokemonId>
            {
                PokemonId.Onix,
                PokemonId.Venusaur,
                PokemonId.Charizard,
                PokemonId.Blastoise,
                PokemonId.Raichu,
                PokemonId.Nidoqueen,
                PokemonId.Nidoking,
                PokemonId.Ninetales,
                PokemonId.Wigglytuff,
                PokemonId.Dugtrio,
                PokemonId.Arcanine,
                PokemonId.Alakazam,
                PokemonId.Rapidash,
                PokemonId.Slowbro,
                PokemonId.Magneton,
                PokemonId.Farfetchd,
                PokemonId.Dewgong,
                PokemonId.Grimer,
                PokemonId.Muk,
                PokemonId.Haunter,
                PokemonId.Gengar,
                PokemonId.Exeggutor,
                PokemonId.Marowak,
                PokemonId.Hitmonlee,
                PokemonId.Hitmonchan,
                PokemonId.Lickitung,
                PokemonId.Chansey,
                PokemonId.Kangaskhan,
                PokemonId.MrMime,
                PokemonId.Gyarados,
                PokemonId.Lapras,
                PokemonId.Ditto,
                PokemonId.Vaporeon,
                PokemonId.Jolteon,
                PokemonId.Flareon,
                PokemonId.Porygon,
                PokemonId.Snorlax,
                PokemonId.Articuno,
                PokemonId.Zapdos,
                PokemonId.Moltres,
                PokemonId.Dratini,
                PokemonId.Dragonair,
                PokemonId.Dragonite,
                PokemonId.Mewtwo,
                PokemonId.Mew
            };
            return rarePokemonIds;
        }
    }
}
