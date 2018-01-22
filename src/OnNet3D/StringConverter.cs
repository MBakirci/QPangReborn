using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OnNet3D
{
    public class StringConverter
    {
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

        public static string DecryptStream(string inputStr)
        {
            var input = Encoding.ASCII.GetBytes(inputStr);
            var output = new List<byte>();
            
            var i = 0;
            while (i < input.Length)
            {
                var v7 = input[i + 1];
                var v8 = (byte)(input[i] & 3);
                var v9 = i + 1;
                var v10 = input[v9 + 1];
                v9 += 2;
                var v11 = (byte)(input[v9] & 3);
                i = v9 + 1;
                output.Add((byte)(v11 | 4 * (v10 & 3 | 4 * (v7 & 3 | 4 * v8))));
            }

            var outputBytes = output.ToArray();

            return Encoding.UTF8.GetString(outputBytes, 0, outputBytes.Length);
        }
    }
}
