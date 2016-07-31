using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Helper
{
    public class MessageCache
    {
        const String messagePrefix = "MessageCache_";
        const int minutesToAddInCache = 15;

        public List<SniperInfo> findUnSentMessages(List<SniperInfo> sniperInfos)
        {
            List < SniperInfo > unsentSniperInfo =  new List<SniperInfo>();
            foreach(SniperInfo sniperInfo in sniperInfos)
            {
                if(!isSentAlready(sniperInfo))
                {
                    unsentSniperInfo.Add(sniperInfo);
                }
            }
            return unsentSniperInfo;
        }
        
        private bool isSentAlready(SniperInfo sniperInfo)
        {
            String coordinates = getCoordinatesString(sniperInfo);
            if(MemoryCache.Default.Contains(coordinates))
            {
                return true;
            }
            DateTime expirationDate = sniperInfo.timeStamp != default(DateTime) ? sniperInfo.timeStamp : DateTime.Now.AddMinutes(15);
            MemoryCache.Default.Add(coordinates, sniperInfo, new DateTimeOffset(expirationDate));
            return false;
        }

        private static String getCoordinatesString(SniperInfo sniperInfo)
        {
            return messagePrefix + sniperInfo.latitude + ", " + sniperInfo.longitude;
        }
    }
}
