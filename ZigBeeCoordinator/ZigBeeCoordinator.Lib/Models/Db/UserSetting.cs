using System;
using System.Linq;
using log4net;
using ZigBeeCoordinator.Lib.Infrastructure;

namespace ZigBeeCoordinator.Lib.Models.Db
{
    public partial class UserSetting
    {
    }

    public partial class UserSettingFinder
    {
        private static readonly ILog _log = MyLogManager.GetLogger(typeof(UserSettingFinder));

        public void SetSetting(ZigBeeCoordinatorContext ctx, int userId, string name, string val)
        {
            _log.Debug("Setting user setting \"" + name + "\" = \"" + val + "\" (user id: " + userId + ")");
            UserSetting setting = ctx.DbCtx.UserSettings.FirstOrDefault(us => us.UserId == userId && us.Name == name);
            if (setting == null)
            {
                setting = new UserSetting
                {
                    UserId = userId,
                    Name = name,
                    Value = val
                };
                ctx.DbCtx.UserSettings.AddObject(setting);
            }
            else
            {
                setting.Value = val;
            }
        }

        public string GetSetting(ZigBeeCoordinatorContext ctx, int userId, string name)
        {
            UserSetting setting = ctx.DbCtx.UserSettings.FirstOrDefault(us => us.UserId == userId && us.Name == name);
            if (setting == null)
            {
                return null;
            }
            return setting.Value;
        }

        public double GetSettingAsDouble(ZigBeeCoordinatorContext ctx, int userId, string name, double defaultValue)
        {
            string val = GetSetting(ctx, userId, name);
            if (string.IsNullOrEmpty(val))
            {
                return defaultValue;
            }
            return double.Parse(val);
        }
    }
}
