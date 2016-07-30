using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PogoLocationFeeder
{

    public class PokeSniperReader
    {

        private static Dictionary<int, Result> _cache;

        private const string URL = "http://pokesnipers.com/api/v1/pokemon.json";

        public PokeSniperReader()
        {
            //TODO This is can blow up after time, we should use proper a proper cache
            //This is only used to track which pokemon we already received.
            _cache = new Dictionary<int, Result>();
        }

        public object MemoryCache { get; private set; }

        public async Task<List<SniperInfo>> readAll()
        {

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));


            HttpResponseMessage response = await client.GetAsync(URL);
            if (response.IsSuccessStatusCode)
            {
                var wrapper = response.Content.ReadAsAsync<Wrapper>().Result;
                List<Result> newResults = storeInCache(wrapper.results);

                List<SniperInfo> list = new List<SniperInfo>();
                foreach (Result result in newResults)
                {
                    SniperInfo sniperInfo = map(result);
                    if (sniperInfo != null)
                    {
                        list.Add(sniperInfo);
                    }
                }
                return list;
            } else
            {
                System.Console.WriteLine("Pokesnipers API down ({0})", response.ReasonPhrase);
            }
            return null;
        }

        private List<Result> storeInCache(List<Result> list)
        {
            var newResultList = new List<Result>();
            foreach (Result result in list)
            {
                if (!_cache.ContainsKey(result.id))
                {
                    var expiration = DateTimeOffset.Parse(result.until);
                    _cache.Add(result.id, result);
                    newResultList.Add(result);
                }
            }
            return newResultList;
        }

        private SniperInfo map(Result result)
        {
            SniperInfo sniperInfo = new SniperInfo();
            sniperInfo.id = (PokemonId)Enum.Parse(typeof(PokemonId), result.name, true);
            PokemonId pokemonId = PokemonParser.parsePokemon(result.name);
            sniperInfo.id = pokemonId;
            GeoCoordinates geoCoordinates = GeoCoordinatesParser.parseGeoCoordinates(result.coords);
            if (geoCoordinates == null)
            {
                return null;
            }
            else
            {
                sniperInfo.latitude = geoCoordinates.latitude;
                sniperInfo.longitude = geoCoordinates.longitude;
            }

            sniperInfo.timeStamp = Convert.ToDateTime(result.until);
            return sniperInfo;
        }

        private PokemonId mapPokemon(String pokemonName)
        {
            return (PokemonId)Enum.Parse(typeof(PokemonId), pokemonName);
        }
    }


    class Result
    {
        [JsonProperty("id")]
        public int id { get; set; }
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

