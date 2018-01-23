using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NLog;
using Reborn.Utils;
using Reborn.Utils.Extensions;

namespace Server.Updater
{
    internal class ClientHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Socket _socket;

        private readonly IPEndPoint _remoteEndPoint;

        private readonly byte[] _buffer;

        public ClientHandler(Socket socket)
        {
            _socket = socket;
            _remoteEndPoint = (IPEndPoint)_socket.RemoteEndPoint;
            _buffer = new byte[4096];

            Logger.Warn($"Accepted new connection from {_remoteEndPoint.Address}.");
        }

        public void WaitForData()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
        }

        private void SendData(byte[] data)
        {
            _socket.Send(data);

            Logger.Trace($"{"Sent",-8}: {data.Length} bytes");
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var byteCount = _socket.EndReceive(ar);
                if (byteCount == 0)
                {
                    _socket.Close();
                    _socket.Dispose();

                    Logger.Warn($"Connection from {_remoteEndPoint.Address} was dropped.");
                    return;
                }

                using (var packetBuffer = new MemoryStream(_buffer, 0, byteCount))
                using (var packetReader = new BinaryReader(packetBuffer))
                {
                    var packetLength = packetReader.ReadInt16();
                    if (packetLength > packetReader.BaseStream.Length)
                    {
                        // TODO: Store in a buffer and wait for more.
                        throw new Exception("Not enough bytes have been received to process packet.");
                    }

                    // Show packet information.
                    Logger.Trace($"{"Received",-8}: {byteCount} bytes with packet length {packetLength}.");
                    Logger.Trace($"{"Bytes",-8}: {BitConverter.ToString(_buffer, 0, byteCount)}");

                    // [Packet Id 5] Payload FailReason (int, char[]) BROKEN

                    var packetList = new List<byte>();
                    short packetSize;

                    // Handle packet by packet length.
                    // TODO: Figure out the actual packet id.. so far packet length seems unique and always the same.
                    switch (packetLength)
                    {
                        // Update "QPang.exe".
                        case 16:
                            packetBuffer.Position += 2;

                            var unknown0 = packetReader.ReadInt16();        // 4    static
                            var unknown1 = packetReader.ReadInt16();        // 1    static
                            var unknown2 = packetReader.ReadInt32();        // 1    static
                            var updaterVersion = packetReader.ReadInt32();  // 17   registry

                            if (unknown0 != 4 || unknown1 != 1 || unknown2 != 1)
                            {
                                // Packet payload.
                                packetList.AddRange(EncodeHelper.EncodeInteger(1, true));
                                packetList.AddRange(Encoding.Unicode.GetBytes("Your updater is a bit messed up, please re-download.\0"));

                                // Packet header.
                                packetSize = (short) packetList.Count;
                                packetList.InsertRange(0, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN00
                                packetList.InsertRange(4, EncodeHelper.EncodeShort(packetSize, true));  // [ 2] Packet Length (Payload only)
                                packetList.InsertRange(6, EncodeHelper.EncodeShort(5, true));           // [ 2] Packet Id
                                packetList.InsertRange(8, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN01
                            }
                            else if (updaterVersion != Constants.UpdaterVersion)
                            {
                                // The updater should be updated.
                                var currentDir = Path.GetDirectoryName(GetType().Assembly.Location);
                                var filePath = Path.Combine(currentDir, "Files", Constants.UpdaterFile);
                                var fileBytes = File.ReadAllBytes(filePath);

                                // Packet payload.
                                packetSize = 8;
                                packetList.AddRange(EncodeHelper.EncodeInteger(Constants.UpdaterVersion, true)); // [ 4] FileVersion
                                packetList.AddRange(EncodeHelper.EncodeInteger(fileBytes.Length, true));         // [ 4] FileSize
                                packetList.AddRange(fileBytes);                                                  // [--] FileBytes

                                // Packet header.
                                packetList.InsertRange(0, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN00
                                packetList.InsertRange(4, EncodeHelper.EncodeShort(packetSize, true));  // [ 2] Packet Length (Payload only)
                                packetList.InsertRange(6, EncodeHelper.EncodeShort(3, true));           // [ 2] Packet Id
                                packetList.InsertRange(8, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN01
                            }
                            else
                            {
                                // All is good, next update process please.
                                packetSize = 0;

                                // Packet header.
                                packetList.InsertRange(0, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN00
                                packetList.InsertRange(4, EncodeHelper.EncodeShort(packetSize, true));  // [ 2] Packet Length (Payload only)
                                packetList.InsertRange(6, EncodeHelper.EncodeShort(2, true));           // [ 2] Packet Id
                                packetList.InsertRange(8, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN01
                                
                            }

                            SendData(packetList.ToArray());
                            break;

                        // Update "main" files.
                        case 272:
                            packetBuffer.Position += 2;

                            var unknown3 = packetReader.ReadInt16();    // 260      static
                            var unknown4 = packetReader.ReadInt16();    // 6        static
                            var unknown5 = packetReader.ReadInt32();    // 1        static

                            var projectName = packetReader.ReadQPangString(64);
                            var productName = packetReader.ReadQPangString(64);
                            var productVersion = packetReader.ReadInt32();

                            Logger.Trace($"Project Name: {projectName}");
                            Logger.Trace($"Product Name: {productName}");
                            Logger.Trace($"Project Version: {productVersion}");
                            
                            var productBytes = new byte[0];

                            // Packet payload.
                            using (var buffer = new MemoryStream())
                            using (var writer = new BinaryWriter(buffer))
                            {
                                writer.Write(EncodeHelper.CreatePadding(128));

                                // Main file.
                                switch (productName)
                                {
                                    case "main":
                                        if (productVersion != Constants.MainVersion)
                                        {
                                            var currentDir = Path.GetDirectoryName(GetType().Assembly.Location);
                                            var filePath = Path.Combine(currentDir, "Files", "update.zip");

                                            productBytes = File.ReadAllBytes(filePath);
                                        }

                                        writer.WriteQPangString("main", 63);                                    // Product name
                                        writer.Write(EncodeHelper.EncodeInteger(Constants.MainVersion, true));  // Product version
                                        writer.Write(EncodeHelper.EncodeInteger(productBytes.Length, true));    // Product size
                                        break;

                                        /* case "somefile":
                                            writer.WriteQPangString("somefile", 63);             // Product name
                                            writer.Write(EncodeHelper.EncodeInteger(456, true)); // Product version
                                            writer.Write(EncodeHelper.EncodeInteger(960, true)); // Product size
                                            break; */
                                }

                                // Another file (this does not work properly..)
                                /* if (productName == "main")
                                {
                                    writer.WriteQPangString("somefile", 63);                // Produc namet
                                    writer.Write(EncodeHelper.EncodeInteger(800, true));    // Product version
                                } */

                                // TODO: Not do this ugly hack, throws out of memory error otherwise.
                                writer.Write((byte) 0);
                                writer.Write((byte) 1);

                                // Make sure we send a payload of 1320 bytes!
                                writer.Write(new byte[1320 - buffer.Length]);

                                packetList.AddRange(buffer.ToArray());
                            }
                            
                            // Packet header.
                            packetSize = (short)packetList.Count;
                            packetList.InsertRange(0, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN00
                            packetList.InsertRange(4, EncodeHelper.EncodeShort(packetSize, true));  // [ 2] Packet Length (Payload only)
                            packetList.InsertRange(6, EncodeHelper.EncodeShort(8, true));           // [ 2] Packet Id
                            packetList.InsertRange(8, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN01

                            SendData(packetList.ToArray());

                            // Send product to client.
                            if (productBytes.Length != 0)
                            {
                                SendData(productBytes);
                            }

                            break;

                        default:
                            // Packet payload.
                            packetList.AddRange(EncodeHelper.EncodeInteger(2, true));
                            packetList.AddRange(Encoding.Unicode.GetBytes("Unknown packet was sent to the server.\0"));

                            // Packet header.
                            packetSize = (short)packetList.Count;
                            packetList.InsertRange(0, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN00
                            packetList.InsertRange(4, EncodeHelper.EncodeShort(packetSize, true));  // [ 2] Packet Length (Payload only)
                            packetList.InsertRange(6, EncodeHelper.EncodeShort(5, true));           // [ 2] Packet Id
                            packetList.InsertRange(8, EncodeHelper.CreatePadding(4));               // [ 4] UNKNOWN01
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Something went wrong while processing a request");
            }
            finally
            {
                WaitForData();
            }
        }
    }
}