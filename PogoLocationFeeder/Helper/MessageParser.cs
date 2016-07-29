using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Helper
{
    internal class MessageParser
    {
        private SniperInfo sniperInfo = null;
        public List<SniperInfo> parseMessage(string message)
        {
            var snipeList = new List<SniperInfo>();
            var lines = message.Split(new[] { '\r', '\n' });

            foreach (var input in lines)
            {
                sniperInfo = new SniperInfo();
                if (!parseGeoCoordinates(input))
                {
                    //Console.WriteLine($"Can't get coords from line: {input}"); // debug output, too much spam
                    continue;
                }
                parseIV(input);
                parseTimestamp(input);
                parsePokemonId(input);

                snipeList.Add(sniperInfo);
            }

            return snipeList;
        }

        private bool parseGeoCoordinates(string input)
        {
            Match match = Regex.Match(input, @"(?<lat>\-?\d+(\.\d+)+),\s*(?<long>\-?\d+(\.\d+)+)");
            if (match.Success)
            {
                sniperInfo.latitude = Convert.ToDouble(match.Groups["lat"].Value);
                sniperInfo.longitude = Convert.ToDouble(match.Groups["long"].Value);
            }
            return match.Success;
        }

        private void parseIV(string input)
        {
            Match match = Regex.Match(input, @"(\d+\.?\d*)\%?\s?IV", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                sniperInfo.iv = Convert.ToDouble(match.Groups[1].Value);
            }
        }

        private void parseTimestamp(string input)
        {
            Match match = Regex.Match(input, @"(\d+)\s?sec", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                sniperInfo.timeStamp = DateTime.Now.AddSeconds(Convert.ToDouble(match.Groups[1].Value));
            }
        }

        private void parsePokemonId(string input)
        {
            foreach (string name in Enum.GetNames(typeof(PokemonId)))
            {
                if (input.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    sniperInfo.id = (PokemonId)Enum.Parse(typeof(PokemonId), name);
                    return;
                }
            }
        }
    }
}