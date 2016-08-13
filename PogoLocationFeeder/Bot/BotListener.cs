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
                        Log.Info($"Connected to bot on {port}");
                    };

                    client.Closed += (s, e) =>
                    {
                        Log.Info($"Disconnect from bot on {port}");
                        running = false;
                    };
                    client.MessageReceived += (s, e) =>
                    {
                        if (e.Message.Contains("PokemonCaptureEvent"))
                        {
                            var pokemonCaptureEvent = JsonConvert.DeserializeObject<PokemonCaptureEvent>(e.Message);
                            if (pokemonCaptureEvent.Attempt == 1)
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
                }
            }
        }

        private static SniperInfo Map(PokemonCaptureEvent pokemonCaptureEvent)
        {
            var sniperInfo = new SniperInfo();
            sniperInfo.ChannelInfo = new ChannelInfo() {server = Constants.Bot};
            sniperInfo.IV = pokemonCaptureEvent.Perfection;
            sniperInfo.Id = pokemonCaptureEvent.Id;
            sniperInfo.Latitude = pokemonCaptureEvent.Latitude;
            sniperInfo.Longitude = pokemonCaptureEvent.Longitude;
            return sniperInfo;
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
    }


}
