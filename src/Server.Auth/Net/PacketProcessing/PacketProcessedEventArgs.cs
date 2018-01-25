using System;
using Server.Auth.Net.Packets;

namespace Server.Auth.Net.PacketProcessing
{
    internal class PacketProcessedEventArgs : EventArgs
    {
        public PacketProcessedEventArgs(PacketIn packetIn)
        {
            PacketIn = packetIn;
        }

        public PacketIn PacketIn { get; }
    }
}
