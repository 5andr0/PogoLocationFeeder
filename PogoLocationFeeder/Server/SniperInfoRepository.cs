using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Server
{
    public class SniperInfoRepository
    {
        private const int MinutesToKeepInCache = 10;
        private const double CoordinatesOffsetAllowed = 0.000003;
        public SniperInfoRepository()
        {
        }


        private readonly ConcurrentDictionary<SniperInfo, int> _sniperInfoSet =
            new ConcurrentDictionary<SniperInfo, int>();

        public SniperInfo Find(SniperInfo newSniperInfo)
        {
            SniperInfo foundSniperInfo = null;
            foreach (SniperInfo sniperInfo in _sniperInfoSet.Keys)
            {
                if (IsExpired(sniperInfo))
                {
                    Remove(sniperInfo);
                }
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
                if(IsExpired(sniperInfo))
                {
                    Remove(sniperInfo);
                } else if (ToEpoch(sniperInfo.ReceivedTimeStamp) > lastReceived)
                {
                    sniperInfos.Add(sniperInfo);
                }
            }
            return sniperInfos;
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

        public int Set(SniperInfo sniperInfo, int count)
        {
            Remove(sniperInfo);
            _sniperInfoSet.TryAdd(sniperInfo, count);
            return count;
        }

        public int Remove(SniperInfo toRemove)
        {
            int b = 0;
            foreach (SniperInfo sniperInfo in _sniperInfoSet.Keys)
            {
                if (IsExpired(sniperInfo))
                {
                    int f = 0;
                    _sniperInfoSet.TryRemove(sniperInfo, out f);
                }
                if (SniperInfoEquals(sniperInfo, toRemove))
                {
                    _sniperInfoSet.TryRemove(sniperInfo, out b);
                }
            }
            return b;
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
            var xMinuteAgo = DateTime.Now.AddMinutes(MinutesToKeepInCache);
            return (sniperInfo.ExpirationTimestamp == default(DateTime) &&
                    sniperInfo.ReceivedTimeStamp < xMinuteAgo) ||
                   (sniperInfo.ExpirationTimestamp != default(DateTime)
                    && sniperInfo.ExpirationTimestamp < DateTime.Now);
        }
    }
}