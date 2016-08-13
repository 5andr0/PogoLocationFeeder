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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PogoLocationFeeder.Config;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using WebSocket4Net;

namespace PogoLocationFeeder.Client
{
    public class PogoClient
    {
        public event EventHandler<List<SniperInfo>> _receivedViaServer;

        public void Start()
        {

            List<PokemonId> pokemons = GlobalSettings.UseFilter ? PokemonParser.ParsePokemons(GlobalSettings.PokekomsToFeedFilter) : Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>().ToList();
            var cookieMonster = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("filter", PokemonFilterToBinary.ToBinary(pokemons))
            };
            using (var client = new WebSocket($"ws://{GlobalSettings.ServerHost}:49000", "basic", null, cookieMonster, null, null, WebSocketVersion.Rfc6455))
            {
                client.Opened += (s, e) =>
                {
                    client.Send(@"I've come to talk with you again");
                };

                long timeStamp = GetEpoch();

                client.MessageReceived += (s, e) =>
                {
                    try
                    {
                        var match = Regex.Match(e.Message, @"^(1?\d+):Hear my words that I might teach you:(.*)$");
                        if (match.Success)
                        {
                            timeStamp = Convert.ToInt64(match.Groups[1].Value);
                            var sniperInfos = JsonConvert.DeserializeObject<List<SniperInfo>>(match.Groups[2].Value);
                            OnReceivedViaServer(sniperInfos);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                };
                client.Error += (s, e) =>
                {
                    Log.Warn($"Client error rec: {e.Exception}");

                };
                client.Open();

                while (true)
                {
                    client.Send($"{timeStamp}:I've come to talk with you again");
                    Thread.Sleep(5000);
                }
                client.Close();
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

        private static long GetEpoch()
        {
            return (long)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

    }
}
