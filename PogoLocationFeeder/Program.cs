using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord;
using POGOProtos.Enums;

namespace PogoLoco
{
    public class PokemonInfo
    {
        public PokemonInfo()
        {
        }
        
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double iv { get; set; }
        public DateTime timeStamp { get; set; }
        public PokemonId id { get; set; }
    }

    class Program
    {
        static void Main(string[] args) => new Program().Start();

        private DiscordClient _client;

        private async Task feedToClients(PokemonInfo info)
        {
            Console.WriteLine(info.latitude);
            Console.WriteLine(info.longitude);
            Console.WriteLine(info.iv);
            Console.WriteLine(info.timeStamp);
            Console.WriteLine(info.id);
        }
        private async Task parseMessage(string message)
        {
            PokemonInfo info = new PokemonInfo();
            //var test = "[378 seconds remaining] 100.8% IV - snorlax at 51.537633610483,-0.035369255277754 [ Moveset: LickFast/Earthquake ]\nJynx 90IV 40.677105409698, -73.446518725481";

            var lines = message.Split(new[] { '\r', '\n' });
            foreach (var input in lines)
            {
                {
                    Match match = Regex.Match(input, @"(?<lat>\-?\d+(\.\d+)?),\s*(?<long>\-?\d+(\.\d+)?)");
                    if (match.Success)
                    {
                        info.latitude = Convert.ToDouble(match.Groups["lat"].Value);
                        info.longitude = Convert.ToDouble(match.Groups["long"].Value);
                    } else
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
                    if (message.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) { 
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

            _client.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor && e.Server.ToString() == server && e.Channel.ToString() == channel) {
                    await parseMessage(e.Message.Text);
                }
            };

            _client.ExecuteAndWait(async () => {
                await _client.Connect(discordToken);
            });
        }
    }

}
