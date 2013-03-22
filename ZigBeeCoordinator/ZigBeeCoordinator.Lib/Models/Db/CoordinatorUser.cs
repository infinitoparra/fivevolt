using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using ZigBeeCoordinator.Lib.Infrastructure;

namespace ZigBeeCoordinator.Lib.Models.Db
{
    public partial class CoordinatorUser
    {
        public const int MinPasswordLength = 4;

        public void SignIn(bool rememberMe)
        {
            FormsAuthentication.SetAuthCookie(UserName, rememberMe);
            UpdateSession();
        }

        public bool ChangePassword(string name, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public void UpdateSession()
        {
            HttpContext.Current.Session["UserId"] = UserId;
        }
    }

    public partial class CoordinatorUserFinder
    {
        public CoordinatorUser ValidateUser(ZigBeeCoordinatorContext ctx, string userName, string password)
        {
            CoordinatorUser user = GetUserByUserName(ctx, userName);
            string hash = SecurityHelpers.GetMD5HashHexString(password);
            if (string.Equals(user.Password, hash, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(user.Password, password, StringComparison.InvariantCultureIgnoreCase))
            {
                return user;
            }
            return null;
        }

        public CoordinatorUser GetUserByUserName(ZigBeeCoordinatorContext ctx, string userName)
        {
            return ctx.DbCtx.CoordinatorUsers.FirstOrDefault(u => u.UserName == userName);
        }

        public CoordinatorUser GetUserByUserId(ZigBeeCoordinatorContext ctx, int userId)
        {
            return ctx.DbCtx.CoordinatorUsers.FirstOrDefault(u => u.UserId == userId);
        }

        public int? GetCurrentUserId()
        {
            object userIdObj = HttpContext.Current.Session["UserId"];
            if (userIdObj is int)
            {
                return (int)userIdObj;
            }

            if (HttpContext.Current.Request.IsAuthenticated)
            {
                string userName = HttpContext.Current.User.Identity.Name;
                using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
                {
                    CoordinatorUser user = GetUserByUserName(ctx, userName);
                    if (user != null)
                    {
                        user.UpdateSession();
                        return user.UserId;
                    }
                }
            }
            return null;
        }

        public CoordinatorUser GetCurrentUser(ZigBeeCoordinatorContext ctx)
        {
            int? userId = GetCurrentUserId();
            if (userId != null)
            {
                return GetUserByUserId(ctx, userId.Value);
            }
            return null;
        }

        public void SignOutCurrentUser(ZigBeeCoordinatorContext ctx)
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Session.Remove("UserId");
        }
    }
}
