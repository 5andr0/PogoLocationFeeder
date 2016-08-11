using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using CloudFlareUtilities;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;

namespace PogoLocationFeeder.Repository
{
    public class PokewatchersRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "https://pokewatchers.com/api.php?act=grab";
        private const string Channel = "Pokewatchers";

        public PokewatchersRarePokemonRepository()
        {
        }

        public List<SniperInfo> FindAll()
        {
            try
            {

                // Create a HttpClient that uses the handler.
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Referrer = new Uri("https://pokewatchers.com/");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36");
                        ;

                    // Use the HttpClient as usual. Any JS challenge will be solved automatically for you.
                    var content = client.GetStringAsync(URL).Result;
                    return GetJsonList(content);
                }
            }
            catch (Exception e)
            {
                Log.Debug("Pokewatchers API error: {0}", e.Message);
                return null;
            }
        }

        public string GetChannel()
        {
            return Channel;
        }

        private List<SniperInfo> GetJsonList(string reader)
        {
            var results = JsonConvert.DeserializeObject<List<PokewatchersResult>>(reader, new JsonSerializerSettingsCultureInvariant());
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

        private SniperInfo Map(PokewatchersResult result)
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

            var untilTime = DateTime.Now.AddTicks(result.until);
            sniperInfo.ExpirationTimestamp = untilTime;
            return sniperInfo;
        }
    }


    internal class PokewatchersResult
    {

        [JsonProperty("pokemon")]
        public string name { get; set; }

        [JsonProperty("cords")]
        public string coords { get; set; }

        [JsonProperty("timeend")]
        public long until { get; set; }

        [JsonProperty("icon")]
        public string icon { get; set; }
    }

}