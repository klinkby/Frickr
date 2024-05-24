namespace Klinkby.Frickr2
{
    public record Metadata(
        IReadOnlyCollection<string> Albums,
        IReadOnlyDictionary<string, string> GeoMap,
        IReadOnlyDictionary<string, string> StringMap);
}