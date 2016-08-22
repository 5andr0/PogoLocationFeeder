using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PogoLocationFeeder.Helper;

namespace PogoLocationFeeder.Common
{
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

        public static bool Validate(GeoCoordinates latLng)
        {
            return Validate(latLng.Latitude, latLng.Longitude);
        }

        public static bool Validate(LatLngBounds bounds)
        {
            return (Validate(bounds.SouthWest) && Validate(bounds.NorthEast));
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
            if ((pointLat >= sw.Latitude && pointLat <= ne.Latitude) &&
                (pointLng >= sw.Longitude && pointLng <= ne.Longitude))
                return true;

            //advance check
            bool eastBound = pointLng < ne.Longitude;
            bool westBound = pointLng > sw.Longitude;

            bool inLong = (ne.Longitude < sw.Longitude) ? (eastBound || westBound) : (eastBound && westBound);
            bool inLat = pointLat > sw.Latitude && pointLat < ne.Latitude;

            if (!(inLat && inLong))
            {
                Log.Trace($"SnipeInfo Lat \"{pointLat}\", Lng \"{pointLat}\" not in bounds.");
            }

            return (inLat && inLong);
        }
    }
}
