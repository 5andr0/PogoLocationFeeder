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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using WebSocket4Net;

namespace PogoLocationFeeder.Repository
{
    public class PokemonGoIVClubRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "ws://pokemongoivclub.com:49002/socket.io/?EIO=3&transport=websocket";
        private const string Channel = "Pokemon Go IV Club";
        private const int Timeout = 5000;

        public PokemonGoIVClubRarePokemonRepository()
        {
        }

        public List<SniperInfo> FindAll()
        {
            List<SniperInfo> newSniperInfos = new List<SniperInfo>();
            try
            {
                using (var client = new WebSocket(URL, "basic", WebSocketVersion.Rfc6455))
                {
                    client.MessageReceived += (s, e) =>
                    {
                        try
                        {
                            var message = e.Message;
                            var match = Regex.Match(message, @"(1?\d+)+\[""helo"",(2?.*)\]");
                            if (match.Success)
                            {
                                if (match.Groups[1].Value == "42")
                                {
                                    var sniperInfos = GetJsonList(match.Groups[2].Value);
                                    if (sniperInfos != null && sniperInfos.Any())
                                    {
                                        newSniperInfos.AddRange(sniperInfos);
                                    }
                                }
                            }
                            else
                            {
                                match = Regex.Match(message, @"(1?\d+)+\[""poke"",(2?.*)\]");
                                if (match.Success)
                                {
                                    if (match.Groups[1].Value == "42")
                                    {
                                        var sniperInfo = GetJson(match.Groups[2].Value);
                                        if (sniperInfo != null)
                                        {
                                            newSniperInfos.Add(sniperInfo);

                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Debug("Error receiving message from PokemonGoIVClub", ex);
                        }
                    };
                    client.Open();
                    Thread.Sleep(Timeout);
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Log.Warn("Received error from Pokezz. More info the logs");
                Log.Debug("Received error from Pokezz: ", e);

            }
            return newSniperInfos;
        }

        public string GetChannel()
        {
            return Channel;
        }

        private static List<SniperInfo> GetJsonList(string reader)
        {
            var results = JsonConvert.DeserializeObject<List<PokemongoivclubPokemon>>(reader,
                new JsonSerializerSettingsCultureInvariant());
            var list = new List<SniperInfo>();
            foreach (var result in results)
            {
                var sniperInfo = Map(result);
                if (sniperInfo != null)
                {
                    list.Add(sniperInfo);
                }
            }
            return list;
        }

        private static SniperInfo GetJson(string reader)
        {
            var result = JsonConvert.DeserializeObject<PokemongoivclubPokemon>(reader,
                new JsonSerializerSettingsCultureInvariant());
            return Map(result);
        }

        private static SniperInfo Map(PokemongoivclubPokemon result)
        {
            var sniperInfo = new SniperInfo();
            var pokemonId = PokemonParser.ParsePokemon(result.name);
            sniperInfo.Id = pokemonId;
            sniperInfo.Latitude = result.lat;
            sniperInfo.Longitude = result.lon;
            return sniperInfo;
        }
    }

    internal class PokemongoivclubPokemon
    {
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("lat")]
        public double lat { get; set; }

        [JsonProperty("lon")]
        public double lon { get; set; }
    }
}
