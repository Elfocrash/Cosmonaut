using System.IO;
using System.Text;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Storage
{
    internal class SimpleStringSerializer
    {
        internal static string FromStream(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        internal static Stream ToStream(string input)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, Encoding.Default, 1024, true))
            {
                streamWriter.Write(input);
                streamWriter.Flush();
            }

            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}