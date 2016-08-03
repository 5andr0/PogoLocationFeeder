using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Caching;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CloudFlareUtilities;

namespace PogoLocationFeeder.Repository
{

    public class PokeSniperRarePokemonRepository : RarePokemonRepository
    {
        const int timeout = 20000;

        private const string URL = "http://pokesnipers.com/api/v1/pokemon.json";
        const String channel = "Pokesnipers";
        List<PokemonId> pokemonIdsToFind;

        public PokeSniperRarePokemonRepository(List<PokemonId> pokemonIdsToFind)
        {
            this.pokemonIdsToFind = pokemonIdsToFind;
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
                        
                        return getJsonList(reader.ReadToEnd());
                    }
                }
            }
            catch (Exception e)
            {
                try {
                    var handler = new ClearanceHandler();

                    // Create a HttpClient that uses the handler.
                    var client = new HttpClient(handler);

                    // Use the HttpClient as usual. Any JS challenge will be solved automatically for you.
                    var content = client.GetStringAsync(URL).Result;
                    return getJsonList(content);
                } catch (Exception) {
                    Log.Debug("Pokesnipers API error: {0}", e.Message);
                    return null;
                }
            }
        }
        private List<SniperInfo> getJsonList(string reader) {
            Wrapper wrapper = JsonConvert.DeserializeObject<Wrapper>(reader);
            List<SniperInfo> list = new List<SniperInfo>();
            foreach(Result result in wrapper.results) {
                SniperInfo sniperInfo = map(result);
                if(sniperInfo != null) {
                    list.Add(sniperInfo);
                }
            }
            return list;
        }

        private SniperInfo map(Result result)
        {
            SniperInfo sniperInfo = new SniperInfo();
            PokemonId pokemonId = PokemonParser.parsePokemon(result.name);
            if (!pokemonIdsToFind.Contains(pokemonId))
            {
                return null;
            }
            sniperInfo.Id = pokemonId;
            GeoCoordinates geoCoordinates = GeoCoordinatesParser.parseGeoCoordinates(result.coords);
            if (geoCoordinates == null)
            {
                return null;
            }
            else
            {
                sniperInfo.Latitude = geoCoordinates.latitude;
                sniperInfo.Longitude = geoCoordinates.longitude;
            }

            sniperInfo.ExpirationTimestamp = Convert.ToDateTime(result.until);
            return sniperInfo;
        }

        public string GetChannel()
        {
            return channel;
        }
    }


    class Result
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

    class Wrapper
    {
        [JsonProperty("results")]
        public List<Result> results { get; set; }
    }
}

