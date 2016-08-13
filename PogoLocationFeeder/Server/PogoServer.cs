/*
PogoLocationFeeder gathers pokemon data from various sources and serves it to connected clients
Copyright (C) 2016  PogoLocationFeeder Development Team <admin@pokefeeder.live>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using SuperSocket.SocketBase;
using SuperWebSocket;

namespace PogoLocationFeeder.Server
{
    public class PogoServer
    {
        private readonly MemoryCache _memoryCache;
        private List<PokemonId> pokemonsToFilter;
        public event EventHandler<SniperInfo> _receivedViaClients;

        public PogoServer()
        {
            _memoryCache = new MemoryCache("server", new NameValueCollection());
            pokemonsToFilter = PokemonParser.ParsePokemons(GlobalSettings.PokekomsToFeedFilter);

        }
        public async void Start()
        {
            var server = new WebSocketServer();
            server.Setup(GlobalSettings.ServerHost, 49000);
            server.Start();
            server.NewMessageReceived += new SessionHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            server.NewSessionConnected += socketServer_NewSessionConnected;
        }

        private void socketServer_NewSessionConnected(WebSocketSession session)
        {
            object filterBinary = "";
            if (!session.Items.TryGetValue("filter", out filterBinary))
            {
                throw new Exception("Needs more pandas");
            }
            List<SniperInfo> sniperInfoToSend = new List<SniperInfo>();
            foreach (var value in _memoryCache)
            {
                var sniperInfo = (SniperInfo)value.Value;
                if (pokemonsToFilter.Contains(sniperInfo.Id))
                {
                    sniperInfoToSend.Add(sniperInfo);
                }
            }
            session.Send($"{GetEpoch()}:Hello Darkness my old friend.:" + JsonConvert.SerializeObject(sniperInfoToSend));

        }

        private void socketServer_NewMessageReceived(WebSocketSession session, string value)
        {
            var pokemonIds = GetFilters(session);
            var match = Regex.Match(value, @"^(1?\d+):Disturb the sound of silence:(2?.*)$");
            var matchRequest = Regex.Match(value, @"^(1?\d+):I've come to talk with you again$");

            if (match.Success)
            {
                SniperInfo sniperInfo = JsonConvert.DeserializeObject<SniperInfo>(match.Groups[2].Value);
                OnReceivedViaClients(sniperInfo);
            } else if (matchRequest.Success)
            {
                List<SniperInfo> sniperInfoToSend = new List<SniperInfo>();
                var epoch = GetEpoch();
                foreach (var obj in _memoryCache)
                {
                    var sniperInfo = (SniperInfo) obj.Value;
                    if (pokemonIds.Contains(sniperInfo.Id) && ToEpoch(sniperInfo.ExpirationTimestamp) > epoch)
                    {
                        sniperInfoToSend.Add(sniperInfo);
                    }
                }
                session.Send($"{GetEpoch()}:Hear my words that I might teach you:" + JsonConvert.SerializeObject(sniperInfoToSend));
            }
            else
            {
                session.Send("People talking without speaking");
            }
        }

        private static long ToEpoch(DateTime datetime)
        {
            return (long)datetime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
        private static List<PokemonId> GetFilters(WebSocketSession session)
        {
            object filterBinary = "";
            if (!session.Items.TryGetValue("filter", out filterBinary))
            {
                throw new Exception("Needs more pandas");
            }
            return PokemonFilterParser.ParseBinary(filterBinary.ToString());
        }
        public void QueueAll(List<SniperInfo> sortedMessages)
        {
            foreach (SniperInfo sniperInfo in sortedMessages)
            {
                var unqiueString = GetCoordinatesString(sniperInfo);
                if (!_memoryCache.Contains(unqiueString))
                {
                    _memoryCache.Add(unqiueString, sniperInfo, new DateTimeOffset(DateTime.Now.AddMinutes(10)));
                }
            }
        }

        private static long GetEpoch()
        {
            return (long) DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        private static string GetCoordinatesString(SniperInfo sniperInfo)
        {
            return sniperInfo.Latitude + ", " + sniperInfo.Longitude;
        }

        protected virtual void OnReceivedViaClients(SniperInfo sniperInfo)
        {
            EventHandler<SniperInfo> handler = _receivedViaClients;
            if (handler != null)
            {
                handler(this, sniperInfo);
            }
        }

    }
}