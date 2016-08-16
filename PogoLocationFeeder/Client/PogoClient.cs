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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Caching;
using Newtonsoft.Json;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using WebSocket4Net;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Server;

namespace PogoLocationFeeder.Client
{
    public class PogoClient
    {
        public event EventHandler<List<SniperInfo>> _receivedViaServer;
        public static ConcurrentQueue<SniperInfo> sniperInfosToSend = new ConcurrentQueue<SniperInfo>();

        public void Start(List<ChannelParser.DiscordChannels> discordChannels )
        {
            ServerUploadFilter serverUploadFilter = null;
            while (true)
            {
                var running = true;
                while (running)
                {
                    using (
                        var client = new WebSocket($"ws://{GlobalSettings.ServerHost}:{GlobalSettings.ServerPort}", "basic", WebSocketVersion.Rfc6455))
                    {
                        long timeStamp = GetEpoch2MinAgo();

                        client.Opened += (s, e) =>
                        {
                            var filter = JsonConvert.SerializeObject(FilterFactory.Create(discordChannels));

                            client.Send($"{timeStamp}:I've come to talk with you again:{filter}");
                            GlobalSettings.Output.SetStatus($"Connected to: {GlobalSettings.ServerHost}");
                        };

                        client.Closed += (s, e) =>
                        {
                            Log.Warn("Connection to server lost");
                            GlobalSettings.Output.SetStatus($"Connection lost: {GlobalSettings.ServerHost}");
                            running = false;
                        };
                        client.MessageReceived += (s, e) =>
                        {
                            try
                            {
                                var match = Regex.Match(e.Message,
                                    @"^(1?\d+)\:(?:Hear my words that I might teach you)\:(2?.*)$");
                                var matchDarkness = Regex.Match(e.Message,
                                    @"^(1?\d+)\:(?:Hello Darkness my old friend\.)\:(2?.*)$");
                            
                                if (match.Success)
                                {
                                    timeStamp = Convert.ToInt64(match.Groups[1].Value);
                                    var sniperInfos =
                                        JsonConvert.DeserializeObject<List<SniperInfo>>(match.Groups[2].Value);
                                    Log.Info($"Received {sniperInfos.Count} pokemon from server");
                                    OnReceivedViaServer(sniperInfos);
                                } else if (matchDarkness.Success)
                                {
                                    timeStamp = Convert.ToInt64(matchDarkness.Groups[1].Value);
                                    serverUploadFilter =
                                        JsonConvert.DeserializeObject<ServerUploadFilter>(matchDarkness.Groups[2].Value);


                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Error", ex);
                            }
                        };
                        client.Error += (s, e) =>
                        {
                            //Log.Warn($"Client error rec: {e.Exception}");

                        };
                        client.Open();

                        while (running)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                Thread.Sleep(1000);
                                SniperInfo sniperInfo = null;
                                while (sniperInfosToSend.TryDequeue(out sniperInfo))
                                {
                                    var pokemonIds = serverUploadFilter != null ? PokemonFilterParser.ParseBinary(serverUploadFilter.pokemon) : null;

                                    if (pokemonIds == null || pokemonIds.Contains(sniperInfo.Id))
                                    {
                                        Log.Info($"Uploading bot pokemon: {sniperInfo}");
                                        client.Send($"{GetEpochNow()}:Disturb the sound of silence:" +
                                                    JsonConvert.SerializeObject(sniperInfo));
                                    }
                                    else
                                    {
                                        Log.Info($"Not uploading bot capture because {sniperInfo.Id} is not the server upload filter.");
                                    }
                                }
                            }
                            var filter = JsonConvert.SerializeObject(FilterFactory.Create(discordChannels));
                            client.Send($"{timeStamp}:I've come to talk with you again:{filter}");
                        }
                    }
                    Log.Info("Reconnecting to stream in 10 seconds");
                    Thread.Sleep(10000);
                }
            }
        }

        protected virtual void OnReceivedViaServer(List<SniperInfo> sniperInfos)
        {
            EventHandler<List<SniperInfo>> handler = _receivedViaServer;
            if (handler != null)
            {
                handler(this, sniperInfos);
            }
        }


        private static long GetEpoch2MinAgo()
        {
            return (long)DateTime.Now.AddMinutes(-2).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        private static long GetEpochNow()
        {
            return (long)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

    }
}
