using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Frickr
{
    /// Entry point
    static class Program
    {
        private const string Uncategorized = "Uncategorized";
        internal static ConsoleColor DefaultColor;

        public static int Main(string[] args)
        {
            DefaultColor = Console.ForegroundColor;
            if (2 != args.Length)
            {
                Console.Error.WriteLine("Usage: dotnet run <source-dir> <target-dir>");
                return 1;
            }
            var sourceDir = args[0];
            var targetDir = args[1];
            Run(sourceDir, targetDir);
            return 0;
        }

        private static void Run(string sourceDir, string targetDir)
        {
            var sw = Stopwatch.StartNew();
            var albumPhotos = GetPhotos(sourceDir);
            var albumNames
                = albumPhotos.Values
                             .Select(x => x.AlbumName)
                             .Distinct();
            var albumPathMap = CreateAlbums(targetDir, albumNames);
            int count = IteratePhotos(sourceDir, albumPhotos, albumPathMap);
            Console.WriteLine(Environment.NewLine + $"Transmogriffed {albumPhotos.Count} media into {albumPathMap.Count} albums in {sw.Elapsed.TotalSeconds}s");
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

        private static int IteratePhotos(
            string sourceDir,
            IDictionary<string, (string AlbumName, Photo Photo)> albumPhotos,
            IDictionary<string, string> albumPathMap)
        {
            var tempTargetPath = Path.GetTempFileName();
            try
            {
                int i = 0;
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
                            dataArchive.WithEntry(
                                file.Name,
                                s => TargetWriter.Write((tempTargetPath, albumPhoto.AlbumName, albumPhoto.Photo, s)));
                            i++;
                            var targetPath = Path.Combine(albumPath, fileName);
                            if (File.Exists(targetPath))
                            {
                                if (AreSame(tempTargetPath, albumPath, fileName, targetPath)) continue;
                                fileName = $"{Path.GetFileNameWithoutExtension(fileName)} ({file.Id}){Path.GetExtension(fileName)}";
                                targetPath = Path.Combine(albumPath, fileName);
                                if (File.Exists(targetPath) && AreSame(tempTargetPath, albumPath, fileName, targetPath)) continue;
                            }
                            Console.Write($" > {Path.GetFileName(albumPath)}/{fileName}");
                            File.Move(tempTargetPath, targetPath);
                            Console.WriteLine();
                        }
                    }
                }
                return i;
            }
            finally
            {
                File.Delete(tempTargetPath);
            }
        }

        private static bool AreSame(string tempTargetPath, string albumPath, string fileName, string targetPath)
        {
            using (var fOld = File.OpenRead(targetPath))
            using (var fNew = File.OpenRead(tempTargetPath))
            {
                if (fOld.Length == fNew.Length) // TODO CRC
                {
                    Console.Write($" > {Path.GetFileName(albumPath)}/{fileName}");
                    Console.WriteLine(" (already exists)");
                    return true;
                }
            }
            return false;
        }
    }
}
