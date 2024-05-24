using ExifLibrary;
using System.Globalization;

namespace Klinkby.Frickr2
{
    public static class ExifExtensions
    {
        public static bool SetString(this ExifPropertyCollection<ExifProperty> props, ExifTag tag, string? value)
        {
            value ??= string.Empty;
            ExifProperty? old = props.FirstOrDefault(x => tag == x.Tag);
            if (null != old)
            {
                if (value == (string)old.Value)
                {
                    return false;
                }

                old.Value = value;
            }
            else
            {
                props.Add(tag, value);
            }

            return true;
        }

        public static bool SetDateTime(this ExifPropertyCollection<ExifProperty> props, ExifTag tag,
            string? stringValue)
        {
            stringValue ??= string.Empty;
            ExifDateTime value = new ExifDateTime(ExifTag.DateTime,
                DateTime.Parse(stringValue, CultureInfo.InvariantCulture));
            ExifProperty? old = props.FirstOrDefault(x => tag == x.Tag);
            if ((DateTime?)old?.Value == value.Value)
            {
                return false;
            }

            props.Add(tag, value);
            return true;
        }

        public static bool SetGps(this ExifPropertyCollection<ExifProperty> props, ExifTag tag,
            string ddValue)
        {
            float value = float.Parse($"{ddValue[..2]}.{ddValue[2..]}", CultureInfo.InvariantCulture);
            (float Degrees, float Minutes, float Seconds) dms = Spacial.DecimalDegreesToDms(value);
            GPSLatitudeLongitude newLat;
            try
            {
                newLat = new GPSLatitudeLongitude(tag, dms.Degrees, dms.Minutes, dms.Seconds);
            }
            catch (ArgumentException e)
            {
                return false;
            }

            MathEx.UFraction32[] oldLat = props.Get<GPSLatitudeLongitude>(tag)?.Value ?? new MathEx.UFraction32[3];
            bool changed = newLat.Degrees != oldLat[0]
                           || newLat.Minutes != oldLat[1]
                           || newLat.Seconds != oldLat[2];
            if (!changed)
            {
                return false;
            }

            props.Add(newLat);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault -- only GPS tags are relevant
            switch (tag)
            {
                case ExifTag.GPSLatitude:
                    props.Add(new ExifEnumProperty<GPSLatitudeRef>(ExifTag.GPSLatitudeRef,
                        ddValue[0] == '-' ? GPSLatitudeRef.South : GPSLatitudeRef.North));
                    break;
                case ExifTag.GPSLongitude:
                    props.Add(new ExifEnumProperty<GPSLongitudeRef>(ExifTag.GPSLongitudeRef,
                        ddValue[0] == '-' ? GPSLongitudeRef.East : GPSLongitudeRef.West));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tag));
            }

            return true;
        }
    }
}