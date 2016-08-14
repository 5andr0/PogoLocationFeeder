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
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Helper
{
    public class MessageParser
    {

        public static List<SniperInfo> ParseMessage(string message)
        {
            var snipeList = new List<SniperInfo>();
            message = Regex.Replace(message, "\n|\r", " ");

            var sniperInfo = new SniperInfo();
            var geoCoordinates = GeoCoordinatesParser.ParseGeoCoordinates(message);
            if (geoCoordinates == null)
            {
                Log.Debug($"Can't get coords from line: {message}");
                return snipeList;
            }
            sniperInfo.Latitude = Math.Round(geoCoordinates.Latitude, 7);
            sniperInfo.Longitude = Math.Round(geoCoordinates.Longitude, 7);
            var iv = IVParser.ParseIV(message);
            sniperInfo.IV = iv;
            var timeStamp = ParseTimestamp(message);
            var pokemon = PokemonParser.ParsePokemon(message);
            sniperInfo.Id = pokemon;
            sniperInfo.ExpirationTimestamp = timeStamp;
            snipeList.Add(sniperInfo);

            return snipeList;
        }


        private static DateTime ParseTimestamp(string input)
        {
            try
            {
                var match = Regex.Match(input, @"(\d+)\s?sec", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return DateTime.Now.AddSeconds(Convert.ToDouble(match.Groups[1].Value));
                }

                match = Regex.Match(input, @"(\d+)\s?min", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return DateTime.Now.AddMinutes(Convert.ToDouble(match.Groups[1].Value));
                }

                match = Regex.Match(input, @"(\d+)m\s?(\d+)s", RegexOptions.IgnoreCase);
                    // Aerodactyl | 14m 9s | 34.008105111711,-118.49775510959
                if (match.Success)
                {
                    return DateTime.Now.AddMinutes(Convert.ToDouble(match.Groups[1].Value))
                            .AddSeconds(Convert.ToDouble(match.Groups[2].Value));
                }

                match = Regex.Match(input, @"(\d+)\s?s\s", RegexOptions.IgnoreCase);
                    // Lickitung | 15s | 40.69465351234,-73.99434315197
                if (match.Success)
                {
                    return DateTime.Now.AddSeconds(Convert.ToDouble(match.Groups[1].Value));
                }
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            return default(DateTime);
        }
    }
}
