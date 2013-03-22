using System.Web.Mvc;

namespace ZigBeeCoordinator.Lib.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "";

            return View();
        }
    }
}
