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
    public class RareSpawnsRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "ws://188.165.224.208:49001/socket.io/?EIO=3&transport=websocket";
        private const string Channel = "RareSpawns";
        private WebSocket _client;
        private ConcurrentQueue<SniperInfo> _snipersInfos = new ConcurrentQueue<SniperInfo>();
        private bool _started;

        public RareSpawnsRarePokemonRepository()
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
                _client = new WebSocket(URL, "", WebSocketVersion.None);
                _client.Closed += Client_Closed;
                _client.MessageReceived += Client_MessageReceived;
                _client.Error += Client_Error;

                _client.Open();
            }
            catch (Exception e)
            {
                Log.Warn("Received error from PokeSpawns. More info the logs");
                Log.Debug("Received error from PokeSpawns: ", e);
                CloseClient();
            }
        }

        private void Client_Closed(object sender, EventArgs e)
        {
            CloseClient();
        }

        private void Client_Error(object sender, EventArgs e)
        {
            CloseClient();
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var message = e.Message;
            if (message == "40")
            {
                _client.Send("40/pokes");
            }
            //Log.Debug("Pokezz message: " + message);
            var match = Regex.Match(message, @"(1?\d+)\[""helo"",(2?.*)\]");
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
            match = Regex.Match(message, @"(1?\d+)\[""poke"",(2?.*)\]");
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

        private List<SniperInfo> GetJsonList(string reader)
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

        private SniperInfo GetJson(string reader)
        {
            var result = JsonConvert.DeserializeObject<PokeSpawnsPokemon>(reader,
                new JsonSerializerSettingsCultureInvariant());
            return Map(result);
        }

        private SniperInfo Map(PokeSpawnsPokemon result)
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
            _started = false;
            try
            {
                try
                {
                    _client.Close();
                }
                catch (Exception) { }
                _client.Dispose();
                _client = null;
            }
            catch (Exception) { }
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
}