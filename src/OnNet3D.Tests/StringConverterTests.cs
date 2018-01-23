using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace OnNet3D.Tests
{
    public class StringConverterTests
    {
        private readonly ITestOutputHelper output;

        public StringConverterTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("EotK", "s")]
        [InlineData("EotKyfp{ACXJMnJemgX`}sqL", "script")]
        [InlineData("ejlEa[M@QKela[DzYvrAiVLvus}uA{YPUz}E", "attribute")]
        public void ShouldDecrypt(string encryptedString, string decryptedString)
        {
            Assert.Equal(StringConverter.DecryptStream(encryptedString), decryptedString);
        }

        [Theory]
        [InlineData("s")]
        [InlineData("this is\na test\r\nhi\tbye")]
        public void ShouldEncrypt(string test)
        {
            output.WriteLine(BitConverter.ToString(Encoding.ASCII.GetBytes(StringConverter.DecryptStream(StringConverter.EncryptStream(test)))));

            Assert.Equal(test, StringConverter.DecryptStream(StringConverter.EncryptStream(test)));
        }
    }
}
