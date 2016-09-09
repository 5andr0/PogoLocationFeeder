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
using Newtonsoft.Json;
using POGOProtos.Enums;

namespace PogoLocationFeeder.API
{
    public class SniperInfoModel
    {
        [JsonProperty("ExpirationTimestamp")]
        public DateTime ExpirationTimestamp { get; set; }
        [JsonProperty("Latitude")]
        public double Latitude { get; set; }
        [JsonProperty("Longitude")]
        public double Longitude { get; set; }
        [JsonProperty("Id")]
        public PokemonId Id { get; set; } = PokemonId.Missingno;
        [JsonProperty("IV")]
        public double IV { get; set; }
        [JsonProperty("Move1")]
        public PokemonMove Move1 { get; set; }
        [JsonProperty("Move2")]
        public PokemonMove Move2 { get; set; }

    }
}
