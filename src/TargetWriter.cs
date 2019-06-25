using System;
using System.IO;
using System.Linq;
using ExifLibrary;

namespace Frickr
{
    /// Serialize target photo or movie file
    internal static class TargetWriter
    {
        private const string MovExtension = ".mov";
        private const string Mp4Extension = ".mp4";

        public static void Write((string TargetPath, string AlbumName, Photo Photo, Stream Stream) x)
        {
            using (var source = new MemoryStream())
            {
                x.Stream.CopyTo(source);
                source.Seek(0, SeekOrigin.Begin);
                x.Stream = source;
                try
                {
                    WritePhoto(x);
                }
                catch (NotValidImageFileException)
                {
                    source.Seek(0, SeekOrigin.Begin);
                    WriteRaw(x);
                }
                catch (Exception e)
                {
                    source.Seek(0, SeekOrigin.Begin);
                    WriteRaw(x);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($" *{e.GetType().Name}*");
                    Console.ForegroundColor = Program.DefaultColor;
                }
            }
        }

        private static void WritePhoto((string TargetPath, string AlbumName, Photo Photo, Stream Stream) x)
        {
            ImageFile imageFile = ImageFile.FromStream(x.Stream);
            var props = imageFile.Properties;
            SanitizeProperties(props);
            SetProperties(props, x.AlbumName, x.Photo);
            imageFile.Save(x.TargetPath);
        }

        private static void SanitizeProperties(ExifPropertyCollection props)
        {
            var duplicate  = props.GroupBy(x => x.Tag).Select(x => x.Skip(1));
            foreach(var d in duplicate.SelectMany(x => x).ToArray())
            {
                props.Remove(d);
            }
        }

        private static void WriteRaw((string TargetPath, string AlbumName, Photo Photo, Stream Stream) x)
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
            props.AddOrUpdate(ExifTag.WindowsKeywords, string.Join("; ", p.Tags.Concat(new [] { albumName })));
        }
    }
}