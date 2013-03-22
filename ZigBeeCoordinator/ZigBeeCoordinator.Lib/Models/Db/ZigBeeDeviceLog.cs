using System;
using log4net;
using ZigBeeCoordinator.Lib.Controllers;
using ZigBeeCoordinator.Lib.Infrastructure;
using ZigBeeCoordinator.Lib.Models.Other;

namespace ZigBeeCoordinator.Lib.Models.Db
{
    public partial class ZigBeeDeviceLog
    {
        public string PacketDataHtml
        {
            get { return PacketData.ToHtml(); }
        }
    }

    public partial class ZigBeeDeviceLogFinder
    {
        private static readonly ILog _log = MyLogManager.GetLogger(typeof(ZigBeeDeviceLogFinder));

        public void LogPacket(ZigBeeCoordinatorContext ctx, ZigBeeCommunicationsController.Packet packet)
        {
            ZigBeeDevice device = ZigBeeDevice.Finder.FindAddOrUpdate(ctx, packet.Address, packet.NetworkAddress);
            device.LastPacketDateTime = DateTime.Now;

            ZigBeeDeviceLog log = new ZigBeeDeviceLog
            {
                DateTime = packet.ReceivedDataTime,
                PacketData = packet.PacketData,
                ZigBeeDevice = device
            };
            ctx.DbCtx.ZigBeeDeviceLogs.AddObject(log);
            ctx.DbCtx.SaveChanges();

            if (packet is ZigBeeCommunicationsController.ZigBeeReceivePacket)
            {
                LogReceivePacket(ctx, device, (ZigBeeCommunicationsController.ZigBeeReceivePacket)packet);
            }
        }

        private void LogReceivePacket(ZigBeeCoordinatorContext ctx, ZigBeeDevice device, ZigBeeCommunicationsController.ZigBeeReceivePacket packet)
        {
            FiveVoltPacket fiveVoltPacket = FiveVoltPacket.Decode(packet.ReceiveData);
            if (fiveVoltPacket is InvalidFileVoltPacket)
            {
                InvalidFileVoltPacket invalidPacket = (InvalidFileVoltPacket)fiveVoltPacket;
                _log.Error("Invalid five volt packet. Reason \"" + invalidPacket.Reason + "\"");
            }
            else if (fiveVoltPacket is CapabilityFiveVoltPacket)
            {
                CapabilityFiveVoltPacket capabilityFiveVoltPacket = (CapabilityFiveVoltPacket)fiveVoltPacket;
                LogCapabilityFiveVoltPacket(ctx, device, capabilityFiveVoltPacket);
            }
            else if (fiveVoltPacket is CapabilityDataFiveVoltPacket)
            {
                CapabilityDataFiveVoltPacket capabilityDataFiveVoltPacket = (CapabilityDataFiveVoltPacket)fiveVoltPacket;
                LogCapabilityDataFiveVoltPacket(ctx, device, capabilityDataFiveVoltPacket);
            }
            else if (fiveVoltPacket is MessageFiveVoltPacket)
            {
                MessageFiveVoltPacket messageFiveVoltPacket = (MessageFiveVoltPacket)fiveVoltPacket;
                LogMessageFiveVoltPacket(ctx, device, messageFiveVoltPacket);
            }
            else
            {
                _log.Error("Unhandled five volt packet type [" + fiveVoltPacket.GetType() + "]");
            }
        }

        private void LogMessageFiveVoltPacket(ZigBeeCoordinatorContext ctx, ZigBeeDevice device, MessageFiveVoltPacket packet)
        {
            string message = "Message from [" + device.NetworkAddress.ToHexString() + "]: " + packet.Message;
            switch (packet.Level)
            {
                case MessageLevel.Debug: _log.Debug(message); break;
                case MessageLevel.Info: _log.Info(message); break;
                case MessageLevel.Warning: _log.Warn(message); break;
                case MessageLevel.Error: _log.Error(message); break;
                default:
                    _log.Error(message + " (Invalid message level: " + packet.Level + ")");
                    break;
            }
        }

        private void LogCapabilityDataFiveVoltPacket(ZigBeeCoordinatorContext ctx, ZigBeeDevice device, CapabilityDataFiveVoltPacket packet)
        {
            Capability capability = Capability.Finder.FindByIdOrDefault(ctx, packet.Capability);
            if (capability == null)
            {
                _log.Error("Could not find capability with id [" + packet.Capability + "]");
                return;
            }

            capability.ProcessData(ctx, device, packet.Data);
        }

        private void LogCapabilityFiveVoltPacket(ZigBeeCoordinatorContext ctx, ZigBeeDevice device, CapabilityFiveVoltPacket capabilityFiveVoltPacket)
        {
            Capability capability = Capability.Finder.FindByIdOrDefault(ctx, capabilityFiveVoltPacket.Capability);
            if (capability == null)
            {
                _log.Error("Could not find capability with id [" + capabilityFiveVoltPacket.Capability + "]");
                return;
            }

            _log.Info("New capability found [" + capability.Name + "] at address [" + device.DeviceAddressString + "]");
            device.Capabilities.Add(capability);
        }
    }
}
