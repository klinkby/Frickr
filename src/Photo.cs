using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Frickr
{
    /// Photo metadata DTO
    internal class Photo
    {
        private static readonly Regex ExtPattern
            = new Regex(@"(?<ext>\.\w{3})$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        public static Photo Parse(dynamic photo)
        {
            var file = new Uri((string)photo.original).Segments.Last();
            var extension = ExtPattern.Match(file).Groups["ext"].Value;
            var tags = ((JArray)photo.tags).Select(x => (string)((dynamic)x).tag);
            var privacy = "public" == (string)photo.privacy ? Privacy.Public : Privacy.Private;
            return new Photo
            {
                Id = photo.id,
                File = file,
                Name = photo.name,
                Extension = extension,
                Description = photo.description,
                License = photo.license,
                Tags = tags,
                Privacy = privacy
            };
        }
        public string Id { get; private set; }
        public string File { get; private set; }
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public string License { get; private set; }
        public string Description { get; private set; }
        public IEnumerable<string> Tags { get; private set; }
        public Privacy Privacy { get; private set; }
        private Photo()
        { }
    }
}