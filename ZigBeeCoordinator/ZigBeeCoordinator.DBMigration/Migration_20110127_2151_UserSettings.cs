using System;
using System.Data;
using Migrator.Framework;

namespace ZigBeeCoordinator.DBMigration
{
    [Migration(201101272151)]
    public class Migration_20110127_2151_UserSettings : Migration
    {
        public override void Up()
        {
            Database.AddTable("UserSetting",
                new Column("UserSettingId", DbType.Int32, ColumnProperty.NotNull | ColumnProperty.PrimaryKeyWithIdentity),
                new Column("UserId", DbType.Int32, ColumnProperty.NotNull),
                new Column("Name", DbType.String, 255, ColumnProperty.NotNull),
                new Column("Value", DbType.String, 10000, ColumnProperty.Null));
            Database.AddForeignKey("UserSetting_UserFk", "UserSetting", "UserId", "CoordinatorUser", "UserId");
        }

        public override void Down()
        {
            Database.RemoveForeignKey("UserSetting", "UserSetting_UserFk");
            Database.RemoveTable("UserSetting");
        }
    }
}
