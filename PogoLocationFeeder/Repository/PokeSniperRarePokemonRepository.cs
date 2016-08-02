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
                        Wrapper wrapper = JsonConvert.DeserializeObject<Wrapper>(reader.ReadToEnd());
                        List<SniperInfo> list = new List<SniperInfo>();
                        foreach (Result result in wrapper.results)
                        {
                            SniperInfo sniperInfo = map(result);
                            if (sniperInfo != null)
                            {
                                list.Add(sniperInfo);
                            }
                        }
                        return list;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn("Pokesnipers API error: {0}", e.Message);
                return null;
            }
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

        private PokemonId mapPokemon(String pokemonName)
        {
            return (PokemonId)Enum.Parse(typeof(PokemonId), pokemonName);
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

