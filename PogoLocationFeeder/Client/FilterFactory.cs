﻿/*
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
using System.Reflection;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Repository;
using POGOProtos.Enums;
using static PogoLocationFeeder.Helper.ChannelParser;
using PogoLocationFeeder.Common;

namespace PogoLocationFeeder.Client
{
    public class FilterFactory
    {
        public static Filter Create(List<DiscordChannels> discordChannels )
        {
            List<PokemonId> pokemons = GlobalSettings.UseFilter
                ? PokemonParser.ParsePokemons(new List<string>(GlobalSettings.PokekomsToFeedFilter))
                : Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>().ToList();
            var pokemonsBinary = PokemonFilterToBinary.ToBinary(pokemons);
            List<Channel> channelInfos = new List<Channel>();
            if (discordChannels != null && discordChannels.Any())
            {
                foreach (DiscordChannels discordChannel in discordChannels)
                {
                    channelInfos.Add(new Channel() {server = discordChannel.Server, channel = discordChannel.Name});
                }
            }
            if (GlobalSettings.UsePokeSnipers)
            {
                channelInfos.Add(new Channel() {server = PokeSniperRarePokemonRepository.Channel });
            }
            if (GlobalSettings.UsePokemonGoIVClub)
            {
                channelInfos.Add(new Channel() { server = PokemonGoIVClubRarePokemonRepository.Channel });
            }
            if (GlobalSettings.UsePokewatchers)
            {
                channelInfos.Add(new Channel() { server = PokewatchersRarePokemonRepository.Channel });
            }
            if (GlobalSettings.UseTrackemon)
            {
                channelInfos.Add(new Channel() { server = TrackemonRarePokemonRepository.Channel });
            }
            if (GlobalSettings.UsePokezz)
            {
                channelInfos.Add(new Channel() { server = PokezzRarePokemonRepository.Channel });
            }
            channelInfos.Add(new Channel() {server = Constants.PogoFeeder });
            channelInfos.Add(new Channel() { server = Constants.Bot });

            var filter = new Filter();
            filter.channels = channelInfos;
            filter.pokemon = pokemonsBinary;
            filter.verifiedOnly = GlobalSettings.VerifiedOnly;
            filter.version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return filter;
        }
    }
}
