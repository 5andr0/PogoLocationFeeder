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
using System.Linq;
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
        public event EventHandler<SniperInfo> ReceivedViaClients;
        private WebSocketServer _webSocketServer;
        private readonly SniperInfoRepository _serverRepository;
        private readonly SniperInfoRepositoryManager _sniperInfoRepositoryManager;
        private List<PokemonId> _pokemonIds;

        public PogoServer()
        {
            _serverRepository = new SniperInfoRepository();
            _sniperInfoRepositoryManager = new SniperInfoRepositoryManager(_serverRepository);
        }
        public void Start()
        {
            _webSocketServer = new WebSocketServer();
            SuperSocket.SocketBase.Config.RootConfig rootConfig = new SuperSocket.SocketBase.Config.RootConfig();
            var serverConfig = new SuperSocket.SocketBase.Config.ServerConfig();
            serverConfig.Name = "PokeFeeder";
            serverConfig.ServerTypeName = "WebSocketService";
            serverConfig.Ip = "Any";
            serverConfig.Port = GlobalSettings.OutgoingServerPort;
            serverConfig.MaxRequestLength = 4096;
            var socketServerFactory = new SuperSocket.SocketEngine.SocketServerFactory();
            _webSocketServer.Setup(rootConfig, serverConfig, socketServerFactory);
            _webSocketServer.Start();
            _webSocketServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            _webSocketServer.NewSessionConnected += socketServer_NewSessionConnected;
            _webSocketServer.SessionClosed += socketServer_SessionClosed;
            _pokemonIds = GlobalSettings.UseFilter
                ? PokemonParser.ParsePokemons(new List<string>(GlobalSettings.PokekomsToFeedFilter))
                : Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>().ToList();
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var sessionCount  = _webSocketServer?.SessionCount ?? 0;
            Console.Title = $"PogoLocationFeeder Server: Sessions {sessionCount}";
        }

        private void socketServer_NewSessionConnected(WebSocketSession session)
        {
            var uploadFilter = JsonConvert.SerializeObject(ServerUploadFilterFactory.Create(_pokemonIds));
            session.Send($"{GetEpoch()}:Hello Darkness my old friend.:{uploadFilter}");
            Log.Info($"[{_webSocketServer.SessionCount}:{session.SessionID}] Session started");
            UpdateTitle();
        }

        private void socketServer_SessionClosed(WebSocketSession session, CloseReason closeReason)
        {
           Log.Info($"[{_webSocketServer.SessionCount}:{session.SessionID}] Session closed: " + closeReason);
           UpdateTitle();
        }

        private void socketServer_NewMessageReceived(WebSocketSession session, string value)
        {
            try
            {
                var match = Regex.Match(value, @"^(1?\d+)\:(?:Disturb the sound of silence)\:(2?.*)$");
                var matchRequest = Regex.Match(value, @"^(1?\d+)\:(?:I\'ve come to talk with you again\:)(2?.*)$");

                if (match.Success)
                {
                    var sniperInfos= JsonConvert.DeserializeObject<List<SniperInfo>>(match.Groups[2].Value);
                    if (sniperInfos != null && sniperInfos.Any())
                    {
                        foreach (SniperInfo sniperInfo in sniperInfos)
                        {
                            if (_pokemonIds == null || _pokemonIds.Contains(sniperInfo.Id))
                            {
                                OnReceivedViaClients(sniperInfo);
                            }
                        }
                    }
                }
                else if (matchRequest.Success)
                {
                    Filter filter = JsonConvert.DeserializeObject<Filter>(matchRequest.Groups[2].Value);

                    var lastReceived = Convert.ToInt64(matchRequest.Groups[1].Value);
                    var sniperInfos = _serverRepository.FindAllNew(lastReceived, filter.VerifiedOnly);

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
                Log.Error($"[Session {session.SessionID}]Error during message received: ", e);
                session.Send("People talking without speaking");
            }
        }



        public void QueueAll(List<SniperInfo> sortedMessages)
        {
            foreach (SniperInfo sniperInfo in sortedMessages)
            {
                _sniperInfoRepositoryManager.AddToRepository(sniperInfo);
            }
        }

        private static long GetEpoch()
        {
            return (long) DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }


        protected virtual void OnReceivedViaClients(SniperInfo sniperInfo)
        {
            if (GlobalSettings.IsManaged && GlobalSettings.ShareBotCaptures)
            {
                if(Constants.Bot == sniperInfo.ChannelInfo?.server ||
                        Constants.PogoFeeder == sniperInfo.ChannelInfo?.server)
                {
                    PogoClient.sniperInfosToSend.Enqueue(sniperInfo);
                }
            }
            ReceivedViaClients?.Invoke(this, sniperInfo);
        }

    }
}