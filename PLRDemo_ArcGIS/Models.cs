using System.Text.Json;

namespace PLRDemo_ArcGIS
{    public record InfraQueryResult(double Lat, double Lon, double RadiusMeters,
        IReadOnlyList<LayerResult> Layers);
    public record LayerResult(string LayerName, string FeatureType, JsonElement GeoJson);

    public record LayerDef(string Name, string FeatureType, string BaseUrl, int LayerId);
}
