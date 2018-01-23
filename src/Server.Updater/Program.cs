using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;
using Reborn.Utils.Config;

namespace Server.Updater
{
    /// <summary>
    ///     Prototype UpdateServer.
    ///     
    ///     QPang target: 2012-05-02 (s2)
    ///     IP: qpangupdate.realfogs.nl
    ///     Port: 8000
    /// 
    ///     Modify the ip to 127.0.0.1 by opening C:\Windows\System32\drivers\etc\hosts
    ///     and adding this line: "qpangupdate.realfogs.nl 127.0.0.1".
    /// </summary>
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly ManualResetEvent KeepRunning = new ManualResetEvent(false);

        private static bool _shuttingDown;

        private static Socket _serverSocket;

        public static void Main(string[] args)
        {
            // Configure logger.
            LogManager.Configuration = LogConfig.Create();

            // Configure console.
            Console.Title = "QPangReborn | Server.Updater PROTOTYPE";
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                KeepRunning.Set();
            };

            // Start server.
            Logger.Info("Starting up Server.Updater, press CTRL+C to exit properly.");
            
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8000));
            _serverSocket.Listen(20);

            // Start accepting connections.
            AcceptNewConnection();

            // Wait until shutdown (CTL+C).
            KeepRunning.WaitOne();

            // Shutdown server.
            Logger.Warn("Shutting down Server.Updater.");

            _shuttingDown = true;
            _serverSocket.Close();
            _serverSocket.Dispose();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void AcceptNewConnection()
        {
            Logger.Trace("Waiting for a new connection.");

            _serverSocket?.BeginAccept(AcceptConnection, null);
        }

        private static void AcceptConnection(IAsyncResult ar)
        {
            if (_serverSocket == null || _shuttingDown)
            {
                return;
            }

            var clientSocket = _serverSocket.EndAccept(ar);

            new Thread(() => new ClientHandler(clientSocket).WaitForData())
            {
                IsBackground = true
            }.Start();

            AcceptNewConnection();
        }
    }
}
