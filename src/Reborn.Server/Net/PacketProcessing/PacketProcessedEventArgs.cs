using System;
using Reborn.Server.Net.Packets;

namespace Reborn.Server.Net.PacketProcessing
{
    public class PacketProcessedEventArgs : EventArgs
    {
        public PacketProcessedEventArgs(PacketIn packetIn)
        {
            PacketIn = packetIn;
        }

        public PacketIn PacketIn { get; }
    }
}
