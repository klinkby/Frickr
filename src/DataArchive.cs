using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace Frickr
{
    internal partial class DataArchive : IDisposable
    {
        private static readonly Regex IdPattern
            = new Regex(@"_(?<id>\d+)(?:_o)?(?:\.\w{3})$",
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
