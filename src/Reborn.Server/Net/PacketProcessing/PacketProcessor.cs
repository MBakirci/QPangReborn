using System;
using System.IO;
using Reborn.Server.Net.Packets;

namespace Reborn.Server.Net.PacketProcessing
{
    public class PacketProcessor
    {
        private readonly ClientHandlerBase _client;

        private byte[] _bufferOld;

        public PacketProcessor(ClientHandlerBase client, int bufferSize)
        {
            _client = client;
            _bufferOld = new byte[0];

            Buffer = new byte[bufferSize];
        }

        public byte[] Buffer { get; }

        public void Process(int bytesReceived)
        {
            var bytes = new byte[_bufferOld.Length + bytesReceived];

            System.Buffer.BlockCopy(_bufferOld, 0, bytes, 0, _bufferOld.Length);
            System.Buffer.BlockCopy(Buffer, 0, bytes, _bufferOld.Length, bytesReceived);

            using (var memoryStream = new MemoryStream(bytes))
            using (var binaryReader = new BinaryReader(memoryStream))
            {
                var positionSaved = 0;

                if (bytes.Length >= 4)
                {
                    var packetLength = binaryReader.ReadInt16();
                    if (packetLength <= bytes.Length)
                    {
                        binaryReader.BaseStream.Seek(-2, SeekOrigin.Current);

                        var packet = new PacketIn(binaryReader.ReadBytes(packetLength));

                        positionSaved = (int) memoryStream.Position;

                        packet.Verify();
                        packet.DecryptPayload(_client.Crypto.BlowfishSecond ?? _client.Crypto.BlowfishInitial);

                        OnPacketProcessed(packet);
                    }
                }
                
                _bufferOld = binaryReader.ReadBytes((int) (memoryStream.Length - positionSaved));
            }
        }

        private void OnPacketProcessed(PacketIn packet)
        {
            PacketProcessed?.Invoke(this, new PacketProcessedEventArgs(packet));
        }

        public event EventHandler<PacketProcessedEventArgs> PacketProcessed;
    }
}
