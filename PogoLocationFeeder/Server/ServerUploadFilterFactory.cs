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
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Server
{
    class ServerUploadFilterFactory
    {
        public static ServerUploadFilter Create(List<PokemonId> pokemon)
        {
            ServerUploadFilter serverUploadFilter = new ServerUploadFilter();

            var pokemonsBinary = PokemonFilterToBinary.ToBinary(pokemon);

            serverUploadFilter.Pokemon = pokemonsBinary;
            serverUploadFilter.AreaBounds = GlobalSettings.UseGeoLocationBoundsFilter ? GlobalSettings.GeoLocationBounds : null;

            return serverUploadFilter;
        }
    }
}
