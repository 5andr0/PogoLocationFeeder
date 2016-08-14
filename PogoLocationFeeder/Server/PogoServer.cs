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
using System.Globalization;
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
using PogoLocationFeeder.Client;
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
        private WebSocketServer webSocketServer;

        public PogoServer()
        {
            _memoryCache = new MemoryCache("server", new NameValueCollection());
            pokemonsToFilter = PokemonParser.ParsePokemons(GlobalSettings.PokekomsToFeedFilter);

        }
        public void Start()
        {
            var webSocketServer = new WebSocketServer();
            webSocketServer.Setup(49000);
            webSocketServer.Start();
            webSocketServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            webSocketServer.NewSessionConnected += socketServer_NewSessionConnected;
            webSocketServer.SessionClosed += socketServer_SessionClosed;
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
            Log.Info($"[{webSocketServer.SessionCount}] Session started");

        }

        private void socketServer_SessionClosed(WebSocketSession session, CloseReason closeReason)
        {
           Log.Info($"[{webSocketServer.SessionCount}] Session closed: " + closeReason);
        }

        private void socketServer_NewMessageReceived(WebSocketSession session, string value)
        {
            var filter = GetFilter(session);
            var pokemonIds = PokemonFilterParser.ParseBinary(filter.pokemon);
            var channels = filter.channels;
            var match = Regex.Match(value, @"^(1?\d+)\:(?:Disturb the sound of silence)\:(2?.*)$");
            var matchRequest = Regex.Match(value, @"^(1?\d+)\:(?:I\'ve come to talk with you again)$");

            if (match.Success)
            {
                SniperInfo sniperInfo = JsonConvert.DeserializeObject<SniperInfo>(match.Groups[2].Value);
                OnReceivedViaClients(sniperInfo);
            } else if (matchRequest.Success)
            {
                List<SniperInfo> sniperInfoToSend = new List<SniperInfo>();
                var epoch = GetEpoch();
                var lastReceived = Convert.ToInt64(matchRequest.Groups[1].Value);
                foreach (var obj in _memoryCache)
                {
                    var sniperInfo = (SniperInfo) obj.Value;
                    if (pokemonIds.Contains(sniperInfo.Id) 
                        && ( ToEpoch(sniperInfo.ExpirationTimestamp) > epoch || sniperInfo.ExpirationTimestamp == default(DateTime))
                        && ToEpoch(sniperInfo.ReceivedTimeStamp) > lastReceived
                        && MatchesChannel(channels, sniperInfo.ChannelInfo))
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

        private bool MatchesChannel(List<Channel> channels, ChannelInfo channelInfo)
        {
            foreach (Channel channel in channels)
            {
                if((channel == null && channelInfo == null) || 
                    Object.Equals(channel.server,channelInfo.server)
                    && Object.Equals(channel.channel,channelInfo.channel))
                {
                    return true;
                }
            }
            return false;
        }
        private static long ToEpoch(DateTime datetime)
        {
            return (long)datetime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        private static Filter GetFilter(WebSocketSession session)
        {
            object filterString = "";
            if (!session.Items.TryGetValue("filter", out filterString))
            {
                throw new Exception("Needs more pandas");
            }
            return JsonConvert.DeserializeObject<Filter>(filterString.ToString());
        }

        public void QueueAll(List<SniperInfo> sortedMessages)
        {
            foreach (SniperInfo sniperInfo in sortedMessages)
            {
                var unqiueString = GetCoordinatesString(sniperInfo);
                if (!_memoryCache.Contains(unqiueString))
                {
                    _memoryCache.Add(unqiueString, sniperInfo, new DateTimeOffset(DateTime.Now.AddMinutes(10)));
                    const string timeFormat = "HH:mm:ss";
                    Log.Pokemon($"{sniperInfo.ChannelInfo}: {sniperInfo.Id} at {sniperInfo.Latitude.ToString(CultureInfo.InvariantCulture)},{sniperInfo.Longitude.ToString(CultureInfo.InvariantCulture)}"
                            + " with " + (!sniperInfo.IV.Equals(default(double)) ? $"{sniperInfo.IV}% IV" : "unknown IV")
                            +
                            (sniperInfo.ExpirationTimestamp != default(DateTime)
                                ? $" until {sniperInfo.ExpirationTimestamp.ToString(timeFormat)}"
                                : ""));
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