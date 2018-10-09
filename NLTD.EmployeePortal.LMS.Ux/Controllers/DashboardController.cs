using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Ux.AppHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;

namespace NLTD.EmployeePortal.LMS.Ux.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class DashboardController : BaseController
    {
        public ActionResult Index()
        {
            DashBoardModel dbMdl = new DashBoardModel();

            using (var client = new LeaveClient())
            {
                dbMdl = client.GetDashboardData(UserId, OfficeId);
            }
            dbMdl.IsLMSApprover = IsLMSApprover;
            dbMdl.UserRole = Role;
            ViewBag.OfficeAttendance = dbMdl.EmployeeCount;

            ViewBag.PreviousYear = dbMdl.PreviousYear ? "" : "disabled";
            ViewBag.NextYear = dbMdl.NextYear ? "" : "disabled";
            if (dbMdl.lstHolidayModel.Count > 0)
            {
                ViewBag.HolidayOfficeName = dbMdl.lstHolidayModel[0].HolidayOfficeName;
                ViewBag.HolidayYear = dbMdl.lstHolidayModel[0].HolidayDate.Year;
            }

            return View(dbMdl);
        }

        public ActionResult LoadPendingCount()
        {
            int count = 0;
            using (var client = new LeaveClient())
            {
                count = client.GetPendingApprovalCount(UserId);
            }
            return PartialView("PendingApprovalCountPartial", count);
        }

        public ActionResult LoadTeamStatus()
        {
            List<TimeSheetModel> timeSheetModelList;
            using (var client = new LeaveClient())
            {
                timeSheetModelList = client.GetMyTeamTimeSheet(UserId);
            }
            return PartialView("DashboardAttendancePartial", timeSheetModelList);
        }

        public ActionResult ListHolidayByYear(string year)
        {
            int holidayYear = Convert.ToInt32(year);
            IList<HolidayModel> holidayModelList;
            bool previousYear = false;
            bool nextYear = false;
            using (var client = new LeaveClient())
            {
                holidayModelList = client.GetHolidaysDetails(UserId, holidayYear, ref previousYear, ref nextYear);
            }
            ViewBag.PreviousYear = previousYear ? "" : "disabled";
            ViewBag.NextYear = nextYear ? "" : "disabled";
            if (holidayModelList.Count > 0)
            {
                ViewBag.HolidayOfficeName = holidayModelList[0].HolidayOfficeName;
                ViewBag.HolidayYear = holidayModelList[0].HolidayDate.Year;
            }
            return PartialView("~/Views/Dashboard/HolidayListPartial.cshtml", holidayModelList);
        }
    }
}