using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Repository;
using NLTD.EmployeePortal.LMS.Ux.AppHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace NLTD.EmployeePortal.LMS.Ux.Controllers
{
    public class AdminController : BaseController
    {
        public ActionResult Employee()
        {
            return View();
        }

        public ActionResult PublicHolidays()
        {
            return View();
        }

        public ActionResult MapTeam()
        {
            return View();
        }

        public ActionResult MyYearWiseLeaveSumary()
        {
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "My";
            YearwiseLeaveSummaryQueryModel qryMdl = new YearwiseLeaveSummaryQueryModel
            {
                OnlyReportedToMe = true
            };
            return View("ViewEmployeeWiseLeaveSummary", qryMdl);
        }

        public ActionResult TeamYearWiseLeaveSumary()
        {
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "Team";
            YearwiseLeaveSummaryQueryModel qryMdl = new YearwiseLeaveSummaryQueryModel
            {
                OnlyReportedToMe = true
            };
            return View("ViewEmployeeWiseLeaveSummary", qryMdl);
        }

        public ActionResult AdminYearWiseLeaveSumary()
        {
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "Admin";
            YearwiseLeaveSummaryQueryModel qryMdl = new YearwiseLeaveSummaryQueryModel
            {
                OnlyReportedToMe = true
            };
            return View("ViewEmployeeWiseLeaveSummary", qryMdl);
        }

        public ActionResult LoadYearwiseLeaveSummary(int Year, string reqUsr, Int64? paramUserId, bool OnlyReportedToMe)
        {
            IList<EmployeeWiseLeaveSummaryModel> LeaveRequests = null;

            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetEmployeeWiseLeaveSumary(UserId, Year, reqUsr, paramUserId, OnlyReportedToMe);
            }
            return PartialView("YearwiseSummaryPartial", LeaveRequests);
        }

        public ActionResult ExportYearSummaryToExcel(YearwiseLeaveSummaryQueryModel data, string RequestLevelPerson)
        {
            IList<EmployeeWiseLeaveSummaryModel> LeaveRequests = null;
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetEmployeeWiseLeaveSumary(UserId, data.Year, RequestLevelPerson, data.SearchUserID, data.OnlyReportedToMe);
            }
            List<EmployeeWiseLeaveSummaryModel> excelData = new List<EmployeeWiseLeaveSummaryModel>();
            excelData = LeaveRequests.ToList();
            if (excelData.Count > 0)
            {
                string[] columns = { "Emp Id", "Name", "Request Type", "Total Leaves", "Used Leaves", "Pending Approval", "Balance Leaves" };
                byte[] filecontent = ExcelExportHelper.ExportExcelYearSummary(excelData, "", false, columns);
                return File(filecontent, ExcelExportHelper.ExcelContentType, "LeaveBalanceReport_" + System.DateTime.Now + ".xlsx");
            }
            else
            {
                using (var Client = new LeaveClient())
                {
                    var result = Client.GetYearsFromLeaveBalance();
                    ViewBag.YearsInLeaveBal = result;
                }
                ViewBag.RequestLevelPerson = RequestLevelPerson;
                data.ErrorMsg = "Excel file is not generated as no data returned.";
                return View("ViewEmployeeWiseLeaveSummary", data);
            }
        }

        public ActionResult MyMonthWiseLeaves()
        {
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "My";

            YearwiseLeaveSummaryQueryModel qryMdl = new YearwiseLeaveSummaryQueryModel
            {
                OnlyReportedToMe = true
            };
            return View("MonthwiseLeaveCount", qryMdl);
        }

        public ActionResult TeamMonthWiseLeaves()
        {
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "Team";

            YearwiseLeaveSummaryQueryModel qryMdl = new YearwiseLeaveSummaryQueryModel
            {
                OnlyReportedToMe = true
            };
            return View("MonthwiseLeaveCount", qryMdl);
        }

        public ActionResult AdminMonthWiseLeaves()
        {
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "Admin";

            YearwiseLeaveSummaryQueryModel qryMdl = new YearwiseLeaveSummaryQueryModel
            {
                OnlyReportedToMe = true
            };
            return View("MonthwiseLeaveCount", qryMdl);
        }

        public ActionResult LoadMonthWiseLeaveCount(int Year, string reqUsr, Int64? paramUserId, bool OnlyReportedToMe)
        {
            IList<MonthwiseLeavesCountModel> LeaveRequests = null;

            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetMonthwiseLeavesCount(Year, UserId, OnlyReportedToMe, paramUserId, reqUsr);
            }
            return PartialView("MonthwiseLeaveCountPartial", LeaveRequests);
        }

        public ActionResult ExportMonthCountToExcel(YearwiseLeaveSummaryQueryModel data, string RequestLevelPerson)
        {
            IList<MonthwiseLeavesCountModel> LeaveRequests = null;
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetMonthwiseLeavesCount(data.Year, UserId, data.OnlyReportedToMe, data.SearchUserID, RequestLevelPerson);
            }
            List<MonthwiseLeavesCountModel> excelData = new List<MonthwiseLeavesCountModel>();
            excelData = LeaveRequests.ToList();
            if (excelData.Count > 0)
            {
                string[] columns = { "EmpId", "Name", "CL1", "PL1", "DL1", "LWP1", "CO1", "CL2", "PL2", "DL2", "LWP2", "CO2", "CL3", "PL3", "DL3", "LWP3", "CO3", "CL4", "PL4", "DL4", "LWP4", "CO4", "CL5", "PL5", "DL5", "LWP5", "CO5", "CL6", "PL6", "DL6", "LWP6", "CO6", "CL7", "PL7", "DL7", "LWP7", "CO7", "CL8", "PL8", "DL8", "LWP8", "CO8", "CL9", "PL9", "DL9", "LWP9", "CO9", "CL10", "PL10", "DL10", "LWP10", "CO10", "CL11", "PL11", "DL11", "LWP11", "CO11", "CL12", "PL12", "DL12", "LWP12", "CO12" };
                byte[] filecontent = ExcelExportHelper.ExportExcelMonthSummary(excelData, data.Year, "", false, columns);
                return File(filecontent, ExcelExportHelper.ExcelContentType, "MonthwiseReport_" + System.DateTime.Now + ".xlsx");
            }
            else
            {
                using (var Client = new LeaveClient())
                {
                    var result = Client.GetYearsFromLeaveBalance();
                    ViewBag.YearsInLeaveBal = result;
                }
                ViewBag.RequestLevelPerson = RequestLevelPerson;
                data.ErrorMsg = "Excel file is not generated as no data returned.";
                return View("MonthwiseLeaveCount", data);
            }
        }

        public ActionResult MyDaywiseLeaves()
        {
            DaywiseLeaveQueryModel mdl = new DaywiseLeaveQueryModel();
            ViewBag.RequestLevelPerson = "My";
            mdl.OnlyReportedToMe = true;
            mdl.DonotShowRejected = true;
            return View("DaywiseLeaveDateRangeView", mdl);
        }

        public ActionResult TeamDaywiseLeaves()
        {
            DaywiseLeaveQueryModel mdl = new DaywiseLeaveQueryModel();
            ViewBag.RequestLevelPerson = "Team";
            mdl.OnlyReportedToMe = true;
            mdl.DonotShowRejected = true;
            return View("DaywiseLeaveDateRangeView", mdl);
        }

        public ActionResult AdminDaywiseLeaves()
        {
            DaywiseLeaveQueryModel mdl = new DaywiseLeaveQueryModel();
            ViewBag.RequestLevelPerson = "Admin";
            mdl.OnlyReportedToMe = true;
            mdl.DonotShowRejected = true;
            return View("DaywiseLeaveDateRangeView", mdl);
        }

        public ActionResult LoadDaywiseLeaves(Int64? paramUserId, string FromDate, string ToDate, bool IsLeaveOnly, bool OnlyReportedToMe, string reqUsr, bool DonotShowRejected)
        {
            DateTime? startDateFormatted = null;
            DateTime? endDateFormatted = null;

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
            IList<DaywiseLeaveDtlModel> LeaveRequests = null;
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetDaywiseLeaveDtl(startDateFormatted, endDateFormatted, IsLeaveOnly, UserId, OnlyReportedToMe, paramUserId, reqUsr, DonotShowRejected);
            }
            for (int i = 0; i < LeaveRequests.Count; i++)
            {
                if (LeaveRequests[i].LeaveReason != null)
                {
                    if (LeaveRequests[i].LeaveReason.Length > 12)
                    {
                        LeaveRequests[i].ReasonShort = LeaveRequests[i].LeaveReason.Substring(0, 12) + "...";
                    }
                    else
                    {
                        LeaveRequests[i].ReasonShort = LeaveRequests[i].LeaveReason;
                    }
                }
                if (LeaveRequests[i].ApproverComments != null)
                {
                    if (LeaveRequests[i].ApproverComments.Length > 12)
                    {
                        LeaveRequests[i].CommentsShort = LeaveRequests[i].ApproverComments.Substring(0, 12) + "...";
                    }
                    else
                    {
                        LeaveRequests[i].CommentsShort = LeaveRequests[i].ApproverComments;
                    }
                }
            }
            return PartialView("DaywiseLeaveDateRangeDtlPartial", LeaveRequests);
        }

        public ActionResult ExportToExcel(DaywiseLeaveQueryModel data, string RequestLevelPerson)
        {
            IList<DaywiseLeaveDtlModel> LeaveRequests = null;
            string startDate = data.DateRange.Substring(0, 10);
            string endDate = data.DateRange.Substring(12);
            DateTime? startDateFormatted = null;
            DateTime? endDateFormatted = null;
            if (startDate != "")
            {
                try
                {
                    startDateFormatted = DateTime.Parse(startDate, new CultureInfo("en-GB", true));
                    endDateFormatted = DateTime.Parse(endDate, new CultureInfo("en-GB", true));
                }
                catch (Exception)
                {
                    throw;
                }
            }
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetDaywiseLeaveDtl(startDateFormatted, endDateFormatted, data.IsLeaveOnly, UserId, data.OnlyReportedToMe, data.SearchUserID, RequestLevelPerson, data.DonotShowRejected);
            }
            List<DaywiseLeaveDtlModel> excelData = new List<DaywiseLeaveDtlModel>();
            excelData = LeaveRequests.ToList();
            if (excelData.Count > 0)
            {
                string[] columns = { "Emp Id", "Name", "Request Type", "Leave Balance", "Request Date", "Part Of Day", "Duration", "Status", "Reason", "Approver Comments" };
                byte[] filecontent = ExcelExportHelper.ExportExcel(excelData, "", false, columns);
                return File(filecontent, ExcelExportHelper.ExcelContentType, "DaywiseReport_" + System.DateTime.Now + ".xlsx");
            }
            else
            {
                ViewBag.RequestLevelPerson = RequestLevelPerson;
                data.ErrorMsg = "Excel file is not generated as no data returned.";
                return View("DaywiseLeaveDateRangeView", data);
            }
        }

        public ActionResult MyPermissions()
        {
            PermissionQueryModel mdl = new PermissionQueryModel();
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "My";
            mdl.OnlyReportedToMe = true;
            return View("DatewisePermissions", mdl);
        }

        public ActionResult TeamPermissions()
        {
            PermissionQueryModel mdl = new PermissionQueryModel();
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "Team";
            mdl.OnlyReportedToMe = true;
            return View("DatewisePermissions", mdl);
        }

        public ActionResult AdminPermissions()
        {
            PermissionQueryModel mdl = new PermissionQueryModel();
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "Admin";
            mdl.OnlyReportedToMe = true;
            return View("DatewisePermissions", mdl);
        }

        public ActionResult GetPermissionDetail(Int64? paramUserId, string reqUsr, string startDate, string endDate, bool OnlyReportedToMe)
        {
            IList<PermissionDetailsModel> LeaveRequests = null;
            DateTime? startDateFormatted = null;
            DateTime? endDateFormatted = null;

            if (startDate != "")
            {
                try
                {
                    startDateFormatted = DateTime.Parse(startDate, new CultureInfo("en-GB", true));
                    endDateFormatted = DateTime.Parse(endDate, new CultureInfo("en-GB", true));
                }
                catch (Exception)
                {
                    throw;
                }
            }
            if (startDate == "" || endDate == "")
            {
                startDateFormatted = System.DateTime.Now.Date;
                endDateFormatted = System.DateTime.Now.Date;
            }
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetPermissionDetail(paramUserId, reqUsr, startDateFormatted, endDateFormatted, OnlyReportedToMe, UserId);
            }
            for (int i = 0; i < LeaveRequests.Count; i++)
            {
                if (LeaveRequests[i].Reason != null)
                {
                    if (LeaveRequests[i].Reason.Length > 12)
                    {
                        LeaveRequests[i].ReasonShort = LeaveRequests[i].Reason.Substring(0, 12) + "...";
                    }
                    else
                    {
                        LeaveRequests[i].ReasonShort = LeaveRequests[i].Reason;
                    }
                }
                if (LeaveRequests[i].ApproverComments != null)
                {
                    if (LeaveRequests[i].ApproverComments.Length > 12)
                    {
                        LeaveRequests[i].CommentsShort = LeaveRequests[i].ApproverComments.Substring(0, 12) + "...";
                    }
                    else
                    {
                        LeaveRequests[i].CommentsShort = LeaveRequests[i].ApproverComments;
                    }
                }
                if (LeaveRequests[i].PermissionType.Contains('-'))
                {
                    string[] strPermissionType = LeaveRequests[i].PermissionType.Split('-');
                    LeaveRequests[i].PermissionType = strPermissionType[1].ToString().Trim();
                }
            }
            return PartialView("ViewPermissionDetailPartial", LeaveRequests);
        }

        public ActionResult ExportPermissionsExcel(PermissionQueryModel qryMdl, string RequestLevelPerson)
        {
            IList<PermissionDetailsModel> LeaveRequests = null;

            DateTime? startDateFormatted = null;
            DateTime? endDateFormatted = null;
            string startDate = qryMdl.DateRange.Substring(0, 10);
            string endDate = qryMdl.DateRange.Substring(12);
            if (startDate != "")
            {
                try
                {
                    startDateFormatted = DateTime.Parse(startDate, new CultureInfo("en-GB", true));
                    endDateFormatted = DateTime.Parse(endDate, new CultureInfo("en-GB", true));
                }
                catch (Exception)
                {
                    throw;
                }
            }
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetPermissionDetail(qryMdl.SearchUserID, RequestLevelPerson, startDateFormatted, endDateFormatted, qryMdl.OnlyReportedToMe, UserId);
            }
            List<PermissionDetailsModel> excelData = new List<PermissionDetailsModel>();
            excelData = LeaveRequests.ToList();
            if (excelData.Count > 0)
            {
                string[] columns = { "EmpId", "Name", "Month", "PermissionType", "PermissionDate", "TimeFrom", "TimeTo", "Duration", "Reason", "Status", "ApproverComments" };
                byte[] filecontent = ExcelExportHelper.ExportPermissionsExcel(excelData, "", false, columns);
                return File(filecontent, ExcelExportHelper.ExcelContentType, "PermissionsReport_" + System.DateTime.Now + ".xlsx");
            }
            else
            {
                ViewBag.RequestLevelPerson = RequestLevelPerson;
                qryMdl.ErrorMsg = "Excel file is not generated as no data returned.";
                return View("DatewisePermissions", qryMdl);
            }
        }

        public ActionResult GetEmployeeList(string term)
        {
            IList<EmployeeList> mdl;
            using (var Client = new LeaveClient())
            {
                mdl = Client.GetEmployeeList(term, UserId);
            }
            return Json(mdl);
        }

        public ActionResult ViewTransactionLog()
        {
            ViewBag.RequestLevelPerson = "Admin";
            ManageTeamLeavesQueryModel qyMdl = new ManageTeamLeavesQueryModel
            {
                OnlyReportedToMe = true
            };
            return View("SearchTransactionLog", qyMdl);
        }

        public ActionResult MyTransactionLogs()
        {
            ViewBag.RequestLevelPerson = "My";
            ManageTeamLeavesQueryModel qyMdl = new ManageTeamLeavesQueryModel
            {
                OnlyReportedToMe = true
            };
            return View("SearchTransactionLog", qyMdl);
        }

        public ActionResult TeamTransactionLogs()
        {
            ViewBag.RequestLevelPerson = "Team";
            ManageTeamLeavesQueryModel qyMdl = new ManageTeamLeavesQueryModel
            {
                OnlyReportedToMe = true
            };
            return View("SearchTransactionLog", qyMdl);
        }

        public ActionResult GetTransactionLog(string OnlyReportedToMe, string Name, string RequestMenuUser)
        {
            IList<LeaveTransactionDetail> transactionHistory = null;

            if (Name != "")
            {
                Name = Name.Replace("|", " ");
            }

            using (var client = new LeaveTransactionHistoryClient())
            {
                long Userid = this.UserId;
                transactionHistory = client.GetTransactionLog(Name, RequestMenuUser, Userid);
            }

            return PartialView("ViewTransactionLogPartial", transactionHistory);
        }

        public ActionResult LoadEmployeeAttendance(string ID, string FromDate, string ToDate, string requestLevelPerson, bool myDirectEmployees)
        {
            IList<EmployeeAttendanceModel> employeeAttendanceModelList = GetEmployeeAttendanceList(ID, FromDate, ToDate, requestLevelPerson, myDirectEmployees, true);
            ViewBag.RequestLevelPerson = requestLevelPerson;
            return PartialView("EmployeeAttendanceDtlPartial", employeeAttendanceModelList);
        }

        public ActionResult LoadAccesCardEmployeeAttendance(string ID, string FromDate, string ToDate, string requestLevelPerson)
        {
            IList<EmployeeAttendanceModel> employeeAttendanceModelList = GetAccessCardEmployeeAttendanceList(ID, FromDate, ToDate, requestLevelPerson);
            ViewBag.RequestLevelPerson = requestLevelPerson;
            return PartialView("EmployeeAcessCardAttendanceDtlPartial", employeeAttendanceModelList);
        }

        private IList<EmployeeAttendanceModel> GetAccessCardEmployeeAttendanceList(string ID, string FromDate, string ToDate, string requestLevelPerson)
        {
            IList<EmployeeAttendanceModel> employeeAttendanceModelList = null;
            DateTime startDateFormatted, endDateFormatted;
            string tempRequestLevelPerson = requestLevelPerson;
            EmployeeProfile profile = (EmployeeProfile)Session["Profile"];
            Int64 userID = profile.UserId;
            if (!string.IsNullOrEmpty(ID) && ID != "0")
            {
                userID = Convert.ToInt32(ID);
                requestLevelPerson = "My";// If we are not change the requst level person, If we pass any manager ID it will return all timesheet who are all under the manager
            }
            if (string.IsNullOrEmpty(FromDate) || FromDate == "Nodate")
            {
                startDateFormatted = DateTime.Now.AddDays(-30).Date;
                endDateFormatted = DateTime.Now;
            }
            else
            {
                startDateFormatted = DateTime.Parse(FromDate, new CultureInfo("en-GB", true));
                endDateFormatted = DateTime.Parse(ToDate, new CultureInfo("en-GB", true)).Add(new TimeSpan(23, 59, 59));
            }

            IEmployeeAttendanceHelper employeeAttendanceHelper = new EmployeeAttendanceClient();
            employeeAttendanceModelList = employeeAttendanceHelper.GetAccessCardAttendanceForRange(userID, startDateFormatted, endDateFormatted, requestLevelPerson);
            requestLevelPerson = tempRequestLevelPerson;
            return employeeAttendanceModelList;
        }

        private IList<EmployeeAttendanceModel> GetEmployeeAttendanceList(string ID, string FromDate, string ToDate, string requestLevelPerson, bool IsDirectEmployees, bool getBreakStatus)
        {
            IList<EmployeeAttendanceModel> employeeAttendanceModelList = null;
            DateTime startDateFormatted, endDateFormatted;
            string tempRequestLevelPerson = requestLevelPerson;
            EmployeeProfile profile = (EmployeeProfile)Session["Profile"];
            Int64 userID = profile.UserId;

            if (!string.IsNullOrEmpty(ID) && ID != "0")
            {
                userID = Convert.ToInt32(ID);
                requestLevelPerson = "My";// If we are not change the request level person, If we pass any manager ID it will return all timesheet who are all under the manager
            }

            if (string.IsNullOrEmpty(FromDate) || FromDate == "Nodate")
            {
                startDateFormatted = DateTime.Now.AddDays(-30).Date;
                endDateFormatted = DateTime.Now;
            }
            else
            {
                startDateFormatted = DateTime.Parse(FromDate, new CultureInfo("en-GB", true));
                endDateFormatted = DateTime.Parse(ToDate, new CultureInfo("en-GB", true)).Add(new TimeSpan(23, 59, 59));
            }

            IEmployeeAttendanceHelper employeeAttendanceHelper = new EmployeeAttendanceClient();
            employeeAttendanceModelList = employeeAttendanceHelper.GetAttendanceForRange(userID, startDateFormatted, endDateFormatted, requestLevelPerson, IsDirectEmployees, getBreakStatus);
            requestLevelPerson = tempRequestLevelPerson;
            return employeeAttendanceModelList;
        }

        private DateTime ChangeTime(DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }

        public List<TimeSheetModel> GetEmployeeTimeSheet(TimeSheetQueryModel TimeSheetQueryModelObj, out string errorMessage, string requestLevelPerson)
        {
            errorMessage = string.Empty;
            ITimesheetHelper EmployeeAttendanceHelperObj = new TimesheetClient();
            DateTime currentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (string.IsNullOrEmpty(TimeSheetQueryModelObj.Name))
            {
                EmployeeProfile profile = (EmployeeProfile)Session["Profile"];
                TimeSheetQueryModelObj.UserID = profile.UserId;
                if (TimeSheetQueryModelObj.FromDate == DateTime.MinValue)
                {
                    // For Last 30 days Attendance
                    TimeSheetQueryModelObj.FromDate = currentDate.AddDays(-30);
                    TimeSheetQueryModelObj.ToDate = currentDate;
                }
                else
                {
                    TimeSheetQueryModelObj.ToDate = TimeSheetQueryModelObj.ToDate;
                }
            }
            else if (!string.IsNullOrEmpty(TimeSheetQueryModelObj.Name))
            {
                if (TimeSheetQueryModelObj.FromDate == DateTime.MinValue)
                {
                    TimeSheetQueryModelObj.FromDate = currentDate.AddDays(-30);
                    TimeSheetQueryModelObj.ToDate = currentDate;
                }
                else
                {
                    TimeSheetQueryModelObj.ToDate = TimeSheetQueryModelObj.ToDate;
                }
                requestLevelPerson = "My";
            }
            if (TimeSheetQueryModelObj.ToDate >= DateTime.Now)
            {
                TimeSheetQueryModelObj.ToDate = currentDate;
            }
            List<TimeSheetModel> timeSheetModelList = new List<TimeSheetModel>();
            if (requestLevelPerson == "My")
            {
                timeSheetModelList = EmployeeAttendanceHelperObj.GetMyTimeSheet(TimeSheetQueryModelObj.UserID, TimeSheetQueryModelObj.FromDate, TimeSheetQueryModelObj.ToDate);
            }
            else
            {
                timeSheetModelList = EmployeeAttendanceHelperObj.GetMyTeamTimeSheet(TimeSheetQueryModelObj.UserID, TimeSheetQueryModelObj.FromDate, TimeSheetQueryModelObj.ToDate, TimeSheetQueryModelObj.MyDirectEmployees);
            }

            return timeSheetModelList;
        }

        public ActionResult LoadMyTimesheet(TimeSheetQueryModel TimeSheetQueryModelObj)
        {
            string errorMessage = string.Empty;
            List<TimeSheetModel> timeSheetModelList = GetEmployeeTimeSheet(TimeSheetQueryModelObj, out errorMessage, "My");
            ViewBag.RequestLevelPerson = "My";
            return PartialView("TimeSheetPartial", timeSheetModelList);
        }

        public ActionResult LoadMyTeamTimesheet(TimeSheetQueryModel TimeSheetQueryModelObj)
        {
            string errorMessage = string.Empty;
            TimesheetClient tsClient = new TimesheetClient();
            List<TimeSheetModel> timeSheetModelList = new List<TimeSheetModel>();
            if (tsClient.IsUserHR(TimeSheetQueryModelObj.UserID) == false)
            {
                timeSheetModelList = GetEmployeeTimeSheet(TimeSheetQueryModelObj, out errorMessage, "Team");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return new HttpStatusCodeResult(400, "Given Username does not exists");
                }
            }
            ViewBag.RequestLevelPerson = "Team";
            return PartialView("TimeSheetPartial", timeSheetModelList);
        }

        public ActionResult ExportTimeSheetToExcel(TimeSheetQueryModel TimeSheetQueryModelObj, string RequestLevelPerson)
        {
            string errorMessage = string.Empty;
            string startDate = TimeSheetQueryModelObj.DateRange.Substring(0, 10);
            string endDate = TimeSheetQueryModelObj.DateRange.Substring(12);
            //TimeSheetQueryModelObj.FromDate = startDate;
            if (startDate != "")
            {
                try
                {
                    TimeSheetQueryModelObj.FromDate = DateTime.Parse(startDate, new CultureInfo("en-GB", true));
                    TimeSheetQueryModelObj.ToDate = DateTime.Parse(endDate, new CultureInfo("en-GB", true));
                }
                catch (Exception)
                {
                    throw;
                }
            }

            List<TimeSheetModel> timeSheetModelList = GetEmployeeTimeSheet(TimeSheetQueryModelObj, out errorMessage, RequestLevelPerson);
            List<TimeSheetModel> excelData = timeSheetModelList.ToList();
            if (excelData.Count > 0)
            {
                List<string> columns = new List<string>() { "Date", "Shift", "In Time", "Out Time", "Working Hours", "Status", "Requests", "Day", "Late In", "Early Out", "Name", "Reporting Manager" };
                string fileName = string.Format("Timesheet_{0}{1}", DateTime.Now.ToString("ddMMyyyyHHmmss"), ".xlsx");

                //if (RequestLevelPerson == "My")
                //{
                //    EmployeeProfile profile = (EmployeeProfile)Session["Profile"];
                //    fileName = string.Format("{0}{1}{2}{3}", profile.FirstName, profile.LastName, DateTime.Now.ToString("ddMMyyyyHHmmss"), ".xlsx");
                //}
                //else
                //{
                //fileName = string.Format("TimeSheet_{0}{1}", DateTime.Now.ToString("ddMMyyyyHHmmss"), ".xlsx");
                //}
                byte[] filecontent = ExcelExportHelper.ExportTimesheetExcel(excelData, "", true, columns.ToArray());
                return File(filecontent, ExcelExportHelper.ExcelContentType, fileName);
            }
            else
            {
                using (var Client = new LeaveClient())
                {
                    var result = Client.GetYearsFromLeaveBalance();
                    ViewBag.YearsInLeaveBal = result;
                }
                ViewBag.RequestLevelPerson = RequestLevelPerson;
                EmployeeAttendanceQueryModel data = new EmployeeAttendanceQueryModel
                {
                    ErrorMsg = "Excel file is not generated as no data returned."
                };
                return View("~/Views/Attendance/TimeSheet.cshtml", data);
            }
        }

        public ActionResult ExportAttendanceToExcel(EmployeeAttendanceQueryModel EmployeeAttendanceQueryModelObj, string RequestLevelPerson)
        {
            List<EmployeeAttendanceModel> excelData = GetEmployeeAttendanceList(EmployeeAttendanceQueryModelObj.UserID.ToString(), (EmployeeAttendanceQueryModelObj.FromDate == DateTime.MinValue ? "" : EmployeeAttendanceQueryModelObj.FromDate.ToString()),
                (EmployeeAttendanceQueryModelObj.ToDate == DateTime.MinValue ? "" : EmployeeAttendanceQueryModelObj.ToDate.ToString()), RequestLevelPerson, EmployeeAttendanceQueryModelObj.DirectEmployees, true).ToList();

            if (excelData.Count > 0)
            {
                List<string> columns = new List<string>() { "AttendanceDate", "InOutTime", "InOut", "Name", "BreakDuration" };
                // string fileName = "Attendance.xlsx";
                string fileName = string.Format("Attendance_{0}{1}", DateTime.Now.ToString("ddMMyyyyHHmmss"), ".xlsx");
                if (EmployeeAttendanceQueryModelObj.RequestLevelPerson == "My")
                {
                    columns = new List<string>() { "AttendanceDate", "InOutTime", "InOut", "BreakDuration" };
                }

                byte[] filecontent = ExcelExportHelper.ExportExcelAttendance(excelData, "", false, columns.ToArray());
                return File(filecontent, ExcelExportHelper.ExcelContentType, fileName);
            }
            else
            {
                ViewBag.RequestLevelPerson = RequestLevelPerson;
                EmployeeAttendanceQueryModel data = new EmployeeAttendanceQueryModel
                {
                    ErrorMsg = "Excel file is not generated as no data returned."
                };
                return View("~/Views/Attendance/MyAttendance.cshtml", data);
            }
        }

        public ActionResult ExportAccessCardAttendanceToExcel(EmployeeAttendanceQueryModel EmployeeAttendanceQueryModelObj, string RequestLevelPerson)
        {
            List<EmployeeAttendanceModel> excelData = GetAccessCardEmployeeAttendanceList(EmployeeAttendanceQueryModelObj.CardID.ToString(), (EmployeeAttendanceQueryModelObj.FromDate == DateTime.MinValue ? "" : EmployeeAttendanceQueryModelObj.FromDate.ToString()),
                (EmployeeAttendanceQueryModelObj.ToDate == DateTime.MinValue ? "" : EmployeeAttendanceQueryModelObj.ToDate.ToString()), RequestLevelPerson).ToList();

            if (excelData.Count > 0)
            {
                List<string> columns = new List<string>() { "Card Id", "Date", "In Out Time", "In Out", "Name" };
                string fileName = string.Format("AccessCardReport_{0}{1}", DateTime.Now.ToString("ddMMyyyyHHmmss"), ".xlsx");
                if (EmployeeAttendanceQueryModelObj.RequestLevelPerson == "My")
                {
                    columns = new List<string>() { "Card Id", "Date", "In Out Time", "In Out", "Name" };
                }

                byte[] filecontent = ExcelExportHelper.ExportExcelAccessCardAttendance(excelData, "", false, columns.ToArray());
                return File(filecontent, ExcelExportHelper.ExcelContentType, fileName);
            }
            else
            {
                ViewBag.RequestLevelPerson = RequestLevelPerson;
                EmployeeAttendanceQueryModel data = new EmployeeAttendanceQueryModel
                {
                    ErrorMsg = "Excel file is not generated as no data returned."
                };
                return View("~/Views/Attendance/AccessCardAttendance.cshtml", data);
            }
        }

        public ActionResult MyOverTimePermissions()
        {
            PermissionQueryModel mdl = new PermissionQueryModel();
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "My";
            mdl.OnlyReportedToMe = true;
            return View("OverTimePermissions", mdl);
        }

        public ActionResult TeamOverTimePermissions()
        {
            PermissionQueryModel mdl = new PermissionQueryModel();
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "Team";
            mdl.OnlyReportedToMe = true;
            return View("OverTimePermissions", mdl);
        }

        public ActionResult AdminOverTimePermissions()
        {
            PermissionQueryModel mdl = new PermissionQueryModel();
            using (var Client = new LeaveClient())
            {
                var result = Client.GetYearsFromLeaveBalance();
                ViewBag.YearsInLeaveBal = result;
            }
            ViewBag.RequestLevelPerson = "Admin";
            mdl.OnlyReportedToMe = true;
            return View("OverTimePermissions", mdl);
        }

        public ActionResult GetOverTimePermissionDetail(Int64? paramUserId, string reqUsr, string startDate, string endDate, bool OnlyReportedToMe)
        {
            IList<PermissionDetailsModel> LeaveRequests = null;
            DateTime? startDateFormatted = null;
            DateTime? endDateFormatted = null;

            if (startDate != "")
            {
                try
                {
                    startDateFormatted = DateTime.Parse(startDate, new CultureInfo("en-GB", true));
                    endDateFormatted = DateTime.Parse(endDate, new CultureInfo("en-GB", true));
                }
                catch (Exception)
                {
                    throw;
                }
            }
            if (startDate == "" || endDate == "")
            {
                startDateFormatted = System.DateTime.Now.Date;
                endDateFormatted = System.DateTime.Now.Date;
            }
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetOverTimePermissionDetail(paramUserId, reqUsr, startDateFormatted, endDateFormatted, OnlyReportedToMe, UserId);
            }
            for (int i = 0; i < LeaveRequests.Count; i++)
            {
                if (LeaveRequests[i].Reason != null)
                {
                    if (LeaveRequests[i].Reason.Length > 12)
                    {
                        LeaveRequests[i].ReasonShort = LeaveRequests[i].Reason.Substring(0, 12) + "...";
                    }
                    else
                    {
                        LeaveRequests[i].ReasonShort = LeaveRequests[i].Reason;
                    }
                }
                if (LeaveRequests[i].ApproverComments != null)
                {
                    if (LeaveRequests[i].ApproverComments.Length > 12)
                    {
                        LeaveRequests[i].CommentsShort = LeaveRequests[i].ApproverComments.Substring(0, 12) + "...";
                    }
                    else
                    {
                        LeaveRequests[i].CommentsShort = LeaveRequests[i].ApproverComments;
                    }
                }
            }
            return PartialView("ViewOverTimePermissionDetailPartial", LeaveRequests);
        }

        public ActionResult ExportOverTimePermissionsExcel(PermissionQueryModel qryMdl, string RequestLevelPerson)
        {
            IList<PermissionDetailsModel> LeaveRequests = null;

            DateTime? startDateFormatted = null;
            DateTime? endDateFormatted = null;
            string startDate = qryMdl.DateRange.Substring(0, 10);
            string endDate = qryMdl.DateRange.Substring(12);
            if (startDate != "")
            {
                try
                {
                    startDateFormatted = DateTime.Parse(startDate, new CultureInfo("en-GB", true));
                    endDateFormatted = DateTime.Parse(endDate, new CultureInfo("en-GB", true));
                }
                catch (Exception)
                {
                    throw;
                }
            }
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetOverTimePermissionDetail(qryMdl.SearchUserID, RequestLevelPerson, startDateFormatted, endDateFormatted, qryMdl.OnlyReportedToMe, UserId);
            }
            List<PermissionDetailsModel> excelData = new List<PermissionDetailsModel>();
            excelData = LeaveRequests.ToList();
            if (excelData.Count > 0)
            {
                string[] columns = { "Emp Id", "Name", "Month", "Over Time Date", "Time From", "Time To", "Duration", "Reason", "Status", "Approver Comments", "Reporting Manager" };
                byte[] filecontent = ExcelExportHelper.ExportOverTimeExcel(excelData, "", false, columns);
                return File(filecontent, ExcelExportHelper.ExcelContentType, "OverTimeReport_" + System.DateTime.Now + ".xlsx");
            }
            else
            {
                ViewBag.RequestLevelPerson = RequestLevelPerson;

                qryMdl.ErrorMsg = "Excel file is not generated as no data returned.";
                return View("OverTimePermissions", qryMdl);
            }
        }
    }
}