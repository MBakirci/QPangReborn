using Reborn.Utils;

namespace Reborn.Server
{
    internal static class Constants
    {
        public static byte[] BlowfishKey = HexUtils.StringToByteArraySlow("66642423323E34357D5F7E2E33384C6160272B52452F252D49613D7C3958283F00");

        public static byte[] ChecksumBytes = {
            0x9C, 0x14, 0xED, 0x29, 0xF2, 0xB5, 0x83, 0x7A
        };
    }
}
