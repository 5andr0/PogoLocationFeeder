using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Helper
{
    public class GeoCoordinatesParser
    {
        public const string geoCoordinatesRegex = @"(?<lat>\-?\d+(?:[\,|\.]\d+)?)[,|\s]+(?<long>\-?\d+(?:[\,|\.]\d+)?)";
        public static GeoCoordinates parseGeoCoordinates(string input)
        {
            Match match = Regex.Match(input, geoCoordinatesRegex);
            if (match.Success)
            {
                GeoCoordinates geoCoordinates = new GeoCoordinates();
                var latitude = Convert.ToDouble(match.Groups["lat"].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
                var longitude = Convert.ToDouble(match.Groups["long"].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
                if (Math.Abs(latitude) > 180)
                {
                    Log.Debug("latitude is lower than -180 or higher than 180 for input {0}", input);
                    return null;
                }
                if (Math.Abs(longitude) > 180)
                {
                    Log.Debug("longitude is lower than -180 or higher than 180 for input {0}", input);
                    return null;
                }

                geoCoordinates.latitude = latitude;
                geoCoordinates.longitude = longitude;

                return geoCoordinates;
            }
            return null;
        }
    }

   public class GeoCoordinates
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public GeoCoordinates() { }
        public GeoCoordinates(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }
}
