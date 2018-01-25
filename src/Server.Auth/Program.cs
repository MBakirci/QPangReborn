using System;
using System.Net;
using NLog;
using Reborn.Server;
using Reborn.Utils.Config;
using Server.Auth.Net;

namespace Server.Auth
{
    /// <summary>
    ///     Prototype AuthServer.
    ///     
    ///     QPang target: 2012-05-02 (s2)
    ///     IP: qpanggame.realfogs.nl
    ///     Port: 8003
    /// 
    ///     Modify the ip to 127.0.0.1 by opening C:\Windows\System32\drivers\etc\hosts
    ///     and adding this line: "qpanggame.realfogs.nl 127.0.0.1".
    /// </summary>
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static bool _keepRunning;

        public static void Main(string[] args)
        {
            Console.Title = "QPangReborn | Server.Auth PROTOTYPE";

            LogManager.Configuration = LogConfig.Create();
            Logger.Warn("Starting up Server.Auth.");

            _keepRunning = true;

            using (var server = new RebornServer<ClientHandler>(new IPEndPoint(IPAddress.Any, 8003)))
            {
                server.AcceptConnection();

                while (_keepRunning)
                {
                    var command = Console.ReadLine();
                    switch (command)
                    {
                        case "exit":
                        case "quit":
                        case "q":
                            _keepRunning = false;
                            break;
                        default:
                            Logger.Warn("Unknown command, available commands: exit, quit & q.");
                            break;
                    }
                }

                Logger.Warn("Shutting down Server.Auth.");
            }

            Logger.Warn("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
