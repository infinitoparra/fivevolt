using System.Linq;
using System.Web.Mvc;
using ZigBeeCoordinator.Lib.Models;
using ZigBeeCoordinator.Lib.Infrastructure;
using ZigBeeCoordinator.Lib.Models.Db;

namespace ZigBeeCoordinator.Lib.Controllers
{
    public class DeviceController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "";

            using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
            {
                ViewBag.Devices = ctx.DbCtx.ZigBeeDevices
                    .Include(d => d.ZigBeeDeviceType)
                    .ToList();
            }

            return View();
        }

        public ActionResult Detail(int id)
        {
            using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
            {
                ZigBeeDevice device = ctx.DbCtx.ZigBeeDevices
                    .Include(d => d.ZigBeeDeviceType)
                    .Include(d => d.Capabilities)
                    .FirstOrDefault(d => d.ZigBeeDeviceId == id);
                ctx.DbCtx.LoadProperty(device, d => d.ZigBeeDeviceLogs);
                return View(device);
            }
        }
    }
}
