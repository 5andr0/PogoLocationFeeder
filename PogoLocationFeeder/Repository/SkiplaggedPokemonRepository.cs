using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using CloudFlareUtilities;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Repository
{
    public class SkiplaggedPokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "http://pokesnipers.com/api/v1/pokemon.json";
        private const string Channel = "Skiplagged";
        private readonly List<PokemonId> _pokemonIdsToFind;


        public SkiplaggedPokemonRepository(List<PokemonId> pokemonIdsToFind)
        {
            _pokemonIdsToFind = pokemonIdsToFind;
        }

        public List<SniperInfo> FindAll()
        {
            List<SniperInfo> results = new List<SniperInfo>();
            try
            {
                //string bound = "-37.837022,144.925045,-37.788607,145.02109";
                if (!File.Exists("config/skiplagged_bounds.json"))
                {
                    return null;
                }
                var allBounds = JsonConvert.DeserializeObject<List<BoundInfo>>(File.ReadAllText("config/skiplagged_bounds.json"));
                Parallel.ForEach(allBounds, (bound) =>
                {    if (!bound.enabled) return;
                    var subset = FetchSingleBound(bound);

                    lock (results)
                    {
                        results.AddRange(subset);
                    }
                });
            }
            catch (Exception e)
            {

            }
            return results;
        }

        private List<SkiplaggedSniperInfo> FetchSingleBound(BoundInfo bound)
        {
            List<SkiplaggedSniperInfo> results = new List<SkiplaggedSniperInfo>();

            string url = $"https://skiplagged.com/api/pokemon.php?bounds={bound.bound}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, sdch, br");
            client.DefaultRequestHeaders.Host = "skiplagged.com";
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36");

            client.GetStringAsync(url).ContinueWith((s) =>
            {
                var response = s.Result;
                results = GetJsonList(response);
                if(results != null)
                {
                    results.ForEach((info) =>  { info.RegionName = bound.name; });
                }
            }).Wait();
            return results;
        }

        public string GetChannel()
        {
            return Channel;
        }

        private List<SkiplaggedSniperInfo> GetJsonList(string reader)
        {
            var wrapper = JsonConvert.DeserializeObject<DataModel>(reader, new JsonSerializerSettingsCultureInvariant());
            var list = new List<SkiplaggedSniperInfo>();
            foreach (var result in wrapper.pokemons)
            {
                var sniperInfo = Map(result);
                if (sniperInfo != null)
                {
                    list.Add(sniperInfo);
                }
            }
            return list;
        }

        private SkiplaggedSniperInfo Map(pokemon result)
        {
            var sniperInfo = new SkiplaggedSniperInfo();
            var pokemonId = PokemonParser.ParseById(result.pokemon_id);
            if (!_pokemonIdsToFind.Contains(pokemonId))
            {
                return null;
            }
            sniperInfo.Id = pokemonId;

            sniperInfo.Latitude = result.latitude;
            sniperInfo.Longitude = result.longitude;

            sniperInfo.ExpirationTimestamp = result.expires_date;
            return sniperInfo;
        }
    }
    public class SkiplaggedSniperInfo :SniperInfo{

        public string RegionName { get; set; }
    }
    public class BoundInfo
    {
        public string name { get; set; }
        public string bound { get; set; }
        public bool enabled { get; set; }
    }


    public class DataModel
    {
        public double duration { get; set; }
        public List<pokemon> pokemons { get; set; }
        public DataModel()
        {
            pokemons = new List<pokemon>();
        }
    }
    public class pokemon
    {
        public DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public DateTime expires_date
        {
            get
            {
                return UnixTimeStampToDateTime(expires);
            }
        }

        public double expires { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int pokemon_id { get; set; }
        public string pokemon_name { get; set; }
    }

}