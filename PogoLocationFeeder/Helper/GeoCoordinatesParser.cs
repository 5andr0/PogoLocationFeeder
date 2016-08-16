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

    public class GeoCoordinates
    {
        public GeoCoordinates()
        {
        }

        public GeoCoordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public static class GeoCoordinateValidator
    {
        /// <summary>
        /// Validates the coordinate.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns>True, if the coordinate is valid, false otherwise.</returns>
        public static bool Validate(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90) return false;
            if (longitude < -180 || longitude > 180) return false;

            return true;
        }
    }

    public class LatLngBounds
    {
        public LatLngBounds()
        {
            this.SouthWest = new GeoCoordinates();
            this.NorthEast = new GeoCoordinates();
        }

        public LatLngBounds(GeoCoordinates sw, GeoCoordinates ne)
        {
            this.SouthWest = sw;
            this.NorthEast = ne;
        }

        public GeoCoordinates SouthWest { get; set; }
        public GeoCoordinates NorthEast { get; set; }

        /// <summary>
        /// Determine if Lat/Lng in Bounds
        /// </summary>
        /// <param name="pointLat">The Latitude point to test</param>
        /// <param name="pointLng">The Longitude point to test</param>
        /// <returns>Returns whether this contains the given LatLng.</returns>
        public bool Intersects(double pointLat, double pointLng)
        {
            var sw = this.SouthWest;
            var ne = this.NorthEast;

            //simple check       
            if ((pointLat >= sw.Latitude && pointLat <= ne.Latitude) && (pointLng >= sw.Longitude && pointLng <= ne.Longitude))
                return true;

            //advance check
            bool eastBound = pointLng < ne.Longitude;
            bool westBound = pointLng > sw.Longitude;

            bool inLong = (ne.Longitude < sw.Longitude) ? (eastBound || westBound) : (eastBound && westBound);
            bool inLat = pointLat > sw.Latitude && pointLat < ne.Latitude;

            if (!(inLat && inLong))
            {
                Log.Info($"SnipeInfo Lat \"{pointLat}\", Lng \"{pointLat}\" not in bounds.");
            }

            return (inLat && inLong);
        }
    }
}
