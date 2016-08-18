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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Server
{
    public class SniperInfoRepository
    {
        private const int MinutesToKeepInCache = 20;
        private const double CoordinatesOffsetAllowed = 0.000003;


        public SniperInfoRepository()
        {
            StartCleanupThread();
        }

        private void StartCleanupThread()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    RemoveExpired();
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private readonly ConcurrentDictionary<SniperInfo, int> _sniperInfoSet =
            new ConcurrentDictionary<SniperInfo, int>();

        public SniperInfo Find(SniperInfo newSniperInfo)
        {
            SniperInfo foundSniperInfo = null;
            foreach (SniperInfo sniperInfo in _sniperInfoSet.Keys)
            {
                if (SniperInfoEquals(sniperInfo,newSniperInfo))
                {
                    foundSniperInfo = sniperInfo;
                }
            }
            return foundSniperInfo;
        }

        public List<SniperInfo> FindAllNew(long lastReceived)
        {
            List<SniperInfo> sniperInfos = new List<SniperInfo>();
            foreach (SniperInfo sniperInfo in _sniperInfoSet.Keys)
            {
                if (ToEpoch(sniperInfo.ReceivedTimeStamp) > lastReceived)
                {
                    sniperInfos.Add(sniperInfo);
                }
            }
            return sniperInfos;
        }

        public int Count()
        {
            return _sniperInfoSet.Count;
        }

        private static long ToEpoch(DateTime datetime)
        {
            return
                (long)
                    datetime.ToUniversalTime()
                        .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                        .TotalMilliseconds;
        }

        public int Increase(SniperInfo sniperInfo)
        {
            int count = Remove(sniperInfo);
            if (sniperInfo.ChannelInfo?.server == Constants.Bot)
            {
                count++;
            }
            _sniperInfoSet.TryAdd(sniperInfo, count);
            return count;
        }

        public int Update(SniperInfo sniperInfo)
        {
            int count= Remove(sniperInfo);
            _sniperInfoSet.TryAdd(sniperInfo, count);
            return count;
        }

        public int Remove(SniperInfo toRemove)
        {
            int b = 0;
            _sniperInfoSet.TryRemove(toRemove, out b);
            return b;
        }

        public void RemoveExpired()
        {
            SniperInfo foundSniperInfo = null;
            foreach (SniperInfo sniperInfo in _sniperInfoSet.Keys)
            {
                if (IsExpired(sniperInfo))
                {
                    Log.Trace("Expired: " + sniperInfo);
                    Remove(sniperInfo);
                }
            }
        }

        private static bool SniperInfoEquals(SniperInfo a, SniperInfo b)
        {

            if (Math.Abs(a.Latitude - b.Latitude) <= CoordinatesOffsetAllowed
                && Math.Abs(a.Longitude - b.Longitude) <= CoordinatesOffsetAllowed)
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
        private static bool IsExpired(SniperInfo sniperInfo)
        {
            var xMinuteAgo = DateTime.Now.AddMinutes(-1 * MinutesToKeepInCache);
            return (sniperInfo.ExpirationTimestamp == default(DateTime) &&
                    sniperInfo.ReceivedTimeStamp < xMinuteAgo) ||
                   (sniperInfo.ExpirationTimestamp != default(DateTime)
                    && sniperInfo.ExpirationTimestamp < DateTime.Now);
        }
    }
}