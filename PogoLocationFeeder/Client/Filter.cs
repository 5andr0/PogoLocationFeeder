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
using PogoLocationFeeder.Common;

namespace PogoLocationFeeder.Client
{
    public class Filter
    {
        public string Pokemon { get; set; }
        public List<Channel> Channels;
        public bool VerifiedOnly { get; set; }
        public string Version { get; set; }
        public LatLngBounds AreaBounds;
        public double PokemonNotInFilterMinimumIV { get; set; } = 101;

        public double MinimumIV { get; set; } = 0.0;
        public bool UseUploadedPokemon { get; set; } = true;
        public bool UnverifiedOnly { get; set; } = false;
    }

    public class Channel
    {
        public string Server { get; set; }
        public string ChannelName { get; set; }
    }
}
