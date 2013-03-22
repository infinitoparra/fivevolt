
using System;
using ZigBeeCoordinator.Lib.Models.Db;

namespace ZigBeeCoordinator.Lib.Models.Other
{
    public abstract class CapabilityHandler
    {
        public abstract UInt16 CapabilityId { get; }
        public abstract void ProcessData(ZigBeeCoordinatorContext ctx, ZigBeeDevice device, byte[] data);
    }
}
