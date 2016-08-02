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
using System.Text.RegularExpressions;
using System.Text;

namespace PogoLocationFeeder.Repository
{

    public class TrackemonRarePokemonRepository : RarePokemonRepository
    {
        const int timeout = 20000;
        const String channel = "Trackemon";
        List<PokemonId> pokemonIdsToFind;

        public TrackemonRarePokemonRepository(List<PokemonId> pokemonIdsToFind)
        {
            this.pokemonIdsToFind = pokemonIdsToFind;
        }
        
        public List<SniperInfo> FindAll()
        {
            TrackemonSession session = FindSessionId();
            if (session == null || !session.validate())
            {
                session = FindSessionId();
                if (session == null)
                {
                    Log.Warn("Trackemon: No valid session found!");
                    return null;
                }
            }
            List<SniperInfo> list = new List<SniperInfo>();

            IEnumerable<List<PokemonId>> pokemonTypeIdPartitions = Partitioner.Partition(pokemonIdsToFind, 5);
            foreach(List<PokemonId> partition in pokemonTypeIdPartitions)
            {
                List<SniperInfo> resultList = findSubSetOfPokemon(partition, session);
                if(resultList != null)
                {
                    list.AddRange(resultList);
                }
            }

            return list;
        }

        private List<SniperInfo> findSubSetOfPokemon(List<PokemonId> pokemomnIds, TrackemonSession session)
        {
            String pokemonTypeIds = buildPokemonTypeIds(pokemomnIds);
            List<SniperInfo> list = new List<SniperInfo>();

            string URL = $"https://www.trackemon.com/fetch/rare?pokedexTypeId={pokemonTypeIds}&sessionId={session.sessionId}";
            try
            {
                var request = WebRequest.CreateHttp(URL);
                request.Accept = "*/*";
                request.Method = "GET";
                request.Timeout = timeout;
                request.Headers.Add("Cookie:" + session.cookieHeader);
                using (var response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        List<TrackemonResult> resultList = JsonConvert.DeserializeObject<List<TrackemonResult>>(reader.ReadToEnd());
                        foreach (TrackemonResult result in resultList)
                        {
                            SniperInfo sniperInfo = map(result);
                            if (sniperInfo != null)
                            {
                                list.Add(sniperInfo);
                            }
                        }
                    }
            
                    return list;
                }
            }
            catch (Exception e)
            {
                Log.Warn("Trackemon API error: {0}", e.Message);
                return null;
            }
        }

        private SniperInfo map(TrackemonResult result)
        {
            SniperInfo sniperInfo = new SniperInfo();
            PokemonId pokemonId = PokemonParser.parseById(result.id);
            sniperInfo.Id = pokemonId;

            sniperInfo.Latitude = result.latitude;
            sniperInfo.Longitude = result.longitude;


            sniperInfo.ExpirationTimestamp = DateTime.Now.AddTicks(result.expiration);
            return sniperInfo;
        }

        private PokemonId mapPokemon(String pokemonName)
        {
            return (PokemonId)Enum.Parse(typeof(PokemonId), pokemonName);
        }

        public TrackemonSession FindSessionId()
        {
            TrackemonSession TrackemonSession = new TrackemonSession();
            try
            {
                var cookieContainer = new CookieContainer();
                string homepageUrl = "https://www.trackemon.com";
                var request = WebRequest.CreateHttp(homepageUrl);
                request.Method = "GET";
                request.Timeout = timeout;
                request.CookieContainer = cookieContainer;
                using (var response = request.GetResponse())
                {
                    String cookieHeader = cookieContainer.GetCookieHeader(new Uri("https://www.trackemon.com"));
                    TrackemonSession.cookieHeader = cookieHeader;
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        String line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            Match match = Regex.Match(line, @"var\s+sessionId\s*=\s*\'(1?.*)\'\s*;");
                            if (match.Success)
                            {
                                TrackemonSession.sessionId = match.Groups[1].Value;
                                return TrackemonSession;
                            }
                        }
                    }
                }
            } catch(Exception e)
            {
                Log.Warn("Error trying to get a sessionId for Trackemon: {0}", e.Message);
            }
            return null;
        }

        
        private String buildPokemonTypeIds(List<PokemonId> pokemonIds)
        {
            return String.Join(",", pokemonIds.ConvertAll<long>(p => (long) p));
        }

        public string GetChannel()
        {
            return channel;
        }
    }


    class TrackemonResult
    {
        [JsonProperty("pokedexTypeId")]
        public long id { get; set; }
        [JsonProperty("longitude")]
        public double longitude { get; set; }
        [JsonProperty("latitude")]
        public double latitude { get; set; }
        [JsonProperty("expirationTime")]
        public long expiration { get; set; }
    }

    public class TrackemonSession
    {
        public String cookieHeader { get; set; }
        public String sessionId { get; set; }

        public bool validate()
        {
            if(cookieHeader == null)
            {
                return false;
            }
            if (sessionId == null)
            {
                return false;
            }
            return true;
        }
    }

}

