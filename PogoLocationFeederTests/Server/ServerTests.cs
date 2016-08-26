using Microsoft.VisualStudio.TestTools.UnitTesting;
using PogoLocationFeeder.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PogoLocationFeeder.Helper;
using POGOProtos.Enums;
using WebSocket4Net;

namespace PogoLocationFeeder.Server.Tests
{
    [TestClass()]
    public class ServerTests
    {
        [TestMethod()]
        public void StartTest()
        {
            //var server = new Server();
            //Task.Run(() => server.Start());
            List<PokemonId> pokemons = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>().ToList();
            var cookieMonster = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("filter", PokemonFilterToBinary.ToBinary(pokemons))
            };
            using (var client = new WebSocket("ws://localhost:49000", "basic", null, cookieMonster, null, null, WebSocketVersion.Rfc6455))
            {
                client.Opened += (s, e) =>
                {
                    client.Send(@"I've come to talk with you again");
                };

                long timeStamp = default(long);

                client.MessageReceived += (s, e) =>
                {
                    Console.WriteLine($"Client rec: {e.Message}");

                    try
                    {
                        var match = Regex.Match(e.Message, @"^(1?\d+):.*$");
                        if (match.Success)
                        {
                            timeStamp = Convert.ToInt64(match.Groups[1].Value);
                            Console.WriteLine($"Client rec: {e.Message}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                };
                client.Error += (s, e) =>
                {
                    Console.WriteLine($"Client error rec: {e.Exception}");

                };
                client.Open();

                for (int i = 0; i<30; i++)
                {
                    client.Send($"{timeStamp}:I've come to talk with you again");
                    Thread.Sleep(1000);
                }
                client.Close();
            }
            Thread.Sleep(2000);

        }
    }
}