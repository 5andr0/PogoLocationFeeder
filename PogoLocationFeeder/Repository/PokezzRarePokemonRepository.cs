using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudFlareUtilities;
using Newtonsoft.Json;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using WebSocket4Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Repository
{
    public class PokezzRarePokemonRepository : IRarePokemonRepository
    {
        //private const int timeout = 20000;

        private const string URL = "ws://pokezz.com/socket.io/?EIO=3&transport=websocket";
        private const string Channel = "Pokezz";
        private readonly List<PokemonId> _pokemonIdsToFind;
        private bool _started = false;
        private ConcurrentBag<SniperInfo> _snipersInfos = new ConcurrentBag<SniperInfo>();

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
                foreach (SniperInfo sniperInfo in _snipersInfos)
                {
                    newSniperInfos.Add(sniperInfo);
                }
                _snipersInfos = new ConcurrentBag<SniperInfo>();
            }
            return newSniperInfos;
        }

        private async Task StartClient()
        {
            try
            {
                using (var ws = new ClientWebSocket())
                {
                    var uri = new Uri(URL);

                    await ws.ConnectAsync(uri, CancellationToken.None);

                    while (true)
                    {
                        var buffer = new byte[1024*1024*2];
                        var segment = new ArraySegment<byte>(buffer);
                        var result = await ws.ReceiveAsync(segment, CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Error", CancellationToken.None);
                            return;
                        }

                        int count = result.Count;
                        while (!result.EndOfMessage)
                        {
                            segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                            result = await ws.ReceiveAsync(segment, CancellationToken.None);
                            count += result.Count;
                        }

                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
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
                            else
                            {
                                //Log.Debug($"Message did not match the regex: {message}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn("Error" , e);
                _started = false;
            }
        }

        public string GetChannel()
        {
            return Channel;
        }

        private List<SniperInfo> GetJsonList(string reader)
        {
            var results = JsonConvert.DeserializeObject<List<PokezzPokemon>>(reader);
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