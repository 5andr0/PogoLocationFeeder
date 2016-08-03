using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Helper
{
    public class MessageParser
    {
        private SniperInfo sniperInfo;

        public List<SniperInfo> parseMessage(string message)
        {
            var snipeList = new List<SniperInfo>();
            var lines = message.Split('\r', '\n');

            foreach (var input in lines)
            {
                sniperInfo = new SniperInfo();
                var geoCoordinates = GeoCoordinatesParser.ParseGeoCoordinates(input);
                if (geoCoordinates == null)
                {
                    Log.Debug($"Can't get coords from line: {input}");
                    continue;
                }
                sniperInfo.Latitude = geoCoordinates.Latitude;
                sniperInfo.Longitude = geoCoordinates.Longitude;
                var iv = IVParser.ParseIV(input);
                sniperInfo.IV = iv;
                parseTimestamp(input);
                var pokemon = PokemonParser.ParsePokemon(input);
                sniperInfo.Id = pokemon;
                snipeList.Add(sniperInfo);
            }

            return snipeList;
        }


        private void parseTimestamp(string input)
        {
            try
            {
                var match = Regex.Match(input, @"(\d+)\s?sec", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    sniperInfo.ExpirationTimestamp = DateTime.Now.AddSeconds(Convert.ToDouble(match.Groups[1].Value));
                    return;
                }

                match = Regex.Match(input, @"(\d+)\s?min", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    sniperInfo.ExpirationTimestamp = DateTime.Now.AddMinutes(Convert.ToDouble(match.Groups[1].Value));
                    return;
                }

                match = Regex.Match(input, @"(\d+)m\s?(\d+)s", RegexOptions.IgnoreCase);
                    // Aerodactyl | 14m 9s | 34.008105111711,-118.49775510959
                if (match.Success)
                {
                    sniperInfo.ExpirationTimestamp =
                        DateTime.Now.AddMinutes(Convert.ToDouble(match.Groups[1].Value))
                            .AddSeconds(Convert.ToDouble(match.Groups[2].Value));
                    return;
                }

                match = Regex.Match(input, @"(\d+)\s?s\s", RegexOptions.IgnoreCase);
                    // Lickitung | 15s | 40.69465351234,-73.99434315197
                if (match.Success)
                {
                    sniperInfo.ExpirationTimestamp = DateTime.Now.AddSeconds(Convert.ToDouble(match.Groups[1].Value));
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }
    }
}