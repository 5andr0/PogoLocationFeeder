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
using System.Net.Http;
using CloudFlareUtilities;
using Newtonsoft.Json;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Repository
{
    public class PokeSnipeRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "http://pokeapi.pokesnipe.de/";
        public const string Channel = "pokesnipe.de";

        public PokeSnipeRarePokemonRepository()
        {
        }

        public List<SniperInfo> FindAll()
        {
            try
            {
                using (var client = new HttpClient())
                {

                    var content = client.GetStringAsync(URL).Result;
                    return GetJsonList(content);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Pokesnipe API error: {0}", e.Message);
                return null;
            }
        }

        public string GetChannel()
        {
            return Channel;
        }

        private List<SniperInfo> GetJsonList(string reader)
        {
            var results = JsonConvert.DeserializeObject<List<PokeSnipeResult>>(reader, new JsonSerializerSettingsCultureInvariant());
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

        private SniperInfo Map(PokeSnipeResult result)
        {
            var sniperInfo = new SniperInfo();
            var pokemonId = PokemonParser.ParsePokemon(result.name);
            sniperInfo.Id = pokemonId;

            sniperInfo.Latitude = result.lat;
            sniperInfo.Longitude = result.lon;
            sniperInfo.ChannelInfo = new ChannelInfo { server = Channel };
            return sniperInfo;
        }
    }


    internal class PokeSnipeResult
    {

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("lat")]
        public double lat { get; set; }

        [JsonProperty("lon")]
        public double lon { get; set; }
    }

}
