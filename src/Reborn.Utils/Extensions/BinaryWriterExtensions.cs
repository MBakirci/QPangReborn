using System;
using System.IO;
using System.Text;

namespace Reborn.Utils.Extensions
{
    public static class BinaryWriterExtensions
    {
        public static void WriteQPangString(this BinaryWriter writer, string str, int size)
        {
            if (str.Length > size)
            {
                throw new ArgumentException("String is too long for the given size.");
            }

            var buffer = new byte[(size + 1) * 2];
            var bufferStr = Encoding.Unicode.GetBytes(str);

            Buffer.BlockCopy(bufferStr, 0, buffer, 0, str.Length * 2);

            writer.Write(buffer);
        }
    }
}
