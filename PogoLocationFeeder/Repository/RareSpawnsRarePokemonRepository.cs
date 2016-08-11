using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using WebSocket4Net;

namespace PogoLocationFeeder.Repository
{
    public class RareSpawnsRarePokemonRepository : IRarePokemonRepository
    {
        private const string AuthUrl = "http://188.165.224.208:49002/api/v1/auth";
        private const string Channel = "RareSpawns";
        private const int Timeout = 5000;

        public RareSpawnsRarePokemonRepository()
        {
        }

        public List<SniperInfo> FindAll()
        {
            List<SniperInfo> newSniperInfos = new List<SniperInfo>();
            try
            {
                string token = "";
                using (var client = new HttpClient())
                {

                    // Use the HttpClient as usual. Any JS challenge will be solved automatically for you.
                    var content = client.GetStringAsync(AuthUrl).Result;
                    token = GetToken(content)?.token;
                }
                string URL = $"ws://188.165.224.208:49001/socket.io/?EIO=3&transport=websocket&token={token}";
                using (var client = new WebSocket(URL, "basic", null, 
                    new List<KeyValuePair<string, string>>() {new KeyValuePair<string, string>("Referer","http://www.rarespawns.be/"), new KeyValuePair<string, string>("Host", "188.165.224.208:49001") }, 
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36", "http://www.rarespawns.be", WebSocketVersion.Rfc6455, null))
                {

                        client.MessageReceived += (s, e) =>
                    {
                        try
                        {
                            var message = e.Message;
                            if (message == "40")
                            {
                                client.Send("40/pokes");
                            }
                            var match = Regex.Match(message, @"(1?\d+)+.*\[""helo"",(2?.*)\]");
                            if (match.Success)
                            {
                                if (match.Groups[1].Value == "42")
                                {
                                    var sniperInfos = GetJsonList(match.Groups[2].Value);
                                    if (sniperInfos != null && sniperInfos.Any())
                                    {
                                        newSniperInfos.AddRange(sniperInfos);
                                    }
                                }
                            }
                            else
                            {
                                match = Regex.Match(message, @"(1?\d+)+.*\[""poke"",(2?.*)\]");
                                if (match.Success)
                                {
                                    if (match.Groups[1].Value == "42")
                                    {
                                        var sniperInfo = GetJson(match.Groups[2].Value);
                                        if (sniperInfo != null)
                                        {
                                            newSniperInfos.Add(sniperInfo);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Debug("Error receiving message from RareSpawns", ex);
                        }
                    };
                    client.Open();
                    Thread.Sleep(Timeout);
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Log.Warn("Received error from Pokezz. More info the logs");
                Log.Debug("Received error from Pokezz: ", e);

            }
            return newSniperInfos;
        }

        private Token GetToken(string reader)
        {
            return JsonConvert.DeserializeObject<Token>(reader, new JsonSerializerSettingsCultureInvariant());
        }

        public string GetChannel()
        {
            return Channel;
        }

        private static List<SniperInfo> GetJsonList(string reader)
        {
            var results = JsonConvert.DeserializeObject<List<PokeSpawnsPokemon>>(reader,
                new JsonSerializerSettingsCultureInvariant());
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

        private static SniperInfo GetJson(string reader)
        {
            var result = JsonConvert.DeserializeObject<PokeSpawnsPokemon>(reader,
                new JsonSerializerSettingsCultureInvariant());
            return Map(result);
        }

        private static SniperInfo Map(PokeSpawnsPokemon result)
        {
            var sniperInfo = new SniperInfo();
            var pokemonId = PokemonParser.ParsePokemon(result.name);
            sniperInfo.Id = pokemonId;
            sniperInfo.Latitude = result.lat;
            sniperInfo.Longitude = result.lon;
            return sniperInfo;
        }
    }

    internal class PokeSpawnsPokemon
    {
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("lat")]
        public double lat { get; set; }

        [JsonProperty("lon")]
        public double lon { get; set; }
    }

    internal class Token
    {
        [JsonProperty("token")]
        public string token { get; set; }
    }
}