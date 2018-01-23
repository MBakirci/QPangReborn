using System.IO;
using System.Text;

namespace Reborn.Utils.Extensions
{
    public static class BinaryReaderExtensions
    {
        /// <summary>
        ///     QPang strings;
        ///     - have a fixed width
        ///     - are unicode
        ///     - are null-terminated
        /// </summary>
        /// <returns></returns>
        public static string ReadQPangString(this BinaryReader reader, int length)
        {
            var bytes = reader.ReadBytes(length * 2);
            var end = 0;

            while (end < bytes.Length && (bytes[end * 2] != 0 || bytes[end * 2 + 1] != 0))
            {
                end++;
            }

            return Encoding.Unicode.GetString(bytes, 0, end * 2);
        }
    }
}