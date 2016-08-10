using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using WebSocket4Net;

namespace PogoLocationFeeder.Repository
{
    public class PokemonGoIVClubRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "ws://pokemongoivclub.com:49002/socket.io/?EIO=3&transport=websocket";
        private const string Channel = "Pokemon Go IV Club";
        private WebSocket _client;
        private ConcurrentQueue<SniperInfo> _snipersInfos = new ConcurrentQueue<SniperInfo>();
        private bool _started;

        public PokemonGoIVClubRarePokemonRepository()
        {
        }

        public List<SniperInfo> FindAll()
        {
            if (!_started)
            {
                Task.Run(() => StartClient());
                _started = true;
                Thread.Sleep(1000);
            }
            var newSniperInfos = new List<SniperInfo>();
            lock (_snipersInfos)
            {
                SniperInfo sniperInfo = null;
                while (_snipersInfos.TryDequeue(out sniperInfo))
                {
                    newSniperInfos.Add(sniperInfo);
                }
            }
            return newSniperInfos;
        }


        public string GetChannel()
        {
            return Channel;
        }

        public async Task StartClient()
        {
            try
            {
                _client = new WebSocket(URL, "basic", WebSocketVersion.Rfc6455);
                _client.Closed += Client_Closed;
                _client.MessageReceived += Client_MessageReceived;
                _client.Open();
            }
            catch (Exception e)
            {
                Log.Warn("Received error from Pokemon Go IV Club. More info the logs");
                Log.Debug("Received error from Pokemon Go IV Club: ", e);
                CloseClient();
            }
        }

        private void Client_Closed(object sender, EventArgs e)
        {
            CloseClient();
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var message = e.Message;
                var match = Regex.Match(message, @"(1?\d+)+\[""helo"",(2?.*)\]");
                if (match.Success)
                {
                    if (match.Groups[1].Value == "42")
                    {
                        var sniperInfos = GetJsonList(match.Groups[2].Value);
                        if (sniperInfos != null && sniperInfos.Any())
                        {
                            lock (_snipersInfos)
                            {
                                sniperInfos.ForEach(i => _snipersInfos.Enqueue(i));
                            }
                        }
                    }
                }
                else
                {
                    match = Regex.Match(message, @"(1?\d+)+\[""poke"",(2?.*)\]");
                    if (match.Success)
                    {
                        if (match.Groups[1].Value == "42")
                        {
                            var sniperInfo = GetJson(match.Groups[2].Value);
                            if (sniperInfo != null)
                            {
                                lock (_snipersInfos)
                                {
                                    _snipersInfos.Enqueue(sniperInfo);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Error receiving message from PokemonGoIVClub", ex);
            }
        }

        private List<SniperInfo> GetJsonList(string reader)
        {
            var results = JsonConvert.DeserializeObject<List<PokemongoivclubPokemon>>(reader,
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

        private SniperInfo GetJson(string reader)
        {
            var result = JsonConvert.DeserializeObject<PokemongoivclubPokemon>(reader,
                new JsonSerializerSettingsCultureInvariant());
            return Map(result);
        }

        private SniperInfo Map(PokemongoivclubPokemon result)
        {
            var sniperInfo = new SniperInfo();
            var pokemonId = PokemonParser.ParsePokemon(result.name);
            sniperInfo.Id = pokemonId;
            sniperInfo.Latitude = result.lat;
            sniperInfo.Longitude = result.lon;
            return sniperInfo;
        }

        private void CloseClient()
        {
            lock (_client)
            {
                if (_started)
                {
                    _started = false;
                    try
                    {
                        try
                        {
                            _client?.Close();
                        }
                        catch (Exception e)
                        {
                            // ignore
                        }
                        _client?.Dispose();
                        _client = null;
                    }
                    catch (Exception e)
                    {
                        // ignore
                    }
                }
            }
        }
    }

    internal class PokemongoivclubPokemon
    {
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("lat")]
        public double lat { get; set; }

        [JsonProperty("lon")]
        public double lon { get; set; }
    }
}