using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using CloudFlareUtilities;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Repository
{
    public class PokeSniperRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "http://pokesnipers.com/api/v1/pokemon.json";
        private const string Channel = "Pokesnipers";

        public PokeSniperRarePokemonRepository()
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
            sniperInfo.Latitude = geoCoordinates.Latitude;
            sniperInfo.Longitude = geoCoordinates.Longitude;

            sniperInfo.ExpirationTimestamp = Convert.ToDateTime(result.until);
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