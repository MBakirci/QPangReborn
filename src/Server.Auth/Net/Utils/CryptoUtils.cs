using System;

namespace Server.Auth.Net.Utils
{
    internal static class CryptoUtils
    {
        public static byte[] AppendChecksum(byte[] payload)
        {
            var extra = (payload.Length + 2) & 7;
            if (extra == 0 || extra == 8)
            {
                throw new Exception($"Unable to get checksum of packet. [{extra} == 0 || {extra} == 8]");
            }

            var amount = 8 - extra;
            var pad = new byte[payload.Length + amount + 2];
            Buffer.BlockCopy(payload, 0, pad, 0, payload.Length);

            for (var i = 0; i < amount; i++)
            {
                pad[payload.Length + i] = (byte)(payload[i] ^ Constants.ChecksumBytes[i]);
            }

            var amountBytes = NetUtils.WriteShort((short)amount);

            pad[payload.Length + amount] = amountBytes[0];
            pad[payload.Length + amount + 1] = amountBytes[1];

            return pad;
        }
    }
}
