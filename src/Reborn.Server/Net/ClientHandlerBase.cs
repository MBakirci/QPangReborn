using System;
using System.Net;
using System.Net.Sockets;
using NLog;
using Reborn.Server.Net.PacketProcessing;
using Reborn.Server.Net.Packets;
using Reborn.Utils;

namespace Reborn.Server.Net
{
    public abstract class ClientHandlerBase
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected readonly int ClientId;

        protected readonly Socket Socket;

        protected readonly IPEndPoint RemoteEndPoint;

        protected ClientHandlerBase(int clientId, Socket socket)
        {
            ClientId = clientId;
            Socket = socket;
            RemoteEndPoint = (IPEndPoint) Socket.RemoteEndPoint;

            Crypto = new PacketCrypto();
            Processor = new PacketProcessor(this, 4096);
            Processor.PacketProcessed += ProcessorOnPacketProcessed;

            Logger.Debug($"[{ClientId}] Accepted new connection from {RemoteEndPoint.Address}.");
        }

        public PacketCrypto Crypto { get; }

        public PacketProcessor Processor { get; }

        public void WaitForData()
        {
            if (Socket != null && Socket.Connected)
            {
                Socket.BeginReceive(Processor.Buffer, 0, Processor.Buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
        }
        
        public void SendPacket(PacketOut packet)
        {
            var rawPacket = packet.ToArray();

            // Debug to console.
            Logger.Trace($"[{ClientId}] Sending packet:\n" +
                         $"\t{"Id", -9}: {packet.Id}\n" +
                         $"\t{"Length", -9}: {packet.Length}\n" +
                         $"\t{"Encrypted", -9}: {packet.Encrypted}\n" +
                         $"\t{"Hex dump", -9}:\n" +
                         $"{HexUtils.HexDump(rawPacket)}");

            // Send to client.
            Socket.Send(rawPacket);

            // Clean-up packet resources.
            packet.Dispose();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var byteCount = Socket.EndReceive(ar, out var errorCode);
                if (byteCount <= 0 || errorCode != SocketError.Success)
                {
                    Socket.Close();
                    Socket.Dispose();

                    Logger.Debug(errorCode != SocketError.Success 
                        ? $"[{ClientId}] Connection with {RemoteEndPoint.Address} was forcibly dropped, socket error {errorCode}."
                        : $"[{ClientId}] Connection from {RemoteEndPoint.Address} was dropped.");
                    return;
                }

                Processor.Process(byteCount);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"[{ClientId}] An exception occured during receiving data.");
            }
            finally
            {
                WaitForData();
            }
        }

        private void ProcessorOnPacketProcessed(object sender, PacketProcessedEventArgs e)
        {
            var payload = e.PacketIn.Payload;

            Logger.Trace($"[{ClientId}] Received packet:\n" +
                         $"\t{"Id",-9}: {payload.Id}\n" +
                         $"\t{"Length",-9}: {payload.Length}\n" +
                         $"\t{"Hex dump",-9}:\n" +
                         $"{HexUtils.HexDump(payload.Payload)}");
        }
    }
}