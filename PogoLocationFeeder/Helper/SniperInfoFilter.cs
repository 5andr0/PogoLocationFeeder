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
            var useUploadedPokemon = filter.UseUploadedPokemon;
            var unverifiedOnly = filter.UnverifiedOnly;
            var pokemonNotInFilterMinimumIV = filter.PokemonNotInFilterMinimumIV;
            return sniperInfos.Where(
                s => Matches(s, pokemonIds, verifiedOnly, channels, areaBounds, minimumIV, useUploadedPokemon, unverifiedOnly, pokemonNotInFilterMinimumIV)).ToList();
        }

        private static bool Matches(SniperInfo sniperInfo, List<PokemonId> pokemonIds,  
            bool verifiedOnly, List<Channel> channels, LatLngBounds areaBounds, double minimumIV, bool useUploadedPokemon, 
            bool unverifiedOnly, double pokemonNotInFilterMinimumIV) 
        {

            if (!useUploadedPokemon && (Constants.Bot == sniperInfo.ChannelInfo.server
                || Constants.PogoFeeder == sniperInfo.ChannelInfo.server))
            {
                Log.Trace($"Skipped {sniperInfo} because useUploadedPokemon is false.");
                return false;
            }
            if (verifiedOnly && !sniperInfo.Verified)
            {
                Log.Trace($"Skipped {sniperInfo} because verifiedOnly is on, but this isn't verified");
                return false;
            }
            if (unverifiedOnly && sniperInfo.Verified)
            {
                Log.Trace($"Skipped {sniperInfo} because unverifiedOnly was true.");
                return false;
            }
            if (minimumIV > sniperInfo.IV)
            {
                Log.Trace($"Skipped {sniperInfo} because the IV was lower than {minimumIV}");
                return false;
            }
            if (!pokemonIds.Contains(sniperInfo.Id))
            {
                if(pokemonNotInFilterMinimumIV > sniperInfo.IV) { 
                    Log.Trace($"Skipped {sniperInfo} because not in pokemon list and pokemonNotInFilterMinimumIV is higher than its IV");
                    return false;
                }
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
