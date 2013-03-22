using System.Web.Mvc;

namespace ZigBeeCoordinator.Lib.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "";

            return View();
        }
    }
}
