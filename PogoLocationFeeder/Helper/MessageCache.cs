using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace PogoLocationFeeder.Helper
{
    public class MessageCache
    {
        private const string MessagePrefix = "MessageCache_";
        //private const int minutesToAddInCache = 15;

        public List<SniperInfo> FindUnSentMessages(List<SniperInfo> sniperInfos)
        {
            return sniperInfos.Where(sniperInfo => !IsSentAlready(sniperInfo)).ToList();
        }

        private static bool IsSentAlready(SniperInfo sniperInfo)
        {
            var coordinates = GetCoordinatesString(sniperInfo);
            if (MemoryCache.Default.Contains(coordinates))
            {
                Log.Debug($"Skipping duplicate {sniperInfo}");
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
            return MessagePrefix + sniperInfo.Latitude + ", " + sniperInfo.Longitude;
        }
    }
}