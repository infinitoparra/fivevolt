using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using log4net;
using ZigBeeCoordinator.Lib.Controllers;
using ZigBeeCoordinator.Lib.Infrastructure;
using ZigBeeCoordinator.Lib.Models;
using ZigBeeCoordinator.Lib.Models.Db;

namespace ZigBeeCoordinator.Console
{
    [Export(typeof(IConsoleCommand))]
    public class ServerCommand : IConsoleCommand
    {
        private static readonly ILog _log = MyLogManager.GetLogger(typeof(ServerCommand));

        [Import]
        public ZigBeeCommunicationsController _zigBeeCommunicationsController { get; set; }

        public void Execute(List<string> args)
        {
            if (args.Count != 2)
            {
                System.Console.Error.WriteLine("Invalid arguments");
                PrintHelp();
                return;
            }

            _zigBeeCommunicationsController.PortName = args[0];
            _zigBeeCommunicationsController.BaudRate = int.Parse(args[1]);
            _zigBeeCommunicationsController.PacketArrived += PacketArrived;
            _zigBeeCommunicationsController.Start();
            System.Console.WriteLine("Press any key to stop");
            System.Console.ReadLine();
            _zigBeeCommunicationsController.Stop();
        }

        private void PacketArrived(ZigBeeCommunicationsController source, ZigBeeCommunicationsController.Packet packet)
        {
            try
            {
                if (_log.IsDebugEnabled)
                {
                    _log.Debug("Packet received [length: " + packet.PacketData.Length + "]\n" + packet.PacketData.ToHexStringWithAscii(16));
                }
                using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
                {
                    ZigBeeDeviceLog.Finder.LogPacket(ctx, packet);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Could not process packet [length: " + packet.PacketData.Length + "]\n" + packet.PacketData.ToHexStringWithAscii(16), ex);
            }
        }

        public void PrintHelp()
        {
            System.Console.WriteLine(Name + " [portName] [baudRate]");
        }

        public string Name
        {
            get { return "Server"; }
        }

        public string Description
        {
            get { return "Starts a ZigBee Coordinator server."; }
        }
    }
}
