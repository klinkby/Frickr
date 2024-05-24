using ExifLibrary;

namespace Klinkby.Frickr2
{
    internal static class ExifPropertyCollectionExtensions
    {
        public static bool Update(this ExifPropertyCollection<ExifProperty> props, Metadata metadata)
        {
            bool changed
                = props.SetString(ExifTag.ImageUniqueID, metadata.StringMap["id"]);
            changed |= props.SetString(ExifTag.ImageDescription, metadata.StringMap["description"]);
            changed |= props.SetString(ExifTag.WindowsSubject, metadata.StringMap["description"]);
            changed |= props.SetString(ExifTag.WindowsComment, metadata.StringMap["privacy"]);
            changed |= props.SetString(ExifTag.PageName, metadata.StringMap["name"]);
            changed |= props.SetString(ExifTag.DocumentName, metadata.StringMap["name"]);
            changed |= props.SetString(ExifTag.WindowsTitle, metadata.StringMap["name"]);
            changed |= props.SetString(ExifTag.WindowsKeywords, string.Join("; ", metadata.Albums));

            changed |= props.SetDateTime(ExifTag.DateTime, metadata.StringMap["date_taken"]);
            changed |= props.SetDateTime(ExifTag.DateTimeOriginal, metadata.StringMap["date_taken"]);

            changed |= props.SetGps(ExifTag.GPSLatitude, metadata.GeoMap["latitude"]);
            changed |= props.SetGps(ExifTag.GPSLongitude, metadata.GeoMap["longitude"]);

            return changed;
        }
    }
}