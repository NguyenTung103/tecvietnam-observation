using ES_CapDien.AppCode;
using ES_CapDien.Helpers;
using ES_CapDien.Models;
using ES_CapDien.MongoDb.Service;
using HelperLibrary;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ES_CapDien.Controllers
{
    public class ReportController : Controller
    {
        Cache_BO cacheBO = new Cache_BO();
        private readonly DataObservationMongoService dataObservationMongoService;
        private readonly DataAlarmMongoService dataAlarmMongoService;
        private readonly AreasService areasService;
        private readonly SitesService sitesService;
        private readonly GroupService groupService;
        private readonly ObservationService observationService;
        private readonly UserProfileService userProfileService;
        private readonly ReportTypeService reportTypeService;
        private readonly ReportDailyNhietDoService reportDailyNhietDoService;
        private readonly ReportDailyMucNuocService reportDailyMucNuocService;
        private readonly ReportDailyLuongMuaService reportDailyLuongMuaService;
        private readonly ReportDailyApSuatService reportDailyApSuatService;
        private readonly ReportDailyDoAmService reportDailyDoAmService;
        private readonly ReportDailyTocDoGioService reportDailyTocDoGioService;
        private readonly ReportDailyHuongGioService reportDailyHuongGioService;

        public ReportController()
        {
            dataObservationMongoService = new DataObservationMongoService();
            dataAlarmMongoService = new DataAlarmMongoService();
            areasService = new AreasService();
            sitesService = new SitesService();
            groupService = new GroupService();
            observationService = new ObservationService();
            userProfileService = new UserProfileService();
            reportTypeService = new ReportTypeService();
            reportDailyNhietDoService = new ReportDailyNhietDoService();
            reportDailyMucNuocService = new ReportDailyMucNuocService();
            reportDailyLuongMuaService = new ReportDailyLuongMuaService();
            reportDailyApSuatService = new ReportDailyApSuatService();
            reportDailyDoAmService = new ReportDailyDoAmService();
            reportDailyTocDoGioService = new ReportDailyTocDoGioService();
            reportDailyHuongGioService = new ReportDailyHuongGioService();
        }
        /// <summary>
        /// Báo cáo thường ngày
        /// </summary>
        /// <param name="day"></param>
        /// <param name="areaId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public ActionResult Daily(string Date = "", int? areaId = null, int? deviceId = null, int? groupId = null)
        {
            ViewBag.Title = "";
            ViewBag.MessageStatus = TempData["MessageStatus"];
            ViewBag.Message = TempData["Message"];
            int CurrentUserId = WebMatrix.WebData.WebSecurity.CurrentUserId;
            string userName = User.Identity.Name;
            groupId = userProfileService.userProfileResponsitory.Single(CurrentUserId).Group_Id;
            if (userName == "administrator")
            {
                ViewBag.listSite = sitesService.sitesResponsitory.GetAll().ToList();
                ViewBag.listGroups = groupService.groupResponsitory.GetAll().ToList();
                ViewBag.listArea = areasService.areaResponsitory.GetAll().ToList();
            }
            else
            {
                ViewBag.listGroups = groupService.GetGroups(groupId).ToList();
                ViewBag.listArea = areasService.GetAreasByGroupId(groupId).ToList();
                ViewBag.listSite = sitesService.GetBygroupId(groupId).ToList();
            }            
            DateTime date = DateTime.Today;
            if (Date != "")
            {
                try
                {
                    date = Convert.ToDateTime(Date);
                }
                catch { }
            }
            ReportType rp0 = reportTypeService.GetByCode("RP0").FirstOrDefault();
            ReportType rp1 = reportTypeService.GetByCode("RP1").FirstOrDefault();
            ReportType rp2 = reportTypeService.GetByCode("RP2").FirstOrDefault();
            ReportType rp3 = reportTypeService.GetByCode("RP3").FirstOrDefault();
            ReportType rp4 = reportTypeService.GetByCode("RP4").FirstOrDefault();
            ReportType rp5 = reportTypeService.GetByCode("RP5").FirstOrDefault();
            ReportType rp6 = reportTypeService.GetByCode("RP6").FirstOrDefault();
            Report_NhietDo_DoAm_ApSuat_DailyModel nhietDo = reportDailyNhietDoService.GetByDeviceIdAndDate(date, deviceId).AsEnumerable().Select(s => new Report_NhietDo_DoAm_ApSuat_DailyModel
            {
                Distance1 = s.Distance1,
                Distance2 = s.Distance2,
                Distance3 = s.Distance3,
                Distance4 = s.Distance4,
                Distance5 = s.Distance5,
                Distance6 = s.Distance6,
                Distance7 = s.Distance7,
                Distance8 = s.Distance8,
                Distance9 = s.Distance9,
                Distance10 = s.Distance10,
                Distance11 = s.Distance11,
                Distance12 = s.Distance12,
                Distance13 = s.Distance13,
                Distance14 = s.Distance14,
                Distance15 = s.Distance15,
                Distance16 = s.Distance16,
                Distance17 = s.Distance17,
                Distance18 = s.Distance18,
                Distance19 = s.Distance19,
                Distance20 = s.Distance20,
                Distance21 = s.Distance21,
                Distance22 = s.Distance22,
                Distance23 = s.Distance23,
                Distance24 = s.Distance24,
                MinValue = s.MaxValue, 
                MaxValue = s.MinValue,
                TimeMaxValue = s.TimeMaxValue,
                TimeMinValue=s.TimeMinValue,
                Measure = rp0.Measure,
                ReportTypeCode = s.ReportTypeCode.Trim(),
                ReportTypeName = rp0.Description,
                Title = rp0.Title
            }).FirstOrDefault();
            Report_NhietDo_DoAm_ApSuat_DailyModel doAm = reportDailyDoAmService.GetByDeviceIdAndDate(date, deviceId).AsEnumerable().Select(s => new Report_NhietDo_DoAm_ApSuat_DailyModel
            {
                Distance1 = s.Distance1,
                Distance2 = s.Distance2,
                Distance3 = s.Distance3,
                Distance4 = s.Distance4,
                Distance5 = s.Distance5,
                Distance6 = s.Distance6,
                Distance7 = s.Distance7,
                Distance8 = s.Distance8,
                Distance9 = s.Distance9,
                Distance10 = s.Distance10,
                Distance11 = s.Distance11,
                Distance12 = s.Distance12,
                Distance13 = s.Distance13,
                Distance14 = s.Distance14,
                Distance15 = s.Distance15,
                Distance16 = s.Distance16,
                Distance17 = s.Distance17,
                Distance18 = s.Distance18,
                Distance19 = s.Distance19,
                Distance20 = s.Distance20,
                Distance21 = s.Distance21,
                Distance22 = s.Distance22,
                Distance23 = s.Distance23,
                Distance24 = s.Distance24,
                MinValue = s.MaxValue,
                MaxValue = s.MinValue,
                TimeMaxValue = s.TimeMaxValue,
                TimeMinValue = s.TimeMinValue,
                Measure = rp1.Measure,
                ReportTypeCode = s.ReportTypeCode.Trim(),
                ReportTypeName = rp1.Description,
                Title = rp1.Title
            }).FirstOrDefault();
            Report_NhietDo_DoAm_ApSuat_DailyModel apSuat = reportDailyApSuatService.GetByDeviceIdAndDate(date, deviceId).AsEnumerable().Select(s => new Report_NhietDo_DoAm_ApSuat_DailyModel
            {
                Distance1 = s.Distance1,
                Distance2 = s.Distance2,
                Distance3 = s.Distance3,
                Distance4 = s.Distance4,
                Distance5 = s.Distance5,
                Distance6 = s.Distance6,
                Distance7 = s.Distance7,
                Distance8 = s.Distance8,
                Distance9 = s.Distance9,
                Distance10 = s.Distance10,
                Distance11 = s.Distance11,
                Distance12 = s.Distance12,
                Distance13 = s.Distance13,
                Distance14 = s.Distance14,
                Distance15 = s.Distance15,
                Distance16 = s.Distance16,
                Distance17 = s.Distance17,
                Distance18 = s.Distance18,
                Distance19 = s.Distance19,
                Distance20 = s.Distance20,
                Distance21 = s.Distance21,
                Distance22 = s.Distance22,
                Distance23 = s.Distance23,
                Distance24 = s.Distance24,
                MinValue = s.MaxValue,
                MaxValue = s.MinValue,
                Measure = rp2.Measure,
                TimeMaxValue = s.TimeMaxValue,
                TimeMinValue = s.TimeMinValue,
                Title = rp2.Title,
                ReportTypeCode = s.ReportTypeCode.Trim(),
                ReportTypeName = rp2.Description,
            }).FirstOrDefault();
            Report_MucNuoc_DailyModel mucNuoc = reportDailyMucNuocService.GetByDeviceIdAndDate(date, deviceId).AsEnumerable().Select(s => new Report_MucNuoc_DailyModel
            {
                Distance1 = s.Distance1,
                Distance2 = s.Distance2,
                Distance3 = s.Distance3,
                Distance4 = s.Distance4,
                Distance5 = s.Distance5,
                Distance6 = s.Distance6,
                Distance7 = s.Distance7,
                Distance8 = s.Distance8,
                Distance9 = s.Distance9,
                Distance10 = s.Distance10,
                Distance11 = s.Distance11,
                Distance12 = s.Distance12,
                Distance13 = s.Distance13,
                Distance14 = s.Distance14,
                Distance15 = s.Distance15,
                Distance16 = s.Distance16,
                Distance17 = s.Distance17,
                Distance18 = s.Distance18,
                Distance19 = s.Distance19,
                Distance20 = s.Distance20,
                Distance21 = s.Distance21,
                Distance22 = s.Distance22,
                Distance23 = s.Distance23,
                Distance24 = s.Distance24,
                TimeMaxValue = s.TimeMaxValue,
                TimeMinValue = s.TimeMinValue,
                Measure = rp5.Measure,
                ReportTypeCode = s.ReportTypeCode.Trim(),
                ReportTypeName = rp5.Description,
                Title = rp5.Title
            }).FirstOrDefault();
            Report_LuongMuc_DailyModel luongMua = reportDailyLuongMuaService.GetByDeviceIdAndDate(date, deviceId).AsEnumerable().Select(s => new Report_LuongMuc_DailyModel
            {
                Distance1 = s.Distance1,
                Distance2 = s.Distance2,
                Distance3 = s.Distance3,
                Distance4 = s.Distance4,
                Distance5 = s.Distance5,
                Distance6 = s.Distance6,
                Distance7 = s.Distance7,
                Distance8 = s.Distance8,
                Distance9 = s.Distance9,
                Distance10 = s.Distance10,
                Distance11 = s.Distance11,
                Distance12 = s.Distance12,
                Distance13 = s.Distance13,
                Distance14 = s.Distance14,
                Distance15 = s.Distance15,
                Distance16 = s.Distance16,
                Distance17 = s.Distance17,
                Distance18 = s.Distance18,
                Distance19 = s.Distance19,
                Distance20 = s.Distance20,
                Distance21 = s.Distance21,
                Distance22 = s.Distance22,
                Distance23 = s.Distance23,
                Distance24 = s.Distance24,
                TimeMaxValue = s.TimeMaxValue,
                TimeMinValue = s.TimeMinValue,               
                Measure = rp3.Measure,
                TongLuongMua = CheckValueReport(s.TongLuongMua, s.ReportTypeCode),
                ReportTypeCode = s.ReportTypeCode.Trim(),
                ReportTypeName = rp3.Description,
                Title = rp3.Title
            }).FirstOrDefault();
            Report_TocDoGio_DailyModel tocDoGio = reportDailyTocDoGioService.GetByDeviceIdAndDate(date, deviceId).AsEnumerable().Select(s => new Report_TocDoGio_DailyModel
            {
                Distance1 = s.Distance1,
                Distance2 = s.Distance2,
                Distance3 = s.Distance3,
                Distance4 = s.Distance4,
                Distance5 = s.Distance5,
                Distance6 = s.Distance6,
                Distance7 = s.Distance7,
                Distance8 = s.Distance8,
                Distance9 = s.Distance9,
                Distance10 = s.Distance10,
                Distance11 = s.Distance11,
                Distance12 = s.Distance12,
                Distance13 = s.Distance13,
                Distance14 = s.Distance14,
                Distance15 = s.Distance15,
                Distance16 = s.Distance16,
                Distance17 = s.Distance17,
                Distance18 = s.Distance18,
                Distance19 = s.Distance19,
                Distance20 = s.Distance20,
                Distance21 = s.Distance21,
                Distance22 = s.Distance22,
                Distance23 = s.Distance23,
                Distance24 = s.Distance24,
                TimeMaxValue = s.TimeMaxValue,
                TimeMinValue = s.TimeMinValue,
                Measure = rp6.Measure,
                HuongGioCuaTocDoLonNhat = s.HuongGioCuaTocDoLonNhat,
                HuongGioCuarTocDoNhoNhat =s.HuongGioCuarTocDoNhoNhat,
                TocDoGioLonNhat = s.TocDoGioLonNhat,
                TocDoGioNhoNhat = s.TocDoGioNhoNhat,
                ReportTypeCode = s.ReportTypeCode.Trim(),
                ReportTypeName = rp6.Description,
                Title = rp6.Title
            }).FirstOrDefault();
            Report_HuongGio_DailyModel huongGio = reportDailyHuongGioService.GetByDeviceIdAndDate(date, deviceId).AsEnumerable().Select(s => new Report_HuongGio_DailyModel
            {
                Distance1 = s.Distance1,
                Distance2 = s.Distance2,
                Distance3 = s.Distance3,
                Distance4 = s.Distance4,
                Distance5 = s.Distance5,
                Distance6 = s.Distance6,
                Distance7 = s.Distance7,
                Distance8 = s.Distance8,
                Distance9 = s.Distance9,
                Distance10 =s.Distance10,
                Distance11 =s.Distance11,
                Distance12 =s.Distance12,
                Distance13 =s.Distance13,
                Distance14 =s.Distance14,
                Distance15 =s.Distance15,
                Distance16 =s.Distance16,
                Distance17 =s.Distance17,
                Distance18 =s.Distance18,
                Distance19 =s.Distance19,
                Distance20 =s.Distance20,
                Distance21 =s.Distance21,
                Distance22 =s.Distance22,
                Distance23 =s.Distance23,
                Distance24 =s.Distance24,
                TimeMaxValue = s.TimeMaxValue,
                TimeMinValue = s.TimeMinValue,
                Measure = rp4.Measure,                
                HuongGioDacTrungNhieuNhat= GetTenHuongGio(s.HuongGioDacTrungNhieuNhat),
                HuongGioDacTrungNhieuThuHai= GetTenHuongGio(s.HuongGioDacTrungNhieuThuHai),                
                ReportTypeCode = s.ReportTypeCode.Trim(),
                ReportTypeName = rp4.Description,
                Title = rp4.Title
            }).FirstOrDefault();
            ReportDailyViewModel viewModel = new ReportDailyViewModel
            {
                Date = date,
                NhietDo = nhietDo,
                DoAm = doAm,
                ApSuat = apSuat,
                MucNuoc = mucNuoc,
                LuongMua = luongMua,
                DeviceId = deviceId,
                TocDoGio = tocDoGio,
                HuongGio = huongGio,
                AreaId = areaId,
                GroupId = groupId
            };
            return View(viewModel);
        }
        /// <summary>
        /// Báo cáo theo tháng
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="Date"></param>
        /// <param name="deviceId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<ActionResult> MonthLy(int? areaId = null, string Date = "", int? deviceId = null, string type = "RP0", int? groupId = null)
        {
            ViewBag.Title = "";
            ViewBag.MessageStatus = TempData["MessageStatus"];
            ViewBag.Message = TempData["Message"];
            int CurrentUserId = WebMatrix.WebData.WebSecurity.CurrentUserId;
            string userName = User.Identity.Name;
            groupId = userProfileService.userProfileResponsitory.Single(CurrentUserId).Group_Id;
            ViewBag.listTypeReport = reportTypeService.reportTypeRepository.GetAll().ToList();
            if (!deviceId.HasValue)
            {
                deviceId = sitesService.GetByAreaId(areaId).FirstOrDefault().DeviceId;
            }
            if (userName == "administrator")
            {
                ViewBag.listSite = sitesService.sitesResponsitory.GetAll().ToList();
                ViewBag.listArea = areasService.areaResponsitory.GetAll().ToList();
                ViewBag.listGroups = groupService.groupResponsitory.GetAll().ToList();
            }
            else
            {
                ViewBag.listArea = areasService.GetAreasByGroupId(groupId).ToList();
                ViewBag.listSite = sitesService.GetBygroupId(groupId).ToList();
                ViewBag.listGroups = groupService.GetGroups(groupId).ToList();
            }
            ReportMonthlyModel model = new ReportMonthlyModel();
            model = await cacheBO.GetReportByMonth(deviceId.Value, type, Date);
            DateTime date = DateTime.Today;
            if (Date != "")
            {
                try
                {
                    date = Convert.ToDateTime(Date);
                }
                catch { }
            }
            ReportMonthlyViewModel viewModel = new ReportMonthlyViewModel
            {
                Date = date,
                DeviceId = deviceId,
                data=model,
                AreaId = areaId,

            };
            return View(viewModel);
        }

        /// <summary>
        /// Hàm trả về dữ liệu json
        /// </summary>
        /// <param name="day"></param>
        /// <param name="deviceId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetReportMonthByType(string day = "", int? deviceId = null, string type = "")
        {
            double total1 = 0, total2 = 0, total3 = 0;
            int count1 = 0, count2 = 0, count3 = 0;
            DateTime date = DateTime.Today;
            if (day != "")
            {
                try
                {
                    date = Convert.ToDateTime(day);
                }
                catch { }
            }
            DateTime distance1 = new DateTime(date.Year, date.Month, 1);
            DateTime distance2 = new DateTime(date.Year, date.Month, 10, 23, 59, 59);
            DateTime distance3 = new DateTime(date.Year, date.Month, 11);
            DateTime distance4 = new DateTime(date.Year, date.Month, 20, 23, 59, 59);
            DateTime distance5 = new DateTime(date.Year, date.Month, 21);
            DateTime distance6 = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59);
            var result = await Task.WhenAll(dataObservationMongoService.GetDatayDeviceId(distance1, distance2, deviceId.Value)
                , dataObservationMongoService.GetDatayDeviceId(distance3, distance4, deviceId.Value)
                , dataObservationMongoService.GetDatayDeviceId(distance5, distance6, deviceId.Value));
            foreach (var item in result[0].ToList())
            {
                total1 = Total(item, type.Trim(), total1);
                count1++;
            }
            double tb1 = total1 / count1;
            foreach (var item in result[1].ToList())
            {
                total2 = Total(item, type.Trim(), total2);
                count2++;
            }
            double tb2 = total2 / count2;
            foreach (var item in result[2].ToList())
            {
                total3 = Total(item, type.Trim(), total3);
                count3++;
            }
            double tb3 = total3 / count3;
            ReportType reportType = reportTypeService.GetByCode(type).FirstOrDefault();
            Site site = sitesService.GetByDeviceId(deviceId).FirstOrDefault();
            ReportMonthlyModel model = new ReportMonthlyModel();
            model.Distance1 = tb1;
            model.Distance2 = tb2;
            model.Distance3 = tb3;
            model.Title = reportType == null ? "" : reportType.Title;
            model.Measure = reportType == null ? "" : reportType.Measure;
            model.Name = reportType == null ? "" : reportType.Description + " " + (site == null ? "" : site.Name);
            model.dayInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            return Json(new { model }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Hàm tính tổng số
        /// </summary>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private double Total(Models.Entity.Data data, string code, double input)
        {
            if (code == "RP0")
            {
                input += data.BTI;
            }
            else if (code == "RP1")
            {
                input += data.BHU;
            }
            else if (code == "RP2")
            {
                input += data.BAV;
            }
            else if (code == "RP3")
            {
                input += data.BAC;
            }
            else if (code == "RP4")
            {
                input += data.BAP;
            }
            else if (code == "RP5")
            {
                input += data.BAF;
            }
            else if (code == "RP6")
            {
                input += data.BWS;
            }
            return input;
        }
        /// <summary>
        /// Hàm lấy danh sách trạm theo id khu vực
        /// </summary>
        /// <param name="idArea"></param>
        /// <returns></returns>
        public JsonResult GetSite(int idArea)
        {
            List<Site> data = sitesService.GetByAreaId(idArea).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Hàm lấy danh sách trạm theo id khu vực
        /// </summary>
        /// <param name="idArea"></param>
        /// <returns></returns>
        public JsonResult LoadArea(int groupId)
        {
            List<Area> data = areasService.GetAreasByGroupId(groupId).ToList();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }
        private static double CheckValueReport(double? input, string code)
        {
            double result = 0;
            if (code.Trim() == "RP0" || code.Trim() == "RP1" || code.Trim() == "RP2" || code.Trim() == "RP6")
            {

                result = input.Value / 10 + input.Value % 10 ;
                    return result;              
            }
            else if (code.Trim() == "RP3" || code.Trim() == "RP4")
            {
                                      
                return input.Value;
            }
            else if (code.Trim() == "RP5")
            {

                result = input.Value / 1000;

                return result;
            }
            return result;

        }
        private static string GetTenHuongGio(double? huongGio)
        {
            string result = "Không xác định";
            if (huongGio==0)
            {
                result = "Hướng Bắc";
            }
            else if(huongGio==1)
            {

                result = "Hướng Đông Bắc";
            }
            else if (huongGio == 2)
            {

                result = "Hướng Đông";
            }
            else if (huongGio == 3)
            {

                result = "Hướng Đông Nam";
            }
            else if (huongGio == 4)
            {

                result = "Hướng Nam";
            }
            else if (huongGio == 5)
            {

                result = "Hướng Tây Nam";
            }
            else if (huongGio == 6)
            {

                result = "Hướng Tây";
            }
            else if (huongGio == 7)
            {

                result = "Hướng Tây Bắc";
            }
            return result;

        }
    }
}
