using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Frickr
{
    internal partial class IndexArchive : IDisposable
    {
        private readonly ZipArchive zip;

        public IndexArchive(Stream s)
        {
            zip = new ZipArchive(s, ZipArchiveMode.Read, true);
        }

        public IEnumerable<(string Title, IEnumerable<string> PhotoIds)> Albums
        {
            get
            {
                dynamic root;
                using (Stream s = zip.GetEntry("albums.json").Open())
                {
                    root = s.DeserializeJson();
                }
                foreach (dynamic a in root.albums)
                {
                    var photos = ((JArray)a.photos).Select(x => x.Value<string>());
                    yield return (a.title, photos);
                }
            }
        }

        public IEnumerable<Photo> Photos
        {
            get
            {
                var photoEntries =
                    zip.Entries
                       .Where(x => x.Name.StartsWith("photo_"));
                foreach (var entry in photoEntries)
                {
                    dynamic root;
                    using (var s = entry.Open())
                    {
                        root = s.DeserializeJson();
                    }
                    yield return Photo.Parse(root);
                }
            }
        }

        internal Photo GetPhoto(string pId)
        {
            dynamic p;
            var entry = zip.GetEntry($"photo_{pId}.json");
            if (null == entry)
            {
                throw new FileNotFoundException($"Photo {pId} not found in index archive");
            }
            using (Stream s = entry.Open())
            {
                p = s.DeserializeJson();
            }
            return Photo.Parse(p);
        }

        public void Dispose()
        {
            zip.Dispose();
        }
    }
}
