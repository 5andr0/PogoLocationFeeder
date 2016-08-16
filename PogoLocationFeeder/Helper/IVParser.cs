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
using System.Globalization;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Helper
{
    public class IVParser
    {
        public static double ParseIV(string input)
        {
            var iv = ParseRegexDouble(input,
                GeoCoordinatesParser.GeoCoordinatesRegex + @"\W+(?i)IV\W+(1?\d{1,3}(?:[,.]\d{1,3})?)\b");
                // 97,8.200341 IV 98
            if (iv == default(double))
                iv = ParseRegexDouble(input, @"(?i)\b(1?\d{1,3}(?:[,.]\d{1,3})?)\W*\%?\W*IV"); // 52 IV 52% IV 52IV 52.5 IV
            if (iv == default(double))
                iv = ParseRegexDouble(input, @"(?i)\bIV\W?(1?\d{1,2}(?:[,.]\d{1,3})?)");
            if (iv == default(double))
                iv = ParseRegexDouble(input, @"\b(1?\d{1,3}(?:[,.]\d{1,3})?)\W*\%"); // 52% 52 %

            return iv;
        }

        private static double ParseRegexDouble(string input, string regex)
        {
            var match = Regex.Match(input, regex);
            if (match.Success)
            {
                return Convert.ToDouble(match.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            return default(double);
        }
    }
}
