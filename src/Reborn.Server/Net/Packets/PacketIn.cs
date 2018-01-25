using System;
using Reborn.Server.Net.Utils;
using Reborn.Utils.Crypto;

namespace Reborn.Server.Net.Packets
{
    /// <summary>
    ///     Packet received from QPang client.
    /// </summary>
    public class PacketIn
    {
        private readonly byte[] _packet;

        public PacketIn(byte[] packet)
        {
            _packet = packet;

            // Read packet header
            Length = NetUtils.ReadShort(new[] {packet[1], packet[0]});
            Unknown0 = _packet[2];
            CrcByte = _packet[3];
        }

        /// <summary>
        ///     The packet id.
        /// </summary>
        public short Length { get; }

        public byte Unknown0 { get; }

        /// <summary>
        ///     Byte 0xFFFFFF00 of the crc32 checksum generated from the (encrypted) payload.
        /// </summary>
        public byte CrcByte { get; }

        /// <summary>
        ///     The payload of the packet. (decrypted)
        /// </summary>
        public PacketInPayload Payload { get; private set; }

        /// <summary>
        ///     Grabs the payload from the packet and decrypts it
        ///     so you can use it.
        /// </summary>
        /// <param name="blowfish">The <see cref="Blowfish"/> instance to use.</param>
        public void DecryptPayload(Blowfish blowfish)
        {
            if (_packet.Length <= 4)
            {
                throw new Exception("This packet does not have a payload.");
            }

            var payloadBytes = new byte[_packet.Length - 4];
            Buffer.BlockCopy(_packet, 4, payloadBytes, 0, payloadBytes.Length);

            Payload = new PacketInPayload(blowfish.Decrypt_ECB(payloadBytes));
            Payload.Verify();
        }

        /// <summary>
        ///     Verifies the encrypted packet.
        /// </summary>
        public void Verify()
        {
            // TODO: crc32 and check with the crcbyte. If it doesn't match, throw exception.
        }
    }
}