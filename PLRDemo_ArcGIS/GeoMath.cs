namespace PLRDemo_ArcGIS
{
    using System;

    public record BoundingBox(double MinLat, double MaxLat, double MinLon, double MaxLon)
    {
        public bool Contains(double lat, double lon) =>
            lat >= MinLat && lat <= MaxLat &&
            lon >= MinLon && lon <= MaxLon;
    }

    public static class GeoMath
    {
        private const double MetersPerDegreeLat = 111_320;

        public static BoundingBox BoxAround(double centerLat, double centerLon, double radiusMeters)
        {
            // North-south: degrees of latitude are uniform everywhere.
            double latDelta = radiusMeters / MetersPerDegreeLat;

            // East-west: shrink by cos(latitude) because longitude lines
            // converge toward the poles. cos() needs radians, so convert first.
            double latInRadians = centerLat * Math.PI / 180.0;
            double metersPerDegreeLon = MetersPerDegreeLat * Math.Cos(latInRadians);
            double lonDelta = radiusMeters / metersPerDegreeLon;

            return new BoundingBox(
                MinLat: centerLat - latDelta,
                MaxLat: centerLat + latDelta,
                MinLon: centerLon - lonDelta,
                MaxLon: centerLon + lonDelta);
        }
    }
}
