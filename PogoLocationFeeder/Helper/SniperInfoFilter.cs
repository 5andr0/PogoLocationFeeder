using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Client;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Server;
using POGOProtos.Enums;

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
                s => Matches(s, pokemonIds, verifiedOnly, channels, areaBounds)).ToList();
        }

        private static bool Matches(SniperInfo sniperInfo, List<PokemonId> pokemonIds,  bool verifiedOnly, List<Channel> channels, LatLngBounds areaBounds ) 
        {
            if (!pokemonIds.Contains(sniperInfo.Id))
            {
                Log.Trace($"Skipped {sniperInfo} because not in pokemon list");
                return false;
            }
            if (verifiedOnly && !sniperInfo.Verified)
            {
                Log.Trace($"Skipped {sniperInfo} because verifiedOnly is on, but this isn't verified");
                return false;
            }
            if (channels != null && !MatchesChannel(channels, sniperInfo.ChannelInfo))
            {
                Log.Trace($"Skipped {sniperInfo} because the channel doesn't match the channel list");
                return false;
            }
            if (areaBounds != null && !areaBounds.Intersects(sniperInfo.Latitude, sniperInfo.Longitude))
            {
                Log.Trace($"Skipped {sniperInfo} because the lat & long isn't the areabounds {areaBounds}");
                return false;
            }
            return true;
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
