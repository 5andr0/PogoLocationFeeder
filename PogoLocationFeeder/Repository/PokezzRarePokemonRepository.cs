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
    public class PokezzRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "ws://pokezz.com/socket.io/?EIO=3&transport=websocket";
        private const string Channel = "PokeZZ";
        private readonly List<PokemonId> _pokemonIdsToFind;
        private WebSocket _client;
        private ConcurrentBag<SniperInfo> _snipersInfos = new ConcurrentBag<SniperInfo>();
        private bool _started;

        public PokezzRarePokemonRepository(List<PokemonId> pokemonIdsToFind)
        {
            _pokemonIdsToFind = pokemonIdsToFind;
        }

        public List<SniperInfo> FindAll()
        {
            if (!_started)
            {
                Task.Run(() => StartClient());
                _started = true;
                Thread.Sleep(10*1000);
            }
            var newSniperInfos = new List<SniperInfo>();
            lock (_snipersInfos)
            {
                foreach (var sniperInfo in _snipersInfos)
                {
                    newSniperInfos.Add(sniperInfo);
                }
                _snipersInfos = new ConcurrentBag<SniperInfo>();
            }
            return newSniperInfos;
        }

        public string GetChannel()
        {
            return Channel;
        }

        private async Task StartClient()
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
                Log.Warn("Received error from Pokezz. More info the logs");
                Log.Debug("Received error from Pokezz: ", e);

                _started = false;
            }
        }

        private void Client_Closed(object sender, EventArgs e)
        {
            _started = false;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var message = e.Message;
            //Log.Debug("Pokezz message: " + message);
            var match = Regex.Match(message, @"^(1?\d+)\[""pokemons"",(2?.*)]$");
            if (match.Success)
            {
                if (match.Groups[1].Value == "42")
                {
                    var sniperInfos = GetJsonList(match.Groups[2].Value);
                    if (sniperInfos != null && sniperInfos.Any())
                    {
                        lock (_snipersInfos)
                        {
                            sniperInfos.ForEach(i => _snipersInfos.Add(i));
                        }
                    }
                }
            }
        }

        private List<SniperInfo> GetJsonList(string reader)
        {
            var results = JsonConvert.DeserializeObject<List<PokezzPokemon>>(reader,
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

        private SniperInfo Map(PokezzPokemon result)
        {
            var sniperInfo = new SniperInfo();
            var pokemonId = PokemonParser.ParseById(result.id);
            if (!_pokemonIdsToFind.Contains(pokemonId))
            {
                return null;
            }
            sniperInfo.Id = pokemonId;
            sniperInfo.Latitude = result.lat;
            sniperInfo.Longitude = result.lng;
            if (result.time != default(long))
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var untilTime = epoch.AddMilliseconds(result.time).ToLocalTime();
                if (untilTime < DateTime.Now)
                {
                    return null;
                }
                sniperInfo.ExpirationTimestamp = untilTime;
            }
            return sniperInfo;
        }
    }

    internal class PokezzPokemon
    {
        [JsonProperty("id")]
        public long id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("lat")]
        public double lat { get; set; }

        [JsonProperty("lng")]
        public double lng { get; set; }

        [JsonProperty("time")]
        public long time { get; set; }
    }
}