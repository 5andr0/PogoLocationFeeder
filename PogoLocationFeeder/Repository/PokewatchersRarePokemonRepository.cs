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
    public class PokewatchersRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "https://pokewatchers.com/api.php?act=grab";
        private const string Channel = "Pokewatchers";
        private readonly List<PokemonId> _pokemonIdsToFind;

        public PokewatchersRarePokemonRepository(List<PokemonId> pokemonIdsToFind)
        {
            _pokemonIdsToFind = pokemonIdsToFind;
        }

        public List<SniperInfo> FindAll()
        {
            try
            {
                var request = WebRequest.CreateHttp(URL);
                request.Accept = "application/json";
                request.Method = "GET";
                request.Timeout = 20000;

                using (var response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        return GetJsonList(reader.ReadToEnd());
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    var client = new HttpClient();

                    // Use the HttpClient as usual. Any JS challenge will be solved automatically for you.
                    var content = client.GetStringAsync(URL).Result;
                    return GetJsonList(content);
                }
                catch (Exception)
                {
                    Log.Debug("Pokewatchers API error: {0}", e.Message);
                    return null;
                }
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
            if (!_pokemonIdsToFind.Contains(pokemonId))
            {
                return null;
            }
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