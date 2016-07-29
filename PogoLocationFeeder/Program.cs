using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord;
using POGOProtos.Enums;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using System.IO;
using Newtonsoft.Json;

namespace PogoLoco
{
    public class PokemonInfo
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double iv { get; set; }
        public DateTime timeStamp { get; set; }
        public PokemonId id { get; set; }
    }

    class Program
    {
        static void Main(string[] args) => new Program().Start();

        private TcpListener listener;
        ArrayList arrSocket = new ArrayList();

        bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        public void StartNet()
        {
            listener = new TcpListener(IPAddress.Any, 16969);
            listener.Start();
            Console.WriteLine("Listening...");
            StartAccept();
        }
        private void StartAccept()
        {
            listener.BeginAcceptTcpClient(HandleAsyncConnection, listener);
        }
        private void HandleAsyncConnection(IAsyncResult res)
        {
            StartAccept();
            TcpClient client = listener.EndAcceptTcpClient(res);
            if (client != null && SocketConnected(client.Client))
            {
                arrSocket.Add(client);
                Console.WriteLine("new connection");
            }
        }

        private DiscordClient _client;

        private async Task feedToClients(PokemonInfo info)
        {
            foreach (TcpClient obj in arrSocket) // Repeat for each connected client (socket held in a dynamic array)
            {
                TcpClient socket = obj;

                if (!SocketConnected(socket.Client))
                {
                    //arrSocket.Remove(obj); // this is crashing, needs a fix
                    continue;
                }

                NetworkStream networkStream = socket.GetStream();

                JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriter(networkStream));

                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(jsonWriter, info);
                jsonWriter.Flush();
            }

            // debug output
            Console.WriteLine(info.latitude);
            Console.WriteLine(info.longitude);
            Console.WriteLine(info.iv);
            Console.WriteLine(info.timeStamp);
            Console.WriteLine(info.id);

            /*
            var obj2 = _client.Servers;
            foreach (Server s in obj2)
            {
                Console.WriteLine($"{s.ChannelCount} {s.DefaultChannel} {s.Id} {s.Name}");
            }*/

        }
        private async Task parseMessage(string message)
        {
            PokemonInfo info = new PokemonInfo();
            var a = "[378 seconds remaining] 100.8% IV - snorlax at 51.537633610483,-0.035369255277754 [ Moveset: LickFast/Earthquake ]\nJynx 90IV 40.677105409698, -73.446518725481";

            var lines = message.Split(new[] { '\r', '\n' });
            foreach (var input in lines)
            {
                {
                    Match match = Regex.Match(input, @"(?<lat>\-?\d+(\.\d+)?),\s*(?<long>\-?\d+(\.\d+)?)");
                    if (match.Success)
                    {
                        info.latitude = Convert.ToDouble(match.Groups["lat"].Value);
                        info.longitude = Convert.ToDouble(match.Groups["long"].Value);
                    }
                    else
                    {
                        Console.WriteLine($"Can't get coords from line: {lines}");
                        continue;
                    }
                }

                {
                    Match match = Regex.Match(input, @"(\d+\.?\d*)\%?\s?IV", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        info.iv = Convert.ToDouble(match.Groups[1].Value);
                    }
                }

                {
                    Match match = Regex.Match(input, @"(\d+)\s?sec", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        info.timeStamp = DateTime.Now.AddSeconds(Convert.ToDouble(match.Groups[1].Value));
                    }
                }

                foreach (string name in Enum.GetNames(typeof(PokemonId)))
                {
                    if (input.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        info.id = (PokemonId)Enum.Parse(typeof(PokemonId), name);
                    }
                }

                await feedToClients(info);
            }
        }

        public void Start()
        {
            var server = "PoGo Sniping";
            var channel = "snipers";
            var discordToken = "MjA4MzE2MjQxOTQzNzI0MDMz.Cnv5Hg.JpdTfgfIdw5445Cnnu6QbLHnZDc";

            _client = new DiscordClient();

            StartNet();

            _client.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor && e.Server.ToString() == server && e.Channel.ToString() == channel)
                {
                    await parseMessage(e.Message.Text);
                }
            };

            _client.ExecuteAndWait(async () => {
                await _client.Connect(discordToken);
            });
        }
    }

}
