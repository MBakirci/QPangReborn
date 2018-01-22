using System.IO;
using OnNet3D;

namespace Tools.ConfDecrypt
{
    internal class Program
    {
        private const string InputDirectory = "Input";

        private const string OutputDirectory = "Output";

        private static void Main(string[] args)
        {
            // Make sure the directories exist.
            Directory.CreateDirectory(InputDirectory);
            Directory.CreateDirectory(OutputDirectory);

            // Decrypt all files in it.
            foreach (var fileName in Directory.GetFiles(InputDirectory))
            {
                var fileExtension = Path.GetExtension(fileName);
                var fileDestination = Path.GetFileNameWithoutExtension(fileName);
                var fileContents = StringConverter.DecryptFile(fileName);

                File.WriteAllText(Path.Combine(OutputDirectory, $"{fileDestination}_decrypted{fileExtension}"), fileContents);
            }
        }
    }
}
