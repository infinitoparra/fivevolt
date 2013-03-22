using System;
using System.Collections.Generic;
using System.Linq;
using ZigBeeCoordinator.Lib.Infrastructure;

namespace ZigBeeCoordinator.Lib.Models.Db
{
    public partial class ZigBeeDevice
    {
        public string DeviceAddressHtml
        {
            get { return DeviceAddress.ToHtml(); }
        }

        public string NetworkAddressHtml
        {
            get { return NetworkAddress.ToHtml(); }
        }

        public string DeviceAddressString
        {
            get { return DeviceAddress.ToHexString(""); }
        }
    }

    public partial class ZigBeeDeviceFinder
    {
        public ZigBeeDevice FindByAddressOrDefault(ZigBeeCoordinatorContext ctx, byte[] address)
        {
            List<ZigBeeDevice> devices = ctx.DbCtx.ZigBeeDevices.ToList();
            ZigBeeDevice zigBeeDevice = devices.FirstOrDefault(d => address.EqualsAllItems(d.DeviceAddress));
            if (zigBeeDevice != null)
            {
                return zigBeeDevice;
            }
            return null;
        }

        public ZigBeeDevice FindAddOrUpdate(ZigBeeCoordinatorContext ctx, byte[] address, byte[] networkAddress)
        {
            ZigBeeDevice zigBeeDevice = FindByAddressOrDefault(ctx, address);
            if (zigBeeDevice != null)
            {
                if (!zigBeeDevice.NetworkAddress.EqualsAllItems(networkAddress))
                {
                    zigBeeDevice.NetworkAddress = networkAddress;
                }
            }
            else
            {
                zigBeeDevice = new ZigBeeDevice
                {
                    CreatedDateTime = DateTime.Now,
                    DeviceAddress = address,
                    NetworkAddress = networkAddress
                };
                ctx.DbCtx.ZigBeeDevices.AddObject(zigBeeDevice);
                ctx.DbCtx.SaveChanges();
            }
            return zigBeeDevice;
        }
    }
}
