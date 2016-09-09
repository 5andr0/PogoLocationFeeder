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

using PogoLocationFeeder.Common;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Helper
{
    public class GeoCoordinatesParser
    {
        public const string GeoCoordinatesRegex = @"(?<lat>\-?\d+[\,|\.]\d+)[,|\s]+(?<long>\-?\d+[\,|\.]\d+)";

        public static GeoCoordinates ParseGeoCoordinates(string input)
        {
            var match = Regex.Match(input, GeoCoordinatesRegex);

            if (!match.Success) return null;

            var geoCoordinates = new GeoCoordinates();
            var latitude = Convert.ToDouble(match.Groups["lat"].Value.Replace(',', '.'),
                CultureInfo.InvariantCulture);
            var longitude = Convert.ToDouble(match.Groups["long"].Value.Replace(',', '.'),
                CultureInfo.InvariantCulture);
            if (Math.Abs(latitude) > 180)
            {
                Log.Debug("Latitude is lower than -180 or higher than 180 for input {0}", input);
                return null;
            }
            if (Math.Abs(longitude) > 180)
            {
                Log.Debug("Longitude is lower than -180 or higher than 180 for input {0}", input);
                return null;
            }

            geoCoordinates.Latitude = Math.Round(latitude,6);
            geoCoordinates.Longitude = Math.Round(longitude, 6);

            return geoCoordinates;
        }
    }
}
