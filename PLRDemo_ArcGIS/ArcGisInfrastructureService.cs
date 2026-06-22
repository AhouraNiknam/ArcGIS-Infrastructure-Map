using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using System.Text.Json;

namespace PLRDemo_ArcGIS
{
    // ArcGisInfrastructureService.cs
    public interface IInfrastructureService
    {
        Task<InfraQueryResult> QueryAsync(double lat, double lon, double radiusMeters,
            CancellationToken ct);
    }

    public class ArcGisInfrastructureService : IInfrastructureService
    {
        private readonly IHttpClientFactory _http;

        private static readonly LayerDef[] Layers =
 {
    new("Leveed Areas", "leveed_area",
        "https://ags02.sec.usace.army.mil/server/rest/services/NLD2_PUBLIC/FeatureServer", 15),
    new("Pump Stations", "pump_station",
        "https://geospatial.sec.usace.army.mil/dls/rest/services/NLD/Public/FeatureServer", 4),
    new("Dams", "dam",
        "https://geospatial.sec.usace.army.mil/dls/rest/services/NID/National_Inventory_of_Dams_Public_Service/FeatureServer", 0),
};

        public ArcGisInfrastructureService(IHttpClientFactory http) => _http = http;

        public async Task<InfraQueryResult> QueryAsync(double lat, double lon,
            double radiusMeters, CancellationToken ct)
        {
            var client = _http.CreateClient();
            var tasks = Layers.Select(l => QueryLayerAsync(client, l, lat, lon, radiusMeters, ct));
            var results = (await Task.WhenAll(tasks)).Where(r => r is not null).Cast<LayerResult>();
            return new InfraQueryResult(lat, lon, radiusMeters, results.ToList());
        }

        private static async Task<LayerResult?> QueryLayerAsync(HttpClient client,
            LayerDef layer, double lat, double lon, double radiusMeters, CancellationToken ct)
        {
            var geometry = JsonSerializer.Serialize(new
            {
                x = lon,
                y = lat,
                spatialReference = new { wkid = 4326 }
            });

            var qs = new Dictionary<string, string?>
            {
                ["geometry"] = geometry,
                ["geometryType"] = "esriGeometryPoint",
                ["distance"] = radiusMeters.ToString(CultureInfo.InvariantCulture),
                ["units"] = "esriSRUnit_Meter",
                ["inSR"] = "4326",  // 4326 is code for lat/lon GPS
                ["outSR"] = "4326",
                ["spatialRel"] = "esriSpatialRelIntersects",
                ["outFields"] = "*",
                ["returnGeometry"] = "true",
                ["f"] = "geojson",
            };

            var url = QueryHelpers.AddQueryString(
                $"{layer.BaseUrl}/{layer.LayerId}/query", qs);

            try
            {
                using var resp = await client.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync(ct);
                var doc = JsonSerializer.Deserialize<JsonElement>(json);
                // ArcGIS returns HTTP 200 with an "error" object on failure — guard it
                if (doc.TryGetProperty("error", out _)) return null;
                return new LayerResult(layer.Name, layer.FeatureType, doc);
            }
            catch { return null; } // prototype: degrade gracefully per-layer
        }
    }
}
