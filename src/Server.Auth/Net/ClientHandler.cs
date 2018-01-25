using System;
using System.Net;
using System.Net.Sockets;
using NLog;
using Reborn.Utils;
using Server.Auth.Net.PacketProcessing;
using Server.Auth.Net.Packets;

namespace Server.Auth.Net
{
    internal class ClientHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int _clientId;

        private readonly Socket _socket;

        private readonly IPEndPoint _remoteEndPoint;

        public ClientHandler(int clientId, Socket socket)
        {
            _clientId = clientId;
            _socket = socket;
            _remoteEndPoint = (IPEndPoint) _socket.RemoteEndPoint;

            Crypto = new PacketCrypto();
            Processor = new PacketProcessor(this, 4096);
            Processor.PacketProcessed += ProcessorOnPacketProcessed;

            Logger.Debug($"[{_clientId}] Accepted new connection from {_remoteEndPoint.Address}.");
        }

        public PacketCrypto Crypto { get; }

        public PacketProcessor Processor { get; }

        public void WaitForData()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.BeginReceive(Processor.Buffer, 0, Processor.Buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
        }
        
        public void SendPacket(PacketOut packet)
        {
            var rawPacket = packet.ToArray();

            // Debug to console.
            Logger.Trace($"[{_clientId}] Sending packet:\n" +
                         $"\t{"Id", -9}: {packet.Id}\n" +
                         $"\t{"Length", -9}: {packet.Length}\n" +
                         $"\t{"Encrypted", -9}: {packet.Encrypted}\n" +
                         $"\t{"Hex dump", -9}:\n" +
                         $"{HexUtils.HexDump(rawPacket)}");

            // Send to client.
            _socket.Send(rawPacket);

            // Clean-up packet resources.
            packet.Dispose();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var byteCount = _socket.EndReceive(ar, out var errorCode);
                if (byteCount <= 0 || errorCode != SocketError.Success)
                {
                    _socket.Close();
                    _socket.Dispose();

                    Logger.Debug(errorCode != SocketError.Success 
                        ? $"[{_clientId}] Connection with {_remoteEndPoint.Address} was forcibly dropped, socket error {errorCode}."
                        : $"[{_clientId}] Connection from {_remoteEndPoint.Address} was dropped.");
                    return;
                }

                Processor.Process(byteCount);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"[{_clientId}] An exception occured during receiving data.");
            }
            finally
            {
                WaitForData();
            }
        }

        private void ProcessorOnPacketProcessed(object sender, PacketProcessedEventArgs e)
        {
            var payload = e.PacketIn.Payload;
            
            // Debug to console.
            Logger.Trace($"[{_clientId}] Received packet:\n" +
                         $"\t{"Id",-9}: {payload.Id}\n" +
                         $"\t{"Length",-9}: {payload.Length}\n" +
                         $"\t{"Hex dump",-9}:\n" +
                         $"{HexUtils.HexDump(payload.Payload)}");

            // Handle.
            switch (payload.Id)
            {
                // Blowfish key exchange.
                case 1:
                    SendPacket(new PacketOut(2, Crypto.BlowfishInitial)
                        .WriteByte(new byte[] {0x00, 0x00, 0x00, 0x00})
                        .WriteByte(Crypto.InitializeSecond())
                    );
                    break;
            }
        }
    }
}