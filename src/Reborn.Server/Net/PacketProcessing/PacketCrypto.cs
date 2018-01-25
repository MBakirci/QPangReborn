using System.Security.Cryptography;
using Reborn.Utils.Crypto;

namespace Reborn.Server.Net.PacketProcessing
{
    public class PacketCrypto
    {
        private static readonly RNGCryptoServiceProvider RandomCrypto = new RNGCryptoServiceProvider();
        
        public PacketCrypto()
        {
            BlowfishInitial = new Blowfish(Constants.BlowfishKey)
            {
                compatMode = true
            };
        }

        public Blowfish BlowfishInitial { get; }

        public Blowfish BlowfishSecond { get; private set; }

        public byte[] InitializeSecond()
        {
            // Generate left part of the key.
            var keyPart = new byte[4];
            RandomCrypto.GetBytes(keyPart);

            if (keyPart[0] + 7 > byte.MaxValue)
            {
                keyPart[0] = byte.MaxValue - 7;
            }

            // Generate full key.
            var key = new byte[]
            {
                (byte) (keyPart[0] + 7), keyPart[1], keyPart[2], keyPart[3], 0x29, 0xA1, 0xD3, 0x56
            };

            // Setup crypto.
            BlowfishSecond = new Blowfish(key)
            {
                compatMode = true
            };

            return keyPart;
        }
    }
}