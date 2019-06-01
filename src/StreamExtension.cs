using System.IO;
using Newtonsoft.Json;

namespace Frickr
{
    internal static class StreamExtensions
    {
        public static object DeserializeJson(this Stream stream)
        {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize(jsonTextReader);
            }
        }
    }
}