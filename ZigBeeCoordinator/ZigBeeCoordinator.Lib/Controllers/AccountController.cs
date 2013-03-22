using System.Web.Mvc;
using ZigBeeCoordinator.Lib.Models;
using ZigBeeCoordinator.Lib.Models.Db;
using ZigBeeCoordinator.Lib.Models.Web;

namespace ZigBeeCoordinator.Lib.Controllers
{
    public class AccountController : Controller
    {
        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                CoordinatorUser user;
                using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
                {
                    if ((user = CoordinatorUser.Finder.ValidateUser(ctx, model.UserName, model.Password)) != null)
                    {
                        user.SignIn(model.RememberMe);
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
            {
                CoordinatorUser.Finder.SignOutCurrentUser(ctx);
            }

            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = CoordinatorUser.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
                {
                    if (CoordinatorUser.Finder.GetCurrentUser(ctx).ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                    {
                        return RedirectToAction("ChangePasswordSuccess");
                    }
                }
                ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = CoordinatorUser.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

    }
}
