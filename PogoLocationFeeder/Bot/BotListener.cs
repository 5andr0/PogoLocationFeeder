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
using PogoLocationFeeder.Client;
using PogoLocationFeeder.Common;
using PogoLocationFeeder.Helper;
using PogoLocationFeeder.Input;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using WebSocket4Net;

namespace PogoLocationFeeder.Bot
{
    public class BotListener
    {

        public void Start(int port)
        {
            while (true)
            {
                var running = true;

                using (var client = new WebSocket($"wss://localhost:{port}", "basic", WebSocketVersion.Rfc6455))
                {
                    System.Net.ServicePointManager.ServerCertificateValidationCallback =
                        (sender, certificate, chain, errors) => true;

                    client.AllowUnstrustedCertificate = false;
                    client.Opened += (s, e) =>
                    {
                        Log.Debug($"Connected to bot on {port}");
                    };

                    client.Closed += (s, e) =>
                    {
                        Log.Debug($"Disconnect from bot on {port}");
                        running = false;
                    };
                    client.MessageReceived += (s, e) =>
                    {
                        if (e.Message.Contains("PokemonCaptureEvent"))
                        {
                            PokemonCaptureEvent pokemonCaptureEvent = null;
                            try
                            {
                                pokemonCaptureEvent = JsonConvert.DeserializeObject<NewPokemonCaptureEvent>(e.Message,
                                    new JsonSerializerSettingsCultureInvariant());
                            }
                            catch (Exception exception)
                            {
                                pokemonCaptureEvent = JsonConvert.DeserializeObject<PokemonCaptureEvent>(e.Message, new JsonSerializerSettingsCultureInvariant());
                            }
                            if (pokemonCaptureEvent != null)
                            {
                                InputService.Instance.BotCapture(Map(pokemonCaptureEvent));
                            }

                        }
                    };
                    client.Error += (s, e) =>
                    {

                    };
                    client.Open();
                    while (running)
                    {
                        Thread.Sleep(10000);
                    }
                    Log.Debug("Waiting 30 seconds to try to reconnect to the bot");
                    Thread.Sleep(30*1000);
                }
            }
        }

        private static SniperInfo Map(PokemonCaptureEvent pokemonCaptureEvent)
        {
            var sniperInfo = new SniperInfo();
            sniperInfo.ChannelInfo = new ChannelInfo() {server = Constants.Bot};
            sniperInfo.IV = pokemonCaptureEvent.Perfection;
            sniperInfo.Id = pokemonCaptureEvent.Id;
            sniperInfo.Latitude = Math.Round(pokemonCaptureEvent.Latitude,7);
            sniperInfo.Longitude = Math.Round(pokemonCaptureEvent.Longitude,7);
            sniperInfo.Verified = true;
            sniperInfo.VerifiedOn = DateTime.Now;
            if (pokemonCaptureEvent is NewPokemonCaptureEvent)
            {
                var newPokemonCaptureEvent = (NewPokemonCaptureEvent) pokemonCaptureEvent;
                sniperInfo.ExpirationTimestamp = FromUnixTime(newPokemonCaptureEvent.Expires);
                sniperInfo.SpawnPointId = newPokemonCaptureEvent.SpawnPointId;
                sniperInfo.Move1 = newPokemonCaptureEvent.Move1;
                sniperInfo.Move2 = newPokemonCaptureEvent.Move2;
                sniperInfo.EncounterId = newPokemonCaptureEvent.EncounterId;
                if (newPokemonCaptureEvent.CatchTypeText != "normal")
                {
                    Log.Trace("Skipping pokemon because it was not a wild pokemon");
                }
            }
            return sniperInfo;
        }
        private static DateTime FromUnixTime(double unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime).ToLocalTime();
        }

        public class PokemonCaptureEvent
        {
            public int Attempt;
            public int BallAmount;
            public string CatchType;
            public int Cp;
            public double Distance;
            public int Exp;
            public int FamilyCandies;
            public PokemonId Id;
            public double Level;
            public int MaxCp;
            public double Perfection;
            public ItemId Pokeball;
            public double Probability;
            public int Stardust;
            public CatchPokemonResponse.Types.CatchStatus Status;
            public double Latitude;
            public double Longitude;
        }

        public class NewPokemonCaptureEvent : PokemonCaptureEvent
        {
            public string SpawnPointId;
            public ulong EncounterId;
            public PokemonMove Move1;
            public PokemonMove Move2;
            public long Expires;
            public string CatchTypeText;
        }
    }


}
