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
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PogoLocationFeeder.Client;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using SuperSocket.SocketBase;
using SuperWebSocket;

namespace PogoLocationFeeder.Server
{
    public class PogoServer
    {
        public event EventHandler<SniperInfo> _receivedViaClients;
        private WebSocketServer webSocketServer;
        public static readonly SniperInfoRepository _serverRepository = new SniperInfoRepository();
        const string timeFormat = "HH:mm:ss";
        private List<PokemonId> _pokemonIds;
        public PogoServer()
        {
        }
        public void Start()
        {
            webSocketServer = new WebSocketServer();
            webSocketServer.Setup(49000);
            webSocketServer.Start();
            webSocketServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            webSocketServer.NewSessionConnected += socketServer_NewSessionConnected;
            webSocketServer.SessionClosed += socketServer_SessionClosed;
            _pokemonIds = GlobalSettings.UseFilter
                ? PokemonParser.ParsePokemons(new List<string>(GlobalSettings.PokekomsToFeedFilter))
                : Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>().ToList();
        }

        private void socketServer_NewSessionConnected(WebSocketSession session)
        {
            var uploadFilter = JsonConvert.SerializeObject(ServerUploadFilterFactory.Create(_pokemonIds));
            session.Send($"{GetEpoch()}:Hello Darkness my old friend.:{uploadFilter}");
            Log.Info($"[{webSocketServer.SessionCount}] Session started");

        }

        private void socketServer_SessionClosed(WebSocketSession session, CloseReason closeReason)
        {
           Log.Info($"[{webSocketServer.SessionCount}] Session closed: " + closeReason);
        }

        private void socketServer_NewMessageReceived(WebSocketSession session, string value)
        {
            try
            {
                var match = Regex.Match(value, @"^(1?\d+)\:(?:Disturb the sound of silence)\:(2?.*)$");
                var matchRequest = Regex.Match(value, @"^(1?\d+)\:(?:I\'ve come to talk with you again\:)(2?.*)$");

                if (match.Success)
                {
                    SniperInfo sniperInfo = JsonConvert.DeserializeObject<SniperInfo>(match.Groups[2].Value);
                    if (_pokemonIds == null || _pokemonIds.Contains(sniperInfo.Id))
                    {
                        OnReceivedViaClients(sniperInfo);
                    }
                }
                else if (matchRequest.Success)
                {
                    Filter filter = JsonConvert.DeserializeObject<Filter>(matchRequest.Groups[2].Value);

                    var lastReceived = Convert.ToInt64(matchRequest.Groups[1].Value);
                    var sniperInfos = _serverRepository.FindAllNew(lastReceived);

                    var sniperInfoToSend = SniperInfoFilter.FilterUnmanaged(sniperInfos, filter);

                    session.Send($"{GetEpoch()}:Hear my words that I might teach you:" +
                                 JsonConvert.SerializeObject(sniperInfoToSend));
                }
                else
                {
                    session.Send("People talking without speaking");
                }
            }
            catch (Exception e)
            {
                Log.Error("Error during message received: ", e);
                session.Send("People talking without speaking");
            }
        }



        public void QueueAll(List<SniperInfo> sortedMessages)
        {
            foreach (SniperInfo sniperInfo in sortedMessages)
            {
                var oldSniperInfo = _serverRepository.Find(sniperInfo);

                if (oldSniperInfo != null && sniperInfo.Verified && !oldSniperInfo.Verified)
                {
                    if (PokemonId.Missingno.Equals(oldSniperInfo.Id))
                    {
                        oldSniperInfo.Id = sniperInfo.Id;
                    }
                    oldSniperInfo.IV = sniperInfo.IV;
                    UpdatePokemon(oldSniperInfo, false);
                }
                else
                {
                    UpdatePokemon(sniperInfo, oldSniperInfo == null);
                }
            }
        }

        private void UpdatePokemon(SniperInfo sniperInfo, bool discovered = true)
        {
            if (discovered || sniperInfo.ChannelInfo?.server == Constants.Bot)
            {
                var captures = _serverRepository.Increase(sniperInfo);
                Log.Pokemon($"{(discovered ? "Discovered" : "Captured")}: {sniperInfo.ChannelInfo}: {sniperInfo.Id} at {sniperInfo.Latitude.ToString("N6", CultureInfo.InvariantCulture)},{sniperInfo.Longitude.ToString("N6", CultureInfo.InvariantCulture)}"
                            + " with " +
                            (!sniperInfo.IV.Equals(default(double))
                                ? $"{sniperInfo.IV}% IV"
                                : "unknown IV")
                            +
                            (sniperInfo.ExpirationTimestamp != default(DateTime)
                                ? $" until {sniperInfo.ExpirationTimestamp.ToString(timeFormat)}"
                                : "") + $", Captures {captures}");
            }
        }
        private static long GetEpoch()
        {
            return (long) DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
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