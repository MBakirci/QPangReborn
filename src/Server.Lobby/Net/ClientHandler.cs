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
                        .WriteEmpty(54)
                        .WriteStringUnicode("Reborn...\0")
                    );
                    break;
            }
        }
    }
}
