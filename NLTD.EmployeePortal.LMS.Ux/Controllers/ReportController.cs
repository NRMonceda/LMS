using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Repository;
using NLTD.EmployeePortal.LMS.Ux.AppHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

namespace NLTD.EmployeePortal.LMS.Ux.Controllers
{
    public class ReportController : BaseController
    {
        // GET: TimeSheet

        public ActionResult ShiftBasedLateAndEarlyRpt()
        {
            DaywiseLeaveQueryModel mdl = new DaywiseLeaveQueryModel();
            ViewBag.RequestLevelPerson = "Admin";
            mdl.OnlyReportedToMe = true;
            mdl.DonotShowRejected = true;
            return View("ShiftBasedLateAndEarlyRpt", mdl);
        }

        public ActionResult loadLateAndEarlyRpt(string Name, string FromDate, string ToDate, bool IsLeaveOnly, bool OnlyReportedToMe, string reqUsr, bool DonotShowRejected)
        {
            List<TimeSheetModel> lateAndEarlyRpt = null;

            if (Name != "")
            {
                Name = Name.Replace("|", " ");
            }
            DateTime startDateFormatted = DateTime.Now;
            DateTime endDateFormatted = DateTime.Now;

            if (FromDate != null)
            {
                if (FromDate.Trim() != "")
                {
                    startDateFormatted = DateTime.ParseExact(FromDate, "d-M-yyyy", CultureInfo.InvariantCulture);
                    endDateFormatted = DateTime.ParseExact(ToDate, "d-M-yyyy", CultureInfo.InvariantCulture);
                }
            }

            if (FromDate == "" || ToDate == "")
            {
                startDateFormatted = System.DateTime.Now.Date;
                endDateFormatted = System.DateTime.Now.Date;
            }

            ITimesheetHelper EmployeeAttendanceHelperObj = new TimesheetClient();
            if (!string.IsNullOrEmpty(Name))
            {
                IEmployeeHelper EmployeeHelper = new EmployeeClient();
                Int64 UserID = EmployeeHelper.GetUserId(Name);

                if (UserID > 0)
                {
                    lateAndEarlyRpt = EmployeeAttendanceHelperObj.GetMyTimeSheet(UserID, startDateFormatted, endDateFormatted);
                }
            }
            else
            {
                lateAndEarlyRpt = EmployeeAttendanceHelperObj.GetMyTeamTimeSheet(this.UserId, startDateFormatted, endDateFormatted, OnlyReportedToMe);
            }
            return PartialView("ShiftBasedLateAndEarlyRptPartial", lateAndEarlyRpt);
        }

        public ActionResult NoOfLateReportRpt()
        {
            DaywiseLeaveQueryModel mdl = new DaywiseLeaveQueryModel();
            ViewBag.RequestLevelPerson = "Admin";
            mdl.OnlyReportedToMe = true;
            mdl.DonotShowRejected = true;
            return View("NoOfLateReportRpt", mdl);
        }

        public ActionResult LoadNoOfLateReport(string Name, string FromDate, string ToDate, bool IsLeaveOnly, bool OnlyReportedToMe, string reqUsr, bool DonotShowRejected)
        {
            List<NoOfLateInMonth> noOfLateInMonth = null;

            if (Name != "")
            {
                Name = Name.Replace("|", " ");
            }
            DateTime startDateFormatted = DateTime.Now;
            DateTime endDateFormatted = DateTime.Now;

            if (FromDate != null)
            {
                if (FromDate.Trim() != "")
                {
                    startDateFormatted = DateTime.ParseExact(FromDate, "d-M-yyyy", CultureInfo.InvariantCulture);
                    endDateFormatted = DateTime.ParseExact(ToDate, "d-M-yyyy", CultureInfo.InvariantCulture);
                }
            }

            if (FromDate == "" || ToDate == "")
            {
                startDateFormatted = System.DateTime.Now.Date;
                endDateFormatted = System.DateTime.Now.Date;
            }

            ReportClient reportClient = new ReportClient();
            {
                noOfLateInMonth = reportClient.GetLateReport(this.UserId, startDateFormatted, endDateFormatted, OnlyReportedToMe);
            }
            return PartialView("NoOfLateReportRptPartial", noOfLateInMonth);
        }
    }
}