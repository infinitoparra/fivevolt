
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using log4net;
using ZigBeeCoordinator.Lib.Infrastructure;
using ZigBeeCoordinator.Lib.Models.Db;

namespace ZigBeeCoordinator.Lib.Models.Other
{
    [Export(typeof(CapabilityHandler))]
    public class WirelessScaleCapabilityHandler : CapabilityHandler
    {
        private static readonly ILog _log = MyLogManager.GetLogger(typeof(WirelessScaleCapabilityHandler));

        public override ushort CapabilityId
        {
            get { return 0x0100; }
        }

        public override void ProcessData(ZigBeeCoordinatorContext ctx, ZigBeeDevice device, byte[] data)
        {
            int? userId = data[0];
            float weight = BitConverter.ToInt16(data, 1) / 10.0f;

            // user not specified
            if (userId == 0)
            {
                userId = FindUserBasedOnHistory(ctx, weight);
            }

            if (userId == null)
            {
                _log.Error("Could not find user based on weight '" + weight + "'");
                return;
            }

            _log.Info("Weight received for user [" + userId + "]: " + weight);

            WirelessScaleData wirelessScaleData = new WirelessScaleData
            {
                UserId = userId.Value,
                Value = weight,
                ZigBeeDevice = device,
                ReceivedDateTime = DateTime.Now
            };
            ctx.DbCtx.WirelessScaleDatas.AddObject(wirelessScaleData);
            ctx.DbCtx.SaveChanges();
        }

        private int? FindUserBasedOnHistory(ZigBeeCoordinatorContext ctx, float weight)
        {
            IEnumerable<WirelessScaleData> mostRecentHistory = FindUsersMostRecentData(ctx);
            WirelessScaleData closestData = mostRecentHistory.Smallest(d => Math.Abs(d.Value - weight));
            if (Math.Abs(closestData.Value - weight) > 20)
            {
                return null;
            }
            return closestData.UserId;
        }

        private IEnumerable<WirelessScaleData> FindUsersMostRecentData(ZigBeeCoordinatorContext ctx)
        {
            return ctx.DbCtx.WirelessScaleDatas
                .ToList()
                .GroupBy(q => q.UserId)
                .Select(userData => userData.OrderByDescending(d => d.ReceivedDateTime).FirstOrDefault())
                .ToList();
        }
    }
}
