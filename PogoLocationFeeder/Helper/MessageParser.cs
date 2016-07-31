﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using POGOProtos.Enums;
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
                GeoCoordinates geoCoordinates = GeoCoordinatesParser.parseGeoCoordinates(input);
                if (geoCoordinates == null)
                {
                    //Console.WriteLine($"Can't get coords from line: {input}"); // debug output, too much spam
                    continue;
                }
                else
                {
                    sniperInfo.latitude = geoCoordinates.latitude;
                    sniperInfo.longitude = geoCoordinates.longitude;
                }
                parseIV(input);
                parseTimestamp(input);
                PokemonId pokemon = PokemonParser.parsePokemon(input);
                sniperInfo.id = pokemon;
                snipeList.Add(sniperInfo);
            }

            return snipeList;
        }

        private double parseRegexDouble(string input, string regex)
        {
            Match match = Regex.Match(input, regex);
            if (match.Success)
            {
                return Convert.ToDouble(match.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            else
                return default(double);
        }

        private bool parseGeoCoordinates(string input)
        {
            Match match = Regex.Match(input, @"(?<lat>\-?\d+[\,\.]\d+),\s*(?<long>\-?\d+[\,\.]\d+)");
            if (match.Success)
            {
                sniperInfo.latitude = Convert.ToDouble(match.Groups["lat"].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
                sniperInfo.longitude = Convert.ToDouble(match.Groups["long"].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            return match.Success;
        }

        private void parseIV(string input)
        {
            sniperInfo.iv = parseRegexDouble(input, @"\s(\d\d?[\,\.]?\d\d?\d?)\s?\%?\s?IV"); // 52 IV 52% IV 52IV 52.5 IV
            if (sniperInfo.iv == default(double))
                sniperInfo.iv = parseRegexDouble(input, @"(\d\d?[\,\.]?\d\d?\d?)\s?\%"); // 52% 52 %
            if (sniperInfo.iv == default(double))
                sniperInfo.iv = parseRegexDouble(input, @"IV\s?(\d\d?[\,\.]?\d\d?\d?)");
        }

        private void parseTimestamp(string input)
        {
            try
            {
                Match match = Regex.Match(input, @"(\d+)\s?sec", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    sniperInfo.timeStamp = DateTime.Now.AddSeconds(Convert.ToDouble(match.Groups[1].Value));
                    return;
                }

                match = Regex.Match(input, @"(\d+)\s?min", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    sniperInfo.timeStamp = DateTime.Now.AddMinutes(Convert.ToDouble(match.Groups[1].Value));
                    return;
                }

                match = Regex.Match(input, @"(\d+)m\s?(\d+)s", RegexOptions.IgnoreCase); // Aerodactyl | 14m 9s | 34.008105111711,-118.49775510959
                if (match.Success)
                {
                    sniperInfo.timeStamp = DateTime.Now.AddMinutes(Convert.ToDouble(match.Groups[1].Value)).AddSeconds(Convert.ToDouble(match.Groups[2].Value));
                    return;
                }

                match = Regex.Match(input, @"(\d+)\s?s\s", RegexOptions.IgnoreCase); // Lickitung | 15s | 40.69465351234,-73.99434315197
                if (match.Success)
                {
                    sniperInfo.timeStamp = DateTime.Now.AddSeconds(Convert.ToDouble(match.Groups[1].Value));
                    return;
                }
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
    }
}
