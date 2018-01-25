using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;
using Reborn.Server.Net;

namespace Reborn.Server
{
    public class RebornServer<T> : IDisposable where T : ClientHandlerBase
    {
        private readonly Logger _logger; 

        private int _nextClientId;

        private readonly Socket _serverSocket;

        public RebornServer(EndPoint endPoint)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(endPoint);
            _serverSocket.Listen(20);
        }

        public void AcceptConnection()
        {
            _logger.Debug("Waiting for a new connection.");

            _serverSocket?.BeginAccept(AcceptConnectionCallback, null);
        }

        private void AcceptConnectionCallback(IAsyncResult ar)
        {
            if (_serverSocket == null)
            {
                return;
            }

            var clientSocket = _serverSocket.EndAccept(ar);
            var clientHandler = (ClientHandlerBase) Activator.CreateInstance(typeof(T), _nextClientId++, clientSocket);

            new Thread(() => clientHandler.WaitForData())
            {
                IsBackground = true
            }.Start();

            AcceptConnection();
        }

        public void Dispose()
        {
            _serverSocket?.Close();
            _serverSocket?.Dispose();
        }
    }
}
