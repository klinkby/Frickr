using System.IO;
using ExifLibrary;

namespace Frickr
{
    internal static class TargetWriter
    {
        private const string MovieExtension = ".mov";

        public static void Write((string TargetPath, string AlbumName, Photo Photo, Stream Stream) x)
        {
            switch(x.Photo.Extension.ToLowerInvariant())
            {
                case MovieExtension:
                    WriteVideo(x);
                    break;
                default:
                    WritePhoto(x);
                    break;
            }
        }

        private static void WritePhoto((string TargetPath, string AlbumName, Photo Photo, Stream Stream) x)
        {
            ImageFile imageFile;
            using (var source = new MemoryStream())
            {
                x.Stream.CopyTo(source);
                imageFile = ImageFile.FromStream(source);
            }
            var props = imageFile.Properties;
            SetProperties(props, x.AlbumName, x.Photo);
            imageFile.Save(x.TargetPath);
        }

        private static void WriteVideo((string TargetPath, string AlbumName, Photo Photo, Stream Stream) x)
        {
            using (var target = File.OpenWrite(x.TargetPath))
            {
                x.Stream.CopyTo(target);
            }
        }

        private static void SetProperties(ExifPropertyCollection props, string albumName, Photo p)
        {
            // https://www.exiv2.org/tags.html
            props.AddOrUpdate(ExifTag.ImageUniqueID, p.Id);
            props.AddOrUpdate(ExifTag.ImageDescription, p.Description);
            props.AddOrUpdate(ExifTag.WindowsSubject, p.Description);
            props.AddOrUpdate(ExifTag.WindowsComment, p.Privacy.ToString());
            props.AddOrUpdate(ExifTag.WindowsTitle, p.Name);
            props.AddOrUpdate(ExifTag.WindowsKeywords, albumName);
            foreach (var tag in p.Tags)
            {
                props.Add(ExifTag.WindowsKeywords, tag);
            }
        }
    }
}