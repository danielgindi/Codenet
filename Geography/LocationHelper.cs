using System;

namespace Codenet.Geography
{
    public static class LocationHelper
    {
        public const double AVERAGE_KM_PER_LATITUDE_DEGREE = 111.135;
        public const double AVERAGE_KM_PER_LONGITUDE_DEGREE_AT_40_DEGREES = 85.0;
        public const double AVERAGE_KM_EARTH_RADIUS = 6371.0;

        private static readonly double PI;
        private static readonly double DEGREES_TO_RADIANS;

        static LocationHelper()
        {
            PI = Math.Atan(1.0) * 4.0;
            DEGREES_TO_RADIANS = PI / 180.0;
        }

        /// <summary>
        /// Calculates the distance between two coordinates
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        /// <returns>Distance in kilometers</returns>
        public static double DistanceBetweenTwoCoordinates(double lat1, double lon1, double lat2, double lon2)
        {
            lat1 *= DEGREES_TO_RADIANS;
            lon1 *= DEGREES_TO_RADIANS;
            lat2 *= DEGREES_TO_RADIANS;
            lon2 *= DEGREES_TO_RADIANS;
            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double a = Math.Sin(dLat / 2.0);
            a *= a;
            double c = Math.Sin(dLon / 2.0);
            c *= c;
            a += c * Math.Cos(lat1) * Math.Cos(lat2);
            c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            double d = AVERAGE_KM_EARTH_RADIUS * c;

            return d; // Kilometers
        }
    }
}
