using ExifLibrary;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Klinkby.Frickr2
{
    internal class ExifUpdateManager(
        string workingDirectory,
        string albumsFile,
        string photoJsonFilePattern,
        ILogger<ExifUpdateManager> logger)
    {
        private const int FileBufferSize = 0x4000;

        private static readonly Regex IdPattern = new(@"(?<id>\d+)(?:.json)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture |
            RegexOptions.IgnoreCase |
            RegexOptions.NonBacktracking);

        public async Task Run(CancellationToken cancellationToken = default)
        {
            IReadOnlyDictionary<string, ImmutableArray<string>> photoAlbumMap = await ReadAlbums(cancellationToken);

            IEnumerable<Task<bool>> tasks =
                from photoJsonFile in Directory.EnumerateFiles(workingDirectory, photoJsonFilePattern)
                where !cancellationToken.IsCancellationRequested
                select UpdateMetadata(photoJsonFile, photoAlbumMap, cancellationToken);

            (int total, int updated) =
                tasks.Select(async x => await x) // enumerate sequentially
                    .Aggregate((0, 0), (acc, x) => (acc.Item1 + 1, acc.Item2 + (x.Result ? 1 : 0)));

            logger.LogInformation("Updated {updated} of {total} files", updated, total);
        }

        private async Task<bool> UpdateMetadata(
            string jsonPath,
            IReadOnlyDictionary<string, ImmutableArray<string>> photoAlbumMap,
            CancellationToken cancellationToken)
        {
            using IDisposable? loggerScope = logger.BeginScope(jsonPath);
            logger.LogInformation("Json: {jsonPath}", jsonPath);
            string id = ParseIdFromPath(jsonPath);
            logger.LogInformation("Id: {id}", id);
            string? imagePath = GetImagePath(id);
            if (null == imagePath)
            {
                logger.LogWarning("{value} not found", nameof(imagePath));
                return false;
            }

            logger.LogInformation("Media: {imagePath}", imagePath);
            JsonNode? jsonNode = await GetPhotoMetadata(jsonPath, cancellationToken);
            if (null == jsonNode)
            {
                logger.LogWarning("{value} not found", nameof(jsonNode));
                return false;
            }

            Metadata metadata = new Metadata(
                GetPhotoAlbums(photoAlbumMap, id),
                jsonNode.GetGeoMap(),
                jsonNode.GetStringMap());
            logger.LogInformation("Metadata: {albums} {geos} {strings}",
                metadata.Albums, metadata.GeoMap.Count, metadata.StringMap.Count);

            ImageFile? exif = await OpenImageFile(imagePath);
            if (null == exif)
            {
                logger.LogWarning("{value} not found", nameof(exif));
                return false;
            }

            logger.LogInformation("Format: {format}", exif.Format);

            bool imageUpdated = exif.Properties.Update(metadata);
            logger.LogInformation("Action: {action}", imageUpdated ? "Save" : "Skip");
            if (!imageUpdated || cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            return await SaveImage(imagePath, exif);
        }

        private async Task<bool> SaveImage(string imagePath, ImageFile exif)
        {
            await exif.SaveAsync(imagePath);
            if (!(exif.Errors?.Count > 0))
            {
                return true;
            }

            foreach (ImageError? error in exif.Errors)
            {
                logger.LogError("{error}", error);
            }

            return false;
        }

        private static async Task<JsonNode?> GetPhotoMetadata(string photoJsonFilePath,
            CancellationToken cancellationToken)
        {
            await using FileStream jsonStream = new FileStream(photoJsonFilePath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite, FileBufferSize,
                FileOptions.Asynchronous);
            JsonNode? jsonNode = await JsonNode.ParseAsync(jsonStream, cancellationToken: cancellationToken);
            return jsonNode;
        }


        private string? GetImagePath(string id)
        {
            string? imagePath = FindMediaPath(id);
            if (null != imagePath)
            {
                return imagePath;
            }

            logger.LogError("Media {id} not found", id);
            return null;
        }

        private async Task<ImageFile?> OpenImageFile(string imagePath)
        {
            try
            {
                return await ImageFile.FromFileAsync(imagePath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "{message}", ex.Message);
                return null;
            }
        }

        private ImmutableArray<string> GetPhotoAlbums(
            IReadOnlyDictionary<string, ImmutableArray<string>> photoAlbumMap, string id)
        {
            if (!photoAlbumMap.TryGetValue(id, out ImmutableArray<string> photoAlbums))
            {
                photoAlbums = [];
            }

            logger.LogInformation("Albums: {albums}", photoAlbums.Length);
            return photoAlbums;
        }

        private async Task<IReadOnlyDictionary<string, ImmutableArray<string>>> ReadAlbums(
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Parse {albumsFile} from on {directory}", albumsFile, workingDirectory);
            string filePath = Path.Combine(workingDirectory, albumsFile);
            await using FileStream albumStream = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite,
                FileBufferSize, FileOptions.Asynchronous);
            JsonNode? jsonNode = await JsonNode.ParseAsync(albumStream, cancellationToken: cancellationToken);
            JsonArray? albumsArray = jsonNode?["albums"]?.AsArray();
            if (null == albumsArray)
            {
                return new Dictionary<string, ImmutableArray<string>>();
            }

            (string Title, string[] Photos)[] albums = albumsArray
                .Where(x => x?["title"] != null && x["photos"] != null)
                .Cast<JsonNode>()
                .Select(x => (
                        Title: x["title"]!.GetValue<string>(),
                        Photos: x["photos"]!.AsArray().Where(y => null != y).Cast<JsonNode>()
                            .Select(y => y.GetValue<string>()).ToArray()
                    )
                ).ToArray();
            Dictionary<string, ImmutableArray<string>> photoAlbumMap = ReduceAlbum(albums);
            return photoAlbumMap;
        }

        private static Dictionary<string, ImmutableArray<string>> ReduceAlbum(
            IEnumerable<(string Title, string[] Photos)> albums)
        {
            Dictionary<string, ImmutableArray<string>> photoAlbumMap = albums
                .SelectMany(x => x.Photos.Select(y => (Photo: y, x.Title)))
                .GroupBy(x => x.Photo)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Title).ToImmutableArray());
            return photoAlbumMap;
        }

        private string? FindMediaPath(string id)
        {
            string? imagePath =
                Directory.EnumerateFiles(workingDirectory, $"*_{id}_*")
                    .FirstOrDefault();
            return imagePath;
        }

        private static string ParseIdFromPath(string path)
        {
            string id = IdPattern.Match(path).Groups[1].Value;
            return id;
        }
    }
}