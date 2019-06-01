using System.IO;
using System.Text.RegularExpressions;

namespace Frickr
{

    internal static class FileName
    {
        private const string Replacement = "_";
        private readonly static Regex InvalidFileNameCharPattern
                = new Regex("[" + Regex.Escape(
            "" +
            Path.DirectorySeparatorChar +
            Path.AltDirectorySeparatorChar +
            new string(Path.GetInvalidFileNameChars())) + "]",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public static string Encode(string name)
        {
            string imageFile = InvalidFileNameCharPattern.Replace(
                name.Trim(),
                Replacement);
            return imageFile;
        }
    }
}