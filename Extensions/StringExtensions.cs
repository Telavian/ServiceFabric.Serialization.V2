using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ServiceFabric.Serialization.V2.Extensions
{
    public static class StringExtensions
    {
        public static string DecompressText(this byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return "";
            }

            using (var memStream = new MemoryStream(data))
            {
                using (var gzip = new GZipStream(memStream, CompressionMode.Decompress))
                {
                    using (var decodeStream = new MemoryStream())
                    {
                        gzip.CopyTo(decodeStream);
                        var bytes = decodeStream.ToArray();
                        return Encoding.UTF8.GetString(bytes);
                    }
                }
            }
        }

        public static byte[] CompressText(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new byte[0];
            }

            var bytes = Encoding.UTF8.GetBytes(text);

            using (var memStream = new MemoryStream())
            {
                using (var gzip = new GZipStream(memStream, CompressionLevel.Optimal))
                {
                    gzip.Write(bytes, 0, bytes.Length);
                }

                return memStream.ToArray();
            }
        }
    }
}
