using Newtonsoft.Json;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PogoLocationFeeder
{

    public class PokeSniperReader
    {

        private static Dictionary<int, Result> _cache;

        private const string URL = "http://pokesnipers.com/api/v1/pokemon.json";

        public PokeSniperReader()
        {
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
                    list.Add(sniperInfo);
                }
                return list;
            } else
            {
                System.Console.WriteLine("Pokesnipers API down ({0})", response.StatusCode);
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
            Match match = Regex.Match(result.coords, @"(?<lat>-?\d+\.?\d*)(?:\,|\s)+(?<long>-?\d+\.?\d*)");
            if (match.Success)
            {
                sniperInfo.latitude = Convert.ToDouble(match.Groups["lat"].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
                sniperInfo.longitude = Convert.ToDouble(match.Groups["long"].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
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

    }

    class Wrapper
    {
        [JsonProperty("results")]
        public List<Result> results { get; set; }
    }
}

