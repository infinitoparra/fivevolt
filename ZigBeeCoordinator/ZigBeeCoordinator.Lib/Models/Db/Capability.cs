using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using log4net;
using ZigBeeCoordinator.Lib.Infrastructure;
using ZigBeeCoordinator.Lib.Models.Other;

namespace ZigBeeCoordinator.Lib.Models.Db
{
    public partial class Capability
    {
        private static readonly ILog _log = MyLogManager.GetLogger(typeof(Capability));

        public CapabilityHandler CapabilityHandler
        {
            get { return Finder.FindCapabilityHandlerByIdOrDefault(CapabilityId); }
        }

        public void ProcessData(ZigBeeCoordinatorContext ctx, ZigBeeDevice device, byte[] data)
        {
            _log.Debug("Processing data for capability [" + CapabilityId + ": " + Name + "], from [" + device.DeviceAddressString + "], data [" + data.ToHexString() + "]");

            if (CapabilityHandler == null)
            {
                _log.Error("Unknown capability id [" + CapabilityId + "]");
            }
            else
            {
                CapabilityHandler.ProcessData(ctx, device, data);
            }
        }
    }

    public partial class CapabilityFinder
    {
        [ImportMany]
        public List<CapabilityHandler> CapabilityHandlers { get; set; }

        public Capability FindByIdOrDefault(ZigBeeCoordinatorContext ctx, UInt16 capabilityId)
        {
            return ctx.DbCtx.Capabilities.FirstOrDefault(c => c.CapabilityId == capabilityId);
        }

        public CapabilityHandler FindCapabilityHandlerByIdOrDefault(int capabilityId)
        {
            return CapabilityHandlers.FirstOrDefault(c => c.CapabilityId == capabilityId);
        }
    }
}
