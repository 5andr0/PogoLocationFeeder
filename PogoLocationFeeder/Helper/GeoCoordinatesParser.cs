using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Helper
{
    public class GeoCoordinatesParser
    {
        public const string GeoCoordinatesRegex = @"(?<lat>\-?\d+(?:[\,|\.]\d+)?)[,|\s]+(?<long>\-?\d+(?:[\,|\.]\d+)?)";

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

            geoCoordinates.Latitude = latitude;
            geoCoordinates.Longitude = longitude;

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
}