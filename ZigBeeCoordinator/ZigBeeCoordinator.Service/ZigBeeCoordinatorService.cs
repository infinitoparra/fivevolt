using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using log4net;
using ZigBeeCoordinator.Lib.Controllers;
using ZigBeeCoordinator.Lib.Infrastructure;
using ZigBeeCoordinator.Lib.Models;
using ZigBeeCoordinator.Lib.Models.Db;

namespace ZigBeeCoordinator.Service
{
    public partial class ZigBeeCoordinatorService : ServiceBase
    {
        private static readonly ILog _log = MyLogManager.GetLogger(typeof(ZigBeeCoordinatorService));
        private string _portName = "COM3";
        private int _baudRate = 9600;

        [Import]
        public ZigBeeCommunicationsController _zigBeeCommunicationsController { get; set; }

        public ZigBeeCoordinatorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log.Info("BEGIN OnStart (args: " + args.JoinAsString(", ") + ")");
            try
            {
                LoadConfiguration();

                IocContainer.Instance.Compose(this);

                // verify connection with database
                _log.Debug("Verifing connection with database");
                ZigBeeCoordinatorContext.ConnectionString = "metadata=res://*/Models.Db.ZigBeeCoordinatorModel.csdl|res://*/Models.Db.ZigBeeCoordinatorModel.ssdl|res://*/Models.Db.ZigBeeCoordinatorModel.msl;provider=System.Data.SqlClient;provider connection string=\"Data Source=fileserver;Initial Catalog=ZigBeeCoordinator;Integrated Security=True;MultipleActiveResultSets=True\"";
                using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
                {
                    ctx.DbCtx.CoordinatorUsers.FirstOrDefault();
                }

                _zigBeeCommunicationsController.PortName = _portName;
                _zigBeeCommunicationsController.BaudRate = _baudRate;
                _zigBeeCommunicationsController.PacketArrived += PacketArrived;
                _zigBeeCommunicationsController.Start();
            }
            catch (Exception ex)
            {
                _log.Error("Could not start", ex);
                throw;
            }
            _log.Info("END OnStart");
            base.OnStart(args);
        }

        private void LoadConfiguration()
        {
            string fileName = Path.Combine(Path.GetDirectoryName(typeof(ZigBeeCoordinatorService).Assembly.Location), "ZigBeeCoordinator.config");
            _log.Info("Loading config file: " + fileName);
            string[] configLines = File.ReadAllLines(fileName);
            foreach (string configLine in configLines)
            {
                string[] parts = configLine.Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }

                string name = parts[0].Trim().ToLowerInvariant();
                string value = parts[1].Trim();
                switch (name)
                {
                    case "port":
                        _portName = value;
                        break;
                    case "baudrate":
                        _baudRate = int.Parse(value);
                        break;
                    default:
                        _log.Warn("Invalid config '" + configLine + "'");
                        break;
                }
            }
        }

        protected override void OnStop()
        {
            _log.Info("BEGIN OnStop");
            _zigBeeCommunicationsController.Stop();
            _log.Info("END OnStop");
            base.OnStop();
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
    }
}
