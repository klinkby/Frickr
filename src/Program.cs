using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Frickr
{
    static class Program
    {
        private const string Uncategorized = "Uncategorized";
        readonly static ConsoleColor DefaultColor = Console.ForegroundColor;

        public static int Main(string[] args)
        {
            if (2 != args.Length)
            {
                Console.Error.WriteLine("Usage: frickr.exe <source-dir> <target-dir>");
                return 1;
            }
            var sourceDir = args[0];
            var targetDir = args[1];
            Run(sourceDir, targetDir);
            return 0;
        }

        private static void Run(string sourceDir, string targetDir)
        {
            var albumPhotos = GetPhotos(sourceDir);
            var albumNames
                = albumPhotos.Values
                             .Select(x => x.AlbumName)
                             .Distinct();
            var albumPathMap = CreateAlbums(targetDir, albumNames);
            IteratePhotos(sourceDir, albumPhotos, albumPathMap);
        }

        private static IDictionary<string, (string AlbumName, Photo Photo)> GetPhotos(string sourceDir)
        {
            Console.WriteLine($"Looking for index archive in {sourceDir}");
            var indexArchivePaths = Directory.GetFiles(sourceDir, "*_*_part*.zip");
            Console.WriteLine($"> found {indexArchivePaths.Length}");
            var map = new Dictionary<string, (string Album, Photo Photo)>();
            if (0 == indexArchivePaths.Length)
            {
                return map;
            }
            foreach (var path in indexArchivePaths)
            {
                using (var s = File.OpenRead(path))
                using (var index = new IndexArchive(s))
                {
                    var photoAlbumMap
                        = index.Albums
                               .SelectMany(a => a.PhotoIds.Select(photoId => (PhotoId: photoId, AlbumName: a.Title)))
                               .GroupBy(x => x.PhotoId)
                               .Select(x => x.First())
                               .ToDictionary(k => k.PhotoId, v => v.AlbumName);
                    foreach (var p in index.Photos)
                    {
                        string albumName;
                        photoAlbumMap.TryGetValue(p.Id, out albumName);
                        map.Add(p.Id, (albumName, p));
                    }
                }
            }
            return map;
        }

        private static IDictionary<string, string> CreateAlbums(string targetDir, IEnumerable<string> albumNames)
        {
            var map = new Dictionary<string, string>();
            var allAlbums = albumNames.Where(x => !string.IsNullOrEmpty(x)).Concat(new[] { "" });
            foreach (var album in allAlbums)
            {
                var albumDir = "" == album ? Uncategorized : FileName.Encode(album);
                var path = Path.Combine(targetDir, albumDir);
                Directory.CreateDirectory(path);
                map.Add(album, path);
            }
            return map;
        }

        private static void IteratePhotos(
            string sourceDir,
            IDictionary<string, (string AlbumName, Photo Photo)> albumPhotos,
            IDictionary<string, string> albumPathMap)
        {
            var imageArchivePaths = Directory.GetFiles(sourceDir, "data-download-*.zip");
            foreach (var path in imageArchivePaths)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Path.GetFileName(path));
                Console.ForegroundColor = DefaultColor;
                using (var dataStream = File.OpenRead(path))
                using (var dataArchive = new DataArchive(dataStream))
                {
                    foreach (var file in dataArchive.Files)
                    {
                        if (!albumPhotos.ContainsKey(file.Id))
                        {
                            Console.Error.WriteLine($"No metadata for photo {file.Id}");
                            continue;
                        }
                        var albumPhoto = albumPhotos[file.Id];
                        var albumPath = albumPathMap[albumPhoto.AlbumName ?? ""];
                        var fileName = FileName.Encode(albumPhoto.Photo.Name) + albumPhoto.Photo.Extension;
                        var targetPath = Path.Combine(albumPath, fileName);
                        dataArchive.WithEntry(
                            file.Name,
                            s => TargetWriter.Write((targetPath, albumPhoto.AlbumName, albumPhoto.Photo, s)));
                        Console.WriteLine($" > {Path.GetFileName(albumPath)}/{fileName}");
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
