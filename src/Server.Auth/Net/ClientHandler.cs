﻿using System.Net.Sockets;
using Reborn.Server.Net;
using Reborn.Server.Net.PacketProcessing;
using Reborn.Server.Net.Packets;

namespace Server.Auth.Net
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

                // Login.
                case 500:
                    SendPacket(new PacketOut(501, Crypto.BlowfishSecond)
                            .WriteEmpty(4)
                            .WriteInt(0x0100007F)
                            .WriteInt(0xAABB)
                            .WriteInt(0xCCDD)
                            .WriteInt(0xEEFF)
                            .WriteInt(0x7777)
                            // .WriteByte(new byte[] {0x05, 0x39}) ??
                    );
                    break;
            }
        }
    }
}
