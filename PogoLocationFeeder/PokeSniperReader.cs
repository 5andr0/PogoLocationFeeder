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

namespace PogoLocationFeeder
{

    public class PokeSniperReader
    {
        const String pokesnipers_prefix = "pokesnipersid_";

        private const string URL = "http://pokesnipers.com/api/v1/pokemon.json";

        public PokeSniperReader()
        {
        }

        public List<SniperInfo> readAll()
        {
            try
            {
                var request = WebRequest.CreateHttp(URL);
                request.Accept = "application/json";
                request.Method = "GET";
                request.Timeout = 10000;

                var response = request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                Wrapper wrapper = JsonConvert.DeserializeObject<Wrapper>(reader.ReadToEnd());
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
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Pokesnipers API down: {0}", e.Message);
                return null;
            }
        }

        private List<Result> storeInCache(List<Result> list)
        {
            var newResultList = new List<Result>();
            foreach (Result result in list)
            {
                if (!MemoryCache.Default.Contains(pokesnipers_prefix + result.id.ToString()))
                {
                    var expiration = DateTimeOffset.Parse(result.until);
                    MemoryCache.Default.Add(pokesnipers_prefix + result.id.ToString(), result, expiration);
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

