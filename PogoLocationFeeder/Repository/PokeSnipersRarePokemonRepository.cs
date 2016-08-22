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
    public class PokeSnipersRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "http://www.pokesnipers.com/api/v1/pokemon.json?referrer=home";

        public PokeSnipersRarePokemonRepository()
        {
        }

        public List<SniperInfo> FindAll()
        {
            try
            {
                var handler = new ClearanceHandler();

                // Create a HttpClient that uses the handler.
                using (var client = new HttpClient(handler))
                {

                    // Use the HttpClient as usual. Any JS challenge will be solved automatically for you.
                    var content = client.GetStringAsync(URL).Result;
                    return GetJsonList(content);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Pokesnipers API error: {0}", e.Message);
                return null;
            }
        }

        public string GetChannel()
        {
            return Channel;
        }

        private List<SniperInfo> GetJsonList(string reader)
        {
            var wrapper = JsonConvert.DeserializeObject<Wrapper>(reader, new JsonSerializerSettingsCultureInvariant());
            var list = new List<SniperInfo>();
            foreach (var result in wrapper.results)
            {
                var sniperInfo = Map(result);
                if (sniperInfo != null)
                {
                    list.Add(sniperInfo);
                }
            }
            return list;
        }



        private SniperInfo Map(Result result)
        {
            var sniperInfo = new SniperInfo();
            var pokemonId = PokemonParser.ParsePokemon(result.name);
            sniperInfo.Id = pokemonId;
            var geoCoordinates = GeoCoordinatesParser.ParseGeoCoordinates(result.coords);
            if (geoCoordinates == null)
            {
                return null;
            }
            sniperInfo.Latitude = Math.Round(geoCoordinates.Latitude, 7);
            sniperInfo.Longitude = Math.Round(geoCoordinates.Longitude, 7);

            var timeStamp = Convert.ToDateTime(result.until);
            sniperInfo.ExpirationTimestamp = DateTime.Now.AddMinutes(Constants.MaxExpirationInTheFuture) < timeStamp ?
                DateTime.Now.AddMinutes(Constants.MaxExpirationInTheFuture) : timeStamp;

            sniperInfo.ChannelInfo = new ChannelInfo { server = Channel };
            return sniperInfo;
        }
    }


    internal class Result
    {
        [JsonProperty("id")]
        public long id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("coords")]
        public string coords { get; set; }

        [JsonProperty("until")]
        public string until { get; set; }

        [JsonProperty("icon")]
        public string icon { get; set; }
    }

    internal class Wrapper
    {
        [JsonProperty("results")]
        public List<Result> results { get; set; }
    }
}
