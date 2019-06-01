using System.Linq;
using ExifLibrary;

namespace Frickr
{
    internal static class ExifExtensions
    {
        public static void AddOrUpdate(this ExifPropertyCollection props, ExifTag tag, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var old = props.FirstOrDefault(x => tag == x.Tag);
            if (null != old)
            {
                old.Value = value;
            }
            else
            {
                props.Add(tag, value);
            }
        }
    }
}
