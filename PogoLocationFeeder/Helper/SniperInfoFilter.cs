using System;
using System.Collections.Generic;
using System.Linq;
using PogoLocationFeeder.Client;
using PogoLocationFeeder.Common;
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
            var minimumIV = filter.MinimumIV;
            return sniperInfos.Where(
                s => Matches(s, pokemonIds, verifiedOnly, channels, areaBounds, minimumIV)).ToList();
        }

        private static bool Matches(SniperInfo sniperInfo, List<PokemonId> pokemonIds,  bool verifiedOnly, List<Channel> channels, LatLngBounds areaBounds, double minimumIV ) 
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
            if (channels != null && !MatchesChannel(channels, sniperInfo.GetAllChannelInfos()))
            {
                Log.Trace($"Skipped {sniperInfo} because the channel doesn't match the channel list");
                return false;
            }
            if (areaBounds != null && !areaBounds.Intersects(sniperInfo.Latitude, sniperInfo.Longitude))
            {
                Log.Trace($"Skipped {sniperInfo} because the lat & long isn't the areabounds {areaBounds}");
                return false;
            }
            if (minimumIV > sniperInfo.IV)
            {
                Log.Trace($"Skipped {sniperInfo} because the IV was lower than {minimumIV}");
                return false;
            }
            return true;
        }

        private static bool MatchesChannel(List<Channel> channels, List<ChannelInfo> channelInfos )
        {
            foreach (Channel channel in channels)
            {
                if (channelInfos.Any(channelInfo =>
                    (channel == null && channelInfo == null) ||
                           Object.Equals(channel.Server, channelInfo.server)
                           && Object.Equals(channel.ChannelName, channelInfo.channel)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
