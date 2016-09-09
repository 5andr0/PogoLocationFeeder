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
using System.Linq;
using System.Threading.Tasks;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Server
{
    public class SniperInfoRepository
    {
        private const int MinutesToKeepInCache = 20;
        private const int CleanUpInterval = 60*1000;

        public SniperInfoRepository()
        {
            StartCleanupThread();
        }

        private void StartCleanupThread()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    RemoveExpired();
                    await Task.Delay(CleanUpInterval);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private readonly ConcurrentDictionary<SniperInfo, int> _sniperInfoSet =
            new ConcurrentDictionary<SniperInfo, int>();

        public SniperInfo Find(SniperInfo newSniperInfo)
        {
            return _sniperInfoSet.Keys.
                FirstOrDefault(x => x.GetHashCode() == newSniperInfo.GetHashCode()
                && x.Equals(newSniperInfo));
        }

        public List<SniperInfo> FindAllNew(long lastReceived, bool findNewVerified = false)
        {
            return _sniperInfoSet.Keys.
                Where(x => !IsExpired(x) && ToEpoch(x.ReceivedTimeStamp) > lastReceived 
                    || (findNewVerified && ToEpoch(x.VerifiedOn) > lastReceived)).ToList();
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
            count++;
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
            foreach (SniperInfo sniperInfo in _sniperInfoSet.Keys)
            {
                if (ShouldBeRemoved(sniperInfo))
                {
                    Log.Trace("Expired: " + sniperInfo);
                    Remove(sniperInfo);
                }
            }
        }

        private static bool IsExpired(SniperInfo sniperInfo)
        {
            var xMinuteAgo = DateTime.Now.AddMinutes(-1 * MinutesToKeepInCache);
            return (sniperInfo.ExpirationTimestamp == default(DateTime) &&
                     sniperInfo.ReceivedTimeStamp < xMinuteAgo) ||
                    (sniperInfo.ExpirationTimestamp != default(DateTime)
                    && sniperInfo.ExpirationTimestamp < DateTime.Now);
        }

        private static bool ShouldBeRemoved(SniperInfo sniperInfo)
        {
            var xMinuteAgo = DateTime.Now.AddMinutes(-1 * MinutesToKeepInCache);
            return sniperInfo.ReceivedTimeStamp < xMinuteAgo;
        }
    }
}