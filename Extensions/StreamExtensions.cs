using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.Serialization.V2.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadStreamToByteArray(this Stream stream)
        {
            byte[] bytes;
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                bytes = memStream.ToArray();
            }

            return bytes;
        }

        public static async Task<byte[]> ReadStreamToByteArrayAsync(this Stream stream)
        {
            byte[] bytes;
            using (var memStream = new MemoryStream())
            {
                await stream.CopyToAsync(memStream)
                    .ConfigureAwait(false);

                bytes = memStream.ToArray();
            }

            return bytes;
        }
    }
}
