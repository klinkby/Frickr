using System.Text.Json;
using System.Text.Json.Nodes;

namespace Klinkby.Frickr2
{
    internal static class JsonNodeExtensions
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyGeoMap = new Dictionary<string, string>
        {
            ["latitude"] = "00000000", ["longitude"] = "00000000"
        };

        public static IReadOnlyDictionary<string, string> GetGeoMap(this JsonNode value)
        {
            Dictionary<string, string>? geoMap =
                value.AsObject()
                    .FirstOrDefault(x => x.Key == "geo" && x.Value?.GetValueKind() == JsonValueKind.Object)
                    .Value?.AsObject()
                    .ToDictionary(x => x.Key, x => x.Value!.GetValue<string>());
            return geoMap ?? EmptyGeoMap;
        }

        public static IReadOnlyDictionary<string, string> GetStringMap(this JsonNode value)
        {
            Dictionary<string, string> stringMap =
                value.AsObject()
                    .Where(x => x.Value?.GetValueKind() == JsonValueKind.String)
                    .ToDictionary(x => x.Key, x => x.Value!.GetValue<string>());
            return stringMap;
        }
    }
}