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

    public class TrackermonRarePokemonRepository : RarePokemonRepository
    {
        const int timeout = 20000;
        const String channel = "Trackermon";
        List<PokemonId> pokemonIdsToFind;

        public TrackermonRarePokemonRepository(List<PokemonId> pokemonIdsToFind)
        {
            this.pokemonIdsToFind = pokemonIdsToFind;
        }
        
        public List<SniperInfo> FindAll()
        {
            TrackermonSession session = FindSessionId();
            if (session == null || !session.validate())
            {
                session = FindSessionId();
                if (session == null)
                {
                    Log.warn("Trackemon: No valid session found!");
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

        private List<SniperInfo> findSubSetOfPokemon(List<PokemonId> pokemomnIds, TrackermonSession session)
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
                var response = request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                List<TrackemonResult> resultList = JsonConvert.DeserializeObject<List<TrackemonResult>>(reader.ReadToEnd());
                foreach (TrackemonResult result in resultList)
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
                Log.warn("Trackermon API error: {0}", e.Message);
                return null;
            }
        }

        private SniperInfo map(TrackemonResult result)
        {
            SniperInfo sniperInfo = new SniperInfo();
            PokemonId pokemonId = PokemonParser.parseById(result.id);
            sniperInfo.id = pokemonId;

            sniperInfo.latitude = result.latitude;
            sniperInfo.longitude = result.longitude;


            sniperInfo.timeStamp = DateTime.Now.AddTicks(result.expiration);
            return sniperInfo;
        }

        private PokemonId mapPokemon(String pokemonName)
        {
            return (PokemonId)Enum.Parse(typeof(PokemonId), pokemonName);
        }

        public TrackermonSession FindSessionId()
        {
            TrackermonSession trackermonSession = new TrackermonSession();

            try
            {
                var cookieContainer = new CookieContainer();
                string homepageUrl = "https://www.trackemon.com";
                var request = WebRequest.CreateHttp(homepageUrl);
                request.Method = "GET";
                request.Timeout = timeout;
                request.CookieContainer = cookieContainer;
                var response = request.GetResponse();
                String cookieHeader = cookieContainer.GetCookieHeader(new Uri("https://www.trackemon.com"));
                trackermonSession.cookieHeader = cookieHeader;
                var reader = new StreamReader(response.GetResponseStream());
                String line;

                while ((line = reader.ReadLine()) != null)
                {
                    Match match = Regex.Match(line, @"var\s+sessionId\s*=\s*\'(1?.*)\'\s*;");
                    if (match.Success)
                    {
                        trackermonSession.sessionId = match.Groups[1].Value;
                    }
                }
            } catch(Exception e)
            {
                Log.warn("Error trying to get a sessionId for trackermon: {0}", e.Message);
            }
            return trackermonSession;
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

    public class TrackermonSession
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

