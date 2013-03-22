
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using ZigBeeCoordinator.Lib.Models;
using ZigBeeCoordinator.Lib.Infrastructure;
using ZigBeeCoordinator.Lib.Models.Db;

namespace ZigBeeCoordinator.Lib.Controllers
{
    public class WirelessScaleController : Controller
    {
        private const string GoalWeightSettingKey = "WirelessScaleGoalWeight";

        public ActionResult Index(int userId, string extension)
        {
            ViewBag.Message = "";

            using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
            {
                IEnumerable<WirelessScaleData> datas = GetUserHistory(ctx, userId);
                ViewBag.WirelessScaleDatas = datas;
                ViewBag.GoalWeight = UserSetting.Finder.GetSetting(ctx, userId, GoalWeightSettingKey);

                if (extension == "json")
                {
                    object jsonData = datas
                        .Select(d => new
                        {
                            ReceivedDateTime = d.ReceivedDateTime,
                            Weight = d.Value
                        })
                        .ToList();
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                return View();
            }
        }

        public ActionResult ManualInput(DateTime date, string time, double weight)
        {
            int? userId = CoordinatorUser.Finder.GetCurrentUserId();
            if (userId == null)
            {
                throw new Exception("You must be logged in");
            }

            using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
            {
                DateTime t = DateTime.Parse(date.ToString("d") + " " + time);

                WirelessScaleData data = new WirelessScaleData
                {
                    UserId = userId.Value,
                    ReceivedDateTime = t,
                    Value = weight,
                    ZigBeeDevice = null
                };
                ctx.DbCtx.WirelessScaleDatas.AddObject(data);
                ctx.DbCtx.SaveChanges();
            }

            return RedirectToAction("Index", new { userId = userId });
        }

        public ActionResult UserSettings(double goalWeight)
        {
            int? userId = CoordinatorUser.Finder.GetCurrentUserId();
            if (userId == null)
            {
                throw new Exception("You must be logged in");
            }

            using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
            {
                UserSetting.Finder.SetSetting(ctx, userId.Value, GoalWeightSettingKey, goalWeight.ToString());
                ctx.DbCtx.SaveChanges();
            }

            return RedirectToAction("Index", new { userId = userId });
        }

        public ActionResult DeleteValue(int wirelessScaleDataId)
        {
            int? userId = CoordinatorUser.Finder.GetCurrentUserId();
            if (userId == null)
            {
                throw new Exception("You must be logged in");
            }

            using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
            {
                WirelessScaleData data = ctx.DbCtx.WirelessScaleDatas
                    .FirstOrDefault(d => d.WirelessScaleDataId == wirelessScaleDataId);
                ctx.DbCtx.WirelessScaleDatas.DeleteObject(data);
                ctx.DbCtx.SaveChanges();
            }

            return RedirectToAction("Index", new { userId = userId });
        }

        public enum HistoryChartType
        {
            Week,
            Month,
            All
        }

        public ActionResult HistoryChart(int userId, int? width, int? height, HistoryChartType? chartType)
        {
            using (ZigBeeCoordinatorContext ctx = new ZigBeeCoordinatorContext())
            {
                double minX = UserSetting.Finder.GetSettingAsDouble(ctx, userId, GoalWeightSettingKey, 100.0);

                string title = "History";
                IEnumerable<WirelessScaleData> datas = GetUserHistory(ctx, userId);
                if (chartType == null || chartType.Value == HistoryChartType.All)
                {
                    title = "History - All";
                }
                else if (chartType.Value == HistoryChartType.Month)
                {
                    title = "History - 30-days";
                    DateTime start = DateTime.Now.Subtract(new TimeSpan(30, 0, 0, 0));
                    datas = datas.Where(d => d.ReceivedDateTime > start);
                    WirelessScaleData smallest = datas.Smallest(d => d.Value);
                    if (smallest != null)
                    {
                        minX = smallest.Value;
                    }
                }
                else if (chartType.Value == HistoryChartType.Week)
                {
                    title = "History - 7-days";
                    DateTime start = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
                    datas = datas.Where(d => d.ReceivedDateTime > start);
                    WirelessScaleData smallest = datas.Smallest(d => d.Value);
                    if (smallest != null)
                    {
                        minX = smallest.Value;
                    }
                }
                ViewBag.Chart = BuildHistoryChart(datas, minX, width ?? 400, height ?? 300, title);
                return View();
            }
        }

        private IEnumerable<WirelessScaleData> GetUserHistory(ZigBeeCoordinatorContext ctx, int userId)
        {
            return ctx.DbCtx.WirelessScaleDatas
                .Include(d => d.CoordinatorUser)
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.ReceivedDateTime)
                .ToList();
        }

        private Chart BuildHistoryChart(IEnumerable<WirelessScaleData> datas, double minYVal, int width, int height, string title)
        {
            Chart chart = new Chart(
                width: width,
                height: height);
            chart.AddTitle(title);
            //chart.AddLegend(title: "Legend", name: null);
            chart.SetYAxis(min: minYVal);
            ArrayList xValueArray = new ArrayList();
            ArrayList yValuesArray = new ArrayList();
            foreach (WirelessScaleData data in datas)
            {
                xValueArray.Add(data.ReceivedDateTime.ToString("d"));
                yValuesArray.Add(data.Value);
            }
            chart.AddSeries(
                    name: "History",
                    chartType: "Line",
                    axisLabel: "Weight",
                    xValue: xValueArray,
                    yValues: yValuesArray);
            return chart;
        }
    }
}
