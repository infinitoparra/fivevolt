using System.Data;
using Migrator.Framework;

namespace ZigBeeCoordinator.DBMigration
{
    [Migration(201012150943)]
    public class Migration_20101215_0943_WirelessScale : Migration
    {
        public override void Up()
        {
            Database.AddTable("WirelessScaleData",
                new Column("WirelessScaleDataId", DbType.Int32, ColumnProperty.NotNull | ColumnProperty.PrimaryKeyWithIdentity),
                new Column("UserId", DbType.Int32, ColumnProperty.NotNull),
                new Column("ZigBeeDeviceId", DbType.Int32, ColumnProperty.Null),
                new Column("ReceivedDateTime", DbType.Date, ColumnProperty.NotNull),
                new Column("Value", DbType.Double, ColumnProperty.NotNull));
            Database.AddForeignKey("WirelessScaleData_UserFk", "WirelessScaleData", "UserId", "CoordinatorUser", "UserId");
            Database.AddForeignKey("WirelessScaleData_ZigBeeDeviceFk", "WirelessScaleData", "ZigBeeDeviceId", "ZigBeeDevice", "ZigBeeDeviceId");
        }

        public override void Down()
        {
            Database.RemoveForeignKey("WirelessScaleData", "WirelessScaleData_UserFk");
            Database.RemoveForeignKey("WirelessScaleData", "WirelessScaleData_ZigBeeDeviceFk");
            Database.RemoveTable("WirelessScaleData");
        }
    }
}
