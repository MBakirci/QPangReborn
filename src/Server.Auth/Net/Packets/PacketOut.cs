using System;
using System.Collections.Generic;
using System.IO;
using Server.Auth.Net.Utils;
using Tools.PacketDecrypt.Lib;

namespace Server.Auth.Net.Packets
{
    /// <summary>
    ///     Packet being sent to the QPang client.
    /// </summary>
    internal class PacketOut : IDisposable
    {
        private readonly MemoryStream _memory;

        private readonly BinaryWriter _writer;

        private readonly Blowfish _encryption;

        public PacketOut(short id, Blowfish encryption = null)
        {
            Id = id;

            _encryption = encryption;
            _memory = new MemoryStream();
            _writer = new BinaryWriter(_memory);

            // Packet header of 8 bytes.
            WriteShort(0);      // Length                   [0-1]
            WriteByte((byte)(Encrypted ? 0x01 : 0x00)); //  [2]
            WriteByte(0x00);    // Unknown0                 [3]

            // Packet payload.
            WriteShort(0);      // Payload length           [4-5]
            WriteShort(Id);     // Payload id               [6-7]
        }

        public short Id { get; }

        public long Length => _memory.Length;

        public bool Encrypted => _encryption != null;

        public PacketOut WriteByte(byte value)
        {
            _writer.Write(value);

            return this;
        }

        public PacketOut WriteByte(byte[] value)
        {
            _writer.Write(value);

            return this;
        }

        public PacketOut WriteShort(short value)
        {
            _writer.Write(NetUtils.WriteShort(value));

            return this;
        }

        public PacketOut WriteInt(int value)
        {
            _writer.Write(NetUtils.WriteInt(value));

            return this;
        }

        public byte[] ToArray()
        {
            var packet = _memory.ToArray();
            var payloadLength = NetUtils.WriteShort((short) (packet.Length - 4));

            // Set payload length.
            packet[4] = payloadLength[0];
            packet[5] = payloadLength[1];

            if (_encryption == null)
            {
                var packetLength = NetUtils.WriteShort((short) packet.Length);
                packet[0] = packetLength[0];
                packet[1] = packetLength[1];

                return packet;
            }

            var packetList = new List<byte>();
            
            // Get the header.
            var header = new byte[4];
            Buffer.BlockCopy(packet, 0, header, 0, 4);

            // Get the payload.
            var payload = new byte[packet.Length - 4];
            Buffer.BlockCopy(packet, 4, payload, 0, packet.Length - 4);

            // Append checksum to payload.
            payload = CryptoUtils.AppendChecksum(payload);

            // Reconstruct the packet.
            packetList.AddRange(header);
            packetList.AddRange(_encryption.Encrypt_ECB(payload));

            // Insert length.
            var packetListLength = NetUtils.WriteShort((short)packetList.Count);
            packetList[0] = packetListLength[0];
            packetList[1] = packetListLength[1];

            return packetList.ToArray();
        }

        public void Dispose()
        {
            _memory?.Dispose();
            _writer?.Dispose();
        }
    }
}