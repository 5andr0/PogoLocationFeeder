using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PogoLocationFeeder.Helper
{
    public class GeoCoordinatesParser
    {
        public static GeoCoordinates parseGeoCoordinates(string input)
        {
            Match match = Regex.Match(input, @"(?<lat>\-?\d+[\,|\.]\d+)[,|\s]*(?<long>\-?\d+[\,|\.]\d+)");
            if (match.Success)
            {
                GeoCoordinates geoCoordinates = new GeoCoordinates();
                geoCoordinates.latitude = Convert.ToDouble(match.Groups["lat"].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
                geoCoordinates.longitude = Convert.ToDouble(match.Groups["long"].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
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
