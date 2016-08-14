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
using System.Linq;
using System.Runtime.Caching;

namespace PogoLocationFeeder.Helper
{
    public class MessageCache
    {
        private const string MessagePrefix = "MessageCache_";
        //private const int minutesToAddInCache = 15;
        public static readonly MessageCache Instance = new MessageCache();
    
        private MessageCache() {}
        public List<SniperInfo> FindUnSentMessages(List<SniperInfo> sniperInfos)
        {
            return sniperInfos.Where(sniperInfo => !IsSentAlready(sniperInfo)).ToList();
        }

        private static bool IsSentAlready(SniperInfo sniperInfo)
        {
            var coordinates = GetCoordinatesString(sniperInfo);
            if (MemoryCache.Default.Contains(coordinates))
            {
                Log.Trace($"Skipping duplicate {sniperInfo}");
                return true;
            }
            var expirationDate = sniperInfo.ExpirationTimestamp != default(DateTime)
                ? sniperInfo.ExpirationTimestamp
                : DateTime.Now.AddMinutes(15);
            MemoryCache.Default.Add(coordinates, sniperInfo, new DateTimeOffset(expirationDate));
            return false;
        }

        private static string GetCoordinatesString(SniperInfo sniperInfo)
        {
            return MessagePrefix + sniperInfo.Latitude.ToString("N5", CultureInfo.InvariantCulture) + ", " + sniperInfo.Longitude.ToString("N5", CultureInfo.InvariantCulture);
        }
    }
}
