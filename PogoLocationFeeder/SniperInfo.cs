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
using System.Globalization;
using Newtonsoft.Json;
using PogoLocationFeeder.Common;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Helper
{
    public class SniperInfo
    {
        [JsonIgnore]
        public ulong EncounterId { get; set; }
        public DateTime ExpirationTimestamp { get; set; } = default(DateTime);
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public PokemonId Id { get; set; } = PokemonId.Missingno;
        [JsonIgnore]
        public string SpawnPointId { get; set; } = null;
        public PokemonMove Move1 { get; set; }
        public PokemonMove Move2 { get; set; }
        public double IV { get; set; }
        public bool Verified { get; set; } = false;
        public DateTime VerifiedOn { get; set; } = default(DateTime);
        public ChannelInfo ChannelInfo { get; set; }
        [JsonIgnore]
        public List<ChannelInfo> OtherChannelInfos { get; set; } = new List<ChannelInfo>();
        public DateTime ReceivedTimeStamp { get; set; } = DateTime.Now;
        public bool NeedVerification { get; set; } = false;

        public override string ToString()
        {
            return FormatPokemonLog(this);
        }

        private static string FormatPokemonLog(SniperInfo sniperInfo)
        {
            const string timeFormat = "HH:mm:ss";
            return $"{sniperInfo.ChannelInfo}: {sniperInfo.Id} at {sniperInfo.Latitude.ToString("N6", CultureInfo.InvariantCulture)},{sniperInfo.Longitude.ToString("N6", CultureInfo.InvariantCulture)}"
                   + " with " +
                   (!sniperInfo.IV.Equals(default(double))
                       ? $"{sniperInfo.IV}% IV"
                       : "unknown IV")
                   + (sniperInfo.Move1 != PokemonMove.MoveUnset && sniperInfo.Move1 != null ? $" Move1: {sniperInfo.Move1.ToString()}" : "")
                   + (sniperInfo.Move2 != PokemonMove.MoveUnset && sniperInfo.Move2 != null ? $" Move2: {sniperInfo.Move2.ToString()}" : "")
                   + (sniperInfo.EncounterId != default(ulong) && sniperInfo.EncounterId != null ? $" EncounterId: {sniperInfo.EncounterId.ToString()}" : "")
                   + (sniperInfo.ExpirationTimestamp != default(DateTime)
                       ? $" until {sniperInfo.ExpirationTimestamp.ToString(timeFormat)}"
                       : "");
        }

        public List<ChannelInfo> GetAllChannelInfos()
        {
            var channelInfos = new List<ChannelInfo>();
            if (ChannelInfo != null)
            {
                channelInfos.Add(ChannelInfo);
            }
            channelInfos.AddRange(OtherChannelInfos);
            return channelInfos;
        }

        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return SniperInfoEquals(this, (SniperInfo) obj);
        }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + Math.Round(Latitude, 4).GetHashCode();
            hash = (hash * 7) + Math.Round(Longitude, 4).GetHashCode();
            return hash;
        }

        private static bool SniperInfoEquals(SniperInfo a, SniperInfo b)
        {
            if (Math.Abs(a.Latitude - b.Latitude) <= Constants.CoordinatesOffsetAllowed
                && Math.Abs(a.Longitude - b.Longitude) <= Constants.CoordinatesOffsetAllowed)
            {
                if (a.Id.Equals(PokemonId.Missingno) || b.Id.Equals(PokemonId.Missingno))
                {
                    return true;
                }
                if (a.Id.Equals(b.Id))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
