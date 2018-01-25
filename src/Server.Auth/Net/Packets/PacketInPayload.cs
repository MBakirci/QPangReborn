using System;
using Server.Auth.Net.Utils;

namespace Server.Auth.Net.Packets
{
    internal class PacketInPayload
    {
        public PacketInPayload(byte[] payload)
        {
            Payload = payload;
            Length = ReadShort(0);
            Id = ReadShort(2);
        }

        /// <summary>
        ///     The raw bytes of the payload.
        /// </summary>
        public byte[] Payload { get; private set; }

        /// <summary>
        ///     The length of the payload.
        /// </summary>
        public short Length { get; }

        /// <summary>
        ///     The id of the payload (packet id).
        /// </summary>
        public short Id { get; }

        /// <summary>
        ///     Reads a byte.
        /// </summary>
        /// <param name="position">Position where the byte is located.</param>
        /// <returns></returns>
        public byte ReadByte(byte position)
        {
            return Payload[position];
        }

        /// <summary>
        ///     Reads a short in little-endian byte order.
        /// </summary>
        /// <param name="position">Position where the short starts.</param>
        /// <returns></returns>
        public short ReadShort(int position)
        {
            return NetUtils.ReadShort(Payload, position);
        }

        /// <summary>
        ///     Reads an integer in little-endian byte order.
        /// </summary>
        /// <param name="position">Position where the integer starts.</param>
        /// <returns></returns>
        public int ReadInt(int position)
        {
            return NetUtils.ReadInt(Payload, position);
        }

        /// <summary>
        ///     Checks and removes the checksum at the end of the payload.
        ///     Verifies the decrypted data.
        /// </summary>
        public void Verify()
        {
            var checksumLength = ReadShort(Payload.Length - 2);
            if (checksumLength > 8)
            {
                return;
            }

            var payloadLength = Payload.Length - checksumLength - 2;

            for (var i = 0; i < checksumLength; i++)
            {
                if ((byte)(Constants.ChecksumBytes[i] ^ Payload[i]) != Payload[payloadLength + i])
                {
                    throw new Exception("Post-decryption checksum mismatch.");
                }
            }

            var newPayload = new byte[payloadLength];
            Buffer.BlockCopy(Payload, 0, newPayload, 0, payloadLength);

            Payload = newPayload;
        }
    }
}
