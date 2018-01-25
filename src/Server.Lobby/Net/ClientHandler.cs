using System.Net.Sockets;
using Reborn.Server.Net;
using Reborn.Server.Net.PacketProcessing;
using Reborn.Server.Net.Packets;

namespace Server.Lobby.Net
{
    internal class ClientHandler : ClientHandlerBase
    {
        public ClientHandler(int clientId, Socket socket) : base(clientId, socket)
        {
            Processor.PacketProcessed += ProcessorOnPacketProcessed;
        }

        private void ProcessorOnPacketProcessed(object sender, PacketProcessedEventArgs e)
        {
            var payload = e.PacketIn.Payload;

            // Handle.
            switch (payload.Id)
            {
                // Key exchange.
                case 1:
                    SendPacket(new PacketOut(2, Crypto.BlowfishInitial)
                        .WriteEmpty(4)
                        .WriteByte(Crypto.InitializeSecond())
                    );
                    break;

                // Auth & initialize server browser.
                case 600:
                    SendPacket(new PacketOut(601, Crypto.BlowfishSecond)
                        .WriteEmpty(50, 0x41)
                        .WriteStringUnicode("Reborn...\0")
                        .WriteEmpty(3521, 0x42)
                        .WriteInt(1337)
                    );
                    break;

                // Unknown
                case 762:
                    SendPacket(new PacketOut(763, Crypto.BlowfishSecond)
                        .WriteEmpty(6)
                        .WriteShort(1)
                        .WriteShort(1) // Amount of times there are 122 bytes below.
                        .WriteEmpty(122, 0x43)
                    );
                    break;

                // Unknown
                case 780:
                    var packet = new PacketOut(781, Crypto.BlowfishSecond) // 4 bytes header 
                        .WriteEmpty(4)
                        .WriteShort(1)
                        .WriteShort(1)
                        .WriteShort(1); // Amount of 43 bytes below.

                    packet
                        .WriteInt(0)            // [-29] 4
                        .WriteInt(1)            // [-25] 8
                        .WriteInt(0xAAAABBB)    // [-21] 12
                        .WriteEmpty(17)         // [-17] 29
                        .WriteByte(0x01)        // [0]   30 Bool?
                        .WriteByte(0x00)        // [1]   31 ??
                        .WriteShort(1337)       // [2]   33
                        .WriteByte(0x00)        // [3]   34 ??
                        .WriteByte(0x02)        // [4]   35
                        .WriteEmpty(8);         // [5]   43

                    SendPacket(packet);
                    break;
            }
        }
    }
}