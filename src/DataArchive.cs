using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace Frickr
{
    /// Extract contents of a data-download-#.zip file
    internal partial class DataArchive : IDisposable
    {
        /*
        Match id from
        7176952817_91ac1d3d90_o.jpg
        manfred-p-eventyr_5189977344_o.jpg
        vid_20120906_1417051_7943051628.mp4
        d-fr-bning-_45804173955_o.jpg
        20141119_141647_16343358551_o.jpg
        */
        private static readonly Regex IdPattern
            = new Regex(@"^((?<id>\d{7,})(?:_[a-z\d]+))|((?:[\w_-]+?)_(?<id>\d{7,}))(?:(_o)?.\w{3})$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        private static string ParseIdFromFileName(string fileName)
        {
            var m = IdPattern.Match(fileName);
            if (!m.Success)
            {
                throw new ArgumentException($"Unexpected file name format '{fileName}'");
            }
            return m.Groups["id"].Value;
        }

        private readonly ZipArchive zip;

        public DataArchive(Stream s)
        {
            zip = new ZipArchive(s, ZipArchiveMode.Read, true);
        }

        public IEnumerable<(string Id, string Name)> Files
        {
            get
            {
                return zip.Entries.Select(x => (ParseIdFromFileName(x.Name), x.Name));
            }
        }

        internal void WithEntry(string file, Action<Stream> command)
        {
            var entry = zip.GetEntry(file);
            if (0 == entry.Length)
            {
                Console.Error.WriteLine($"Photo {file} is empty");
                return;
            }
            Console.Write(file);
            using (Stream s = entry.Open())
            {
                command(s);
            }
        }

        public void Dispose()
        {
            zip.Dispose();
        }
    }
}
