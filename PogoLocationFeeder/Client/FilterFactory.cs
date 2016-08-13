using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Repository;
using POGOProtos.Enums;
using static PogoLocationFeeder.Helper.ChannelParser;

namespace PogoLocationFeeder.Client
{
    public class FilterFactory
    {
        public static Filter Create(List<DiscordChannels> discordChannels )
        {
            List<PokemonId> pokemons = GlobalSettings.UseFilter
                ? PokemonParser.ParsePokemons(GlobalSettings.PokekomsToFeedFilter)
                : Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>().ToList();
            var pokemonsBinary = PokemonFilterToBinary.ToBinary(pokemons);
            List<Channel> channelInfos = new List<Channel>();
            if (discordChannels != null && !discordChannels.Any())
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
            var filter = new Filter();
            filter.channels = channelInfos;
            filter.pokemon = pokemonsBinary;
            return filter;
        }
    }
}
