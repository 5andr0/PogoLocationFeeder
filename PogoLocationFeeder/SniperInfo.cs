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
using System.Globalization;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Helper
{
    public class SniperInfo
    {
        public ulong EncounterId { get; set; }
        public DateTime ExpirationTimestamp { get; set; } = default(DateTime);
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public PokemonId Id { get; set; } = PokemonId.Missingno;
        public string SpawnPointId { get; set; }
        public PokemonMove Move1 { get; set; }
        public PokemonMove Move2 { get; set; }
        public double IV { get; set; }
        public bool Verified { get; set; } = false;
        public ChannelInfo ChannelInfo { get; set; }
        public DateTime ReceivedTimeStamp { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return "SniperInfo: id: " + Id
                   + ", Latitude: " + Latitude.ToString("N6", CultureInfo.InvariantCulture)
                   + ", Longitude: " + Longitude.ToString("N6", CultureInfo.InvariantCulture)
                   + (IV != default(double) ? ", IV: " + IV + "%" : "")
                   + (ExpirationTimestamp != default(DateTime) ? ", expiration: " + ExpirationTimestamp : "");
        }
    }
}
