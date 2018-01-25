namespace Reborn.Server.Net.Utils
{
    /// <summary>
    ///     Allows for byte-order consistency across multiple platforms.
    /// </summary>
    internal static class NetUtils
    {
        public static byte[] WriteShort(short value)
        {
            return new[]
            {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
            };
        }

        public static short ReadShort(byte[] bytes, int position = 0)
        {
            return (short)((bytes[position + 1] << 8) | bytes[position]);
        }

        public static byte[] WriteInt(int value)
        {
            return new[]
            {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 24) & 0xFF),
            };
        }

        public static int ReadInt(byte[] bytes, int position = 0)
        {
            return (bytes[position + 3] << 24) |
                   (bytes[position + 2] << 16) |
                   (bytes[position + 1] << 8) |
                   bytes[position];
        }
    }
}
