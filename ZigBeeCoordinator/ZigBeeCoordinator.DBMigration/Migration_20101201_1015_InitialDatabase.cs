using System.Data;
using Migrator.Framework;

namespace ZigBeeCoordinator.DBMigration
{
    [Migration(201012011015)]
    public class Migration_20101201_1015_InitialDatabase : Migration
    {
        public override void Up()
        {
            Database.AddTable("CoordinatorUser",
                new Column("UserId", DbType.Int32, ColumnProperty.NotNull | ColumnProperty.PrimaryKeyWithIdentity),
                new Column("UserName", DbType.String, 255, ColumnProperty.NotNull | ColumnProperty.Unique),
                new Column("Password", DbType.String, 255, ColumnProperty.NotNull));

            Database.AddTable("ZigBeeDeviceType",
                new Column("ZigBeeDeviceTypeId", DbType.Int32, ColumnProperty.NotNull | ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String, 255, ColumnProperty.NotNull));

            Database.AddTable("ZigBeeDevice",
                new Column("ZigBeeDeviceId", DbType.Int32, ColumnProperty.NotNull | ColumnProperty.PrimaryKeyWithIdentity),
                new Column("DeviceAddress", DbType.Binary, 8, ColumnProperty.NotNull),
                new Column("NetworkAddress", DbType.Binary, 2),
                new Column("CreatedDateTime", DbType.DateTime, ColumnProperty.NotNull),
                new Column("LastPacketDateTime", DbType.DateTime),
                new Column("ZigBeeDeviceTypeId", DbType.Int32));
            Database.AddForeignKey("ZigBeeDevice_ZigBeeDeviceTypeFk", "ZigBeeDevice", "ZigBeeDeviceTypeId", "ZigBeeDeviceType", "ZigBeeDeviceTypeId");

            Database.AddTable("ZigBeeDeviceLog",
                new Column("ZigBeeDeviceLogId", DbType.Int32, ColumnProperty.NotNull | ColumnProperty.PrimaryKeyWithIdentity),
                new Column("ZigBeeDeviceId", DbType.Int32, ColumnProperty.NotNull),
                new Column("DateTime", DbType.DateTime, ColumnProperty.NotNull),
                new Column("PacketData", DbType.Binary, 10000, ColumnProperty.NotNull));
            Database.AddForeignKey("ZigBeeDevice_ZigBeeDeviceLogFk", "ZigBeeDeviceLog", "ZigBeeDeviceId", "ZigBeeDevice", "ZigBeeDeviceId");

            Database.AddTable("Capability",
                new Column("CapabilityId", DbType.Int32, ColumnProperty.NotNull | ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String, 255, ColumnProperty.NotNull),
                new Column("Description", DbType.String, 1000, ColumnProperty.NotNull));

            Database.AddTable("DeviceCapability",
                new Column("ZigBeeDeviceId", DbType.Int32, ColumnProperty.NotNull | ColumnProperty.PrimaryKey),
                new Column("CapabilityId", DbType.Int32, ColumnProperty.NotNull | ColumnProperty.PrimaryKey));
            Database.AddForeignKey("ZigBeeDevice_DeviceCapabilityFk", "DeviceCapability", "ZigBeeDeviceId", "ZigBeeDevice", "ZigBeeDeviceId");
            Database.AddForeignKey("Capability_DeviceCapabilityFk", "DeviceCapability", "CapabilityId", "Capability", "CapabilityId");

            Database.Insert("ZigBeeDeviceType", new[] { "ZigBeeDeviceTypeId", "Name" }, new[] { "1", "Coordinator" });
            Database.Insert("ZigBeeDeviceType", new[] { "ZigBeeDeviceTypeId", "Name" }, new[] { "2", "Router" });
            Database.Insert("ZigBeeDeviceType", new[] { "ZigBeeDeviceTypeId", "Name" }, new[] { "3", "End Device" });

            Database.Insert("CoordinatorUser", new[] { "UserName", "Password" }, new[] { "jferner", "password" });
            Database.Insert("CoordinatorUser", new[] { "UserName", "Password" }, new[] { "alison418", "password" });
            Database.Insert("Capability", new[] { "CapabilityId", "Name", "Description" }, new[] { 0x0100.ToString(), "Scale", "Weight scale" });
        }

        public override void Down()
        {
            Database.RemoveForeignKey("DeviceCapability", "ZigBeeDevice_DeviceCapabilityFk");
            Database.RemoveForeignKey("DeviceCapability", "Capability_DeviceCapabilityFk");
            Database.RemoveForeignKey("ZigBeeDeviceLog", "ZigBeeDevice_ZigBeeDeviceLogFk");
            Database.RemoveTable("CoordinatorUser");
            Database.RemoveTable("ZigBeeDevice");
            Database.RemoveTable("ZigBeeDeviceLog");
            Database.RemoveTable("Capability");
            Database.RemoveTable("DeviceCapability");
            Database.RemoveTable("ZigBeeDeviceType");
        }
    }
}
