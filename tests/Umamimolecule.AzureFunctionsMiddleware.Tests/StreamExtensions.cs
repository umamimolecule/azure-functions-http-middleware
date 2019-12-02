using System.IO;

namespace Umamimolecule.AzureFunctionsMiddleware.Tests
{
    public static class StreamExtensions
    {
        public static string ReadAsString(this Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            var result = reader.ReadToEnd();
            stream.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}
