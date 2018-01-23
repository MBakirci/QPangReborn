using System;
using System.IO;
using System.Text;

namespace OnNet3D
{
    public class StringConverter
    {
        private static readonly Random Random = new Random((int) (new DateTime(1970, 1, 1) - DateTimeOffset.UtcNow).TotalSeconds);

        public static string DecryptFile(string fileName)
        {
            var result = new StringBuilder();

            using (var reader = File.OpenText(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length != 0)
                    {
                        result.Append(DecryptStream(line));
                        result.Append("\r\n");
                    }
                }
            }

            return result.ToString();
        }

        public static string EncryptStream(string decryptedString)
        {
            var result = new byte[decryptedString.Length * 4];

            for (var i = 0; i < decryptedString.Length; i++)
            {
                var character = decryptedString[i];
                var mask = ~(~Random.Next(33, 124) | 3);

                result[i * 4] = (byte) ((character >> 6) | mask);
                result[i * 4 + 1] = (byte) ((character >> 4) | mask);
                result[i * 4 + 2] = (byte) ((character >> 2) | mask);
                result[i * 4 + 3] = (byte) ((character >> 0) | mask);
            }

            return Encoding.ASCII.GetString(result);
        }

        public static string DecryptStream(string inputStr)
        {
            if (inputStr.Length % 4 != 0)
            {
                throw new ArgumentException($"{nameof(inputStr)} must be a multiple of 4.");
            }

            var input = Encoding.ASCII.GetBytes(inputStr);
            var output = new byte[inputStr.Length / 4];

            for (var i = 0; i < input.Length; i += 4)
            {
                var char1 = input[i] & 3; // Last two bits of the 1st character
                var char2 = input[i + 1] & 3; // Last two bits of the 2nd character
                var char3 = input[i + 2] & 3; // Last two bits of the 3nd character
                var char4 = input[i + 3] & 3; // Last two bits of the 4nd character

                var result0 = char2 | (char1 << 2);
                var result1 = char3 | (result0 << 2);
                var result2 = char4 | (result1 << 2);

                output[i / 4] = (byte) result2;
            }

            return Encoding.ASCII.GetString(output, 0, output.Length);
        }
    }
}