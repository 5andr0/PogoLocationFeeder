using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Client;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Server;

namespace PogoLocationFeeder.Helper
{
    public class SniperInfoFilter
    {
        internal static List<SniperInfo> FilterUnmanaged(List<SniperInfo> sniperInfos, Filter filter)
        {
            var verifiedOnly = filter.VerifiedOnly;
            var pokemonIds = PokemonFilterParser.ParseBinary(filter.Pokemon);
            var channels = filter.Channels;
            var areaBounds = filter.AreaBounds;

            return sniperInfos.Where(
                s => pokemonIds.Contains(s.Id) 
                     && ((verifiedOnly && s.Verified) || !verifiedOnly)
                     && (GlobalSettings.IsManaged || MatchesChannel(channels, s.ChannelInfo))
                     && (areaBounds == null || areaBounds.Intersects(s.Latitude, s.Longitude))).ToList();
        }

        private static bool MatchesChannel(List<Channel> channels, ChannelInfo channelInfo)
        {
            foreach (Channel channel in channels)
            {
                if ((channel == null && channelInfo == null) ||
                    Object.Equals(channel.Server, channelInfo.server)
                    && Object.Equals(channel.ChannelName, channelInfo.channel))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
