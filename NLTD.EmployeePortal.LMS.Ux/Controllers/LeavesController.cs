using Hangfire;
using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Ux.AppHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace NLTD.EmployeePortal.LMS.Ux.Controllers
{
    public class LeavesController : BaseController
    {
        public ActionResult Apply(DateTime? StartDate, DateTime? LeaveUpto)
        {
            ViewBag.PageTile = "Apply For Leave";

            var request = new LeaveRequestModel();
            request.UserId = this.UserId;

            if (StartDate != null)
            {
                request.LeaveFrom = StartDate.Value;
            }
            else
            {
                request.NumberOfDays = 1;
                request.LeaveFrom = System.DateTime.Now;
            }

            if (LeaveUpto != null)
            {
                request.LeaveUpto = LeaveUpto.Value;
            }
            else
            {
                request.LeaveUpto = System.DateTime.Now;
            }
            
            using (var Client = new LeaveClient())
            {
                var commonData = Client.ApplyLeaveCommonData(this.OfficeId, this.UserId);
                request.lstLeaveTypes = commonData.lstLeaveTypes;
                request.lstSummary = commonData.lstSummary;
                request.WeekOffs = commonData.WeekOffs;
                request.holidayDates = commonData.holidayDates;
                request.TimebasedLeaveTypeIds = commonData.TimebasedLeaveTypeIds;
            }

            request.ReportingToName = this.ReportingToName;

            return View(request);
        }

        public ActionResult LoadAppyOnBehalf(DateTime? StartDate, DateTime? LeaveUpto)
        {
            ViewBag.PageTile = "Apply For Leave Others";
            var request = new LeaveRequestModel();
            Int64 userId = (Int64)TempData["ApplyForUserId"];

            request.UserId = userId;
            if (StartDate != null)
            {
                request.LeaveFrom = StartDate.Value;
            }
            else
            {
                request.NumberOfDays = 1;
                request.LeaveFrom = System.DateTime.Now;
            }
            if (LeaveUpto != null)
                request.LeaveUpto = LeaveUpto.Value;
            else
                request.LeaveUpto = System.DateTime.Now;
            using (var Client = new LeaveClient())
            {
                var commonData = Client.ApplyLeaveCommonData(this.OfficeId, userId);
                request.lstLeaveTypes = commonData.lstLeaveTypes;
                request.lstSummary = commonData.lstSummary;
                request.WeekOffs = commonData.WeekOffs;
                request.holidayDates = commonData.holidayDates;
                request.TimebasedLeaveTypeIds = commonData.TimebasedLeaveTypeIds;
            }
            using (var Client = new EmployeeClient())
            {
                request.ReportingToName = Client.ReportingToName(userId);
            }

            request.ViewTitle = "Apply Request For " + TempData["ApplyForName"].ToString();
            request.ApplyMode = "Others";
            request.ApplyForUserId = userId;
            return View("ApplyFor", request);
        }

        public ActionResult ApplyOnBehalfSearch()
        {
            ApplyOnBehalfSearchModel qryMdl = new ApplyOnBehalfSearchModel();
            qryMdl.ApplyMode = "Other";

            return View("ApplyOnBehalfMainView", qryMdl);
        }

        public ActionResult CallApplyFor(string name)
        {
            Int64 userId = 0;
            if (name != "")
            {
                name = name.Replace("|", " ");
            }
            using (var Client = new EmployeeClient())
            {
                var data = Client.GetUserId(name);
                userId = data;
            }
            if (userId == 0)
            {
                return Json("InvalidName");
            }
            else
            {
                TempData["ApplyForName"] = name;
                TempData["ApplyForUserId"] = userId;
                return Json(new { redirectToUrl = Url.Action("LoadAppyOnBehalf", "Leaves") });
            }
        }

        [HttpPost]
        public ActionResult SaveLeaveRequest(LeaveRequestModel data)
        {
            bool isValid = true;
            Int64 userId = 0;
            if (ModelState.IsValid)
            {
                if (data.ApplyMode == "Others")
                {
                    if (this.Role == "HR" || this.Role == "Admin")
                    {
                    }
                    else
                    {
                        data.ErrorMesage = "You are not authorized to apply request on behalf of others.";
                        isValid = false;
                    }
                }

                if (data.ApplyMode == "Others")
                {
                    userId = data.ApplyForUserId;
                }
                else
                {
                    userId = this.UserId;
                }

                if (data.LeaveFrom > data.LeaveUpto)
                {
                    data.ErrorMesage = "Please select correct From and To dates for the request.";
                    isValid = false;
                }

                if (data.Reason != null)
                {
                    if (data.Reason.Trim() == string.Empty)
                    {
                        data.ErrorMesage = "Please provide the reason for this request.";
                        isValid = false;
                    }
                }
                else
                {
                    data.ErrorMesage = "Please provide the reason for this request.";
                    isValid = false;
                }

                //remove unnecessary data for half day leave
                //remove unnecessary data for permission
                //casual leave max dys get from DB

                if (data.IsTimeBased != "Time")
                {
                    if (isValid)
                    {
                        if (data.LeaveFrom.Year != data.LeaveUpto.Year)
                        {
                            data.ErrorMesage = "The leave request duration should be within the same year.";
                            isValid = false;
                        }
                        if (data.NumberOfDays <= 0)
                        {
                            data.ErrorMesage = "Please select correct From and To dates for the request.";
                            isValid = false;
                        }
                    }
                }
                else
                {
                    if (data.IsOverTime == "Yes")
                    {
                        data.LeaveFrom = data.PermissionDate;
                    }
                }
                if (data.LeaveFrom == data.LeaveUpto)
                {
                    if (data.LeaveFromTime == "F")
                    {
                        data.LeaveUptoTime = "F";
                    }
                    else if (data.LeaveFromTime == "S")
                    {
                        data.LeaveUptoTime = "S";
                    }
                }

                using (var client = new LeaveClient())
                {
                    data.UserId = userId;
                    data.AppliedByUserId = this.UserId;
                    if (isValid)
                    {
                        string result = client.SaveLeaveRequest(data);
                        if (result == null || result == string.Empty)
                        {
                            data.ErrorMesage = "Unhandled exception occurred.";
                        }
                        else
                        {
                            if (result.IndexOf("$") > 0)
                            {
                                data.ErrorMesage = "Saved";

                                try
                                {
                                    EmailHelper emailHelper = new EmailHelper();
                                    //#if DEBUG
                                    emailHelper.SendRequestEmail(Convert.ToInt64(result.Substring(6)), "Pending");
                                    //#else
                                    //BackgroundJob.Enqueue(() => emailHelper.SendEmail(Convert.ToInt64(result.Substring(6)), "Pending"));
                                    //#endif
                                }
                                catch
                                {
                                    data.ErrorMesage = "EmailFailed";
                                }
                            }
                            else if (result == "Duplicate")
                                data.ErrorMesage = "There is a previously applied request falling within this date range.";
                            else if (result.Contains("LeaveExceeded"))
                                data.ErrorMesage = "You cannot apply for days more than the available leaves. Number of leaves available are " + result.Substring(14) + ".";
                            else if (result == "Leave balance profile not created")
                                data.ErrorMesage = "Leave balance has not yet been credited for you.";
                            else if (result == "HolidayFromDate")
                                data.ErrorMesage = "Leave from date is a holiday. Please select working day.";
                            else if (result == "HolidayToDate")
                                data.ErrorMesage = "Leave upto date is a holiday. Please select working day.";
                            else if (result == "PermissionProperDateRange")
                                data.ErrorMesage = "Please select correct date.";
                            else if (result == "PermissionProperTime")
                                data.ErrorMesage = "Please select the correct time duration.";
                            else if (result == "PermissionDurationTime")
                                data.ErrorMesage = "Total permission time exceeds the duration allowed per month.";
                            else if (result == "PermissionDateTobeSame")
                                data.ErrorMesage = "The From and To dates should be same for this type of request.";
                            else if (result.Contains("ExceedMaxPerRequest"))
                                data.ErrorMesage = "Maximum number of days allowed per request are " + result.Substring(19) + ".";
                            else if (result == "MinDaysForCL")
                                data.ErrorMesage = "Casual Leave has to be applied 3 days prior to availing.";
                            else if (result == "MinDaysForSL")
                                data.ErrorMesage = "Sick/Unplanned Leave cannot be applied for future dates.";
                            else if (result == "MinDaysForEL")
                                data.ErrorMesage = "Earned Leave has to be applied 14 days prior to availing.";
                            else if (result.Contains("BelowMinPerRequest"))
                                data.ErrorMesage = "Minimum number of days allowed per request are 3.";
                            else if (result == "CheckException")
                                data.ErrorMesage = "Combining leaves is an exception. Please avail leave combination exception";
                            else if (result == "UnCheckException")
                                data.ErrorMesage = "No combination of leaves found. Please uncheck avail leave combination exception";
                            else if (result.Contains("MaxExceptionsAvailed:"))
                                data.ErrorMesage = "You have already availed " + result.Substring(21) + " leave exceptions. Please contact HR for your case.";

                        }
                    }
                }
            }
            else
            {
                data.ErrorMesage = "There is invalid data in request.";
            }
            using (var Client = new LeaveClient())
            {
                var commonData = Client.ApplyLeaveCommonData(this.OfficeId, userId);
                data.lstLeaveTypes = commonData.lstLeaveTypes;
                data.lstSummary = commonData.lstSummary;
                data.WeekOffs = commonData.WeekOffs;
                data.holidayDates = commonData.holidayDates;
                data.TimebasedLeaveTypeIds = commonData.TimebasedLeaveTypeIds;
            }
            using (var Client = new EmployeeClient())
            {
                data.ReportingToName = Client.ReportingToName(data.UserId);
            }
            if (data.ApplyMode == "Others")
            {
                return View("ApplyFor", data);
            }
            else
            {
                return Json(data.ErrorMesage);
            }
        }

        public ActionResult ManageLeaveRequest()
        {
            ManageTeamLeavesQueryModel qryMdl = new ManageTeamLeavesQueryModel();
            qryMdl.OnlyReportedToMe = true;
            qryMdl.IsAuthorized = this.IsAuthorized;
            return View(qryMdl);
        }

        public ActionResult MyLeaveHistory()
        {
            ViewBag.RequestLevelPerson = "My";
            ManageTeamLeavesQueryModel qyMdl = new ManageTeamLeavesQueryModel();
            qyMdl.OnlyReportedToMe = true;
            int year = DateTime.Now.Year;
            DateTime firstDay = new DateTime(year, 1, 1);
            DateTime lastDay = new DateTime(year, 12, 31);
            qyMdl.FromDate = firstDay;
            qyMdl.ToDate = lastDay;
            return View("TeamHistory", qyMdl);
        }

        public ActionResult TeamLeaveHistory()
        {
            ViewBag.RequestLevelPerson = "Team";
            ManageTeamLeavesQueryModel qyMdl = new ManageTeamLeavesQueryModel();
            qyMdl.OnlyReportedToMe = true;
            int year = DateTime.Now.Year;
            DateTime firstDay = new DateTime(year, 1, 1);
            DateTime lastDay = new DateTime(year, 12, 31);
            qyMdl.FromDate = firstDay;
            qyMdl.ToDate = lastDay;
            return View("TeamHistory", qyMdl);
        }

        public ActionResult AdminLeaveHistory()
        {
            ViewBag.RequestLevelPerson = "Admin";
            ManageTeamLeavesQueryModel qyMdl = new ManageTeamLeavesQueryModel();
            qyMdl.OnlyReportedToMe = true;
            int year = DateTime.Now.Year;
            DateTime firstDay = new DateTime(year, 1, 1);
            DateTime lastDay = new DateTime(year, 12, 31);
            qyMdl.FromDate = firstDay;
            qyMdl.ToDate = lastDay;
            return View("TeamHistory", qyMdl);
        }

        public ActionResult LoadManageLeavePartial(bool ShowOnlyReportedToMe, bool ShowApprovedLeaves, string FromDate, string ToDate, string RequestMenuUser)
        {
            DateTime? startDateFormatted = null;
            DateTime? endDateFormatted = null;
            if (FromDate != "" && ToDate != "")
            {
                try
                {
                    startDateFormatted = DateTime.Parse(FromDate, new CultureInfo("en-GB", true));
                    endDateFormatted = DateTime.Parse(ToDate, new CultureInfo("en-GB", true));
                }
                catch (Exception)
                {
                    throw;
                }
            }
            ManageTeamLeavesQueryModel qryMdl = new ManageTeamLeavesQueryModel();
            qryMdl.OnlyReportedToMe = ShowOnlyReportedToMe;
            qryMdl.ShowApprovedLeaves = ShowApprovedLeaves;
            qryMdl.FromDate = startDateFormatted;
            qryMdl.ToDate = endDateFormatted;
            qryMdl.RequestMenuUser = RequestMenuUser;
            IList<TeamLeaves> LeaveRequests = null;
            qryMdl.LeadId = UserId;
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetLeaveRequests(qryMdl);
            }
            return PartialView("ManageLeaveDetailPartial", LeaveRequests);
        }

        public ActionResult ViewLeaveHistory(bool OnlyReportedToMe, string FromDate, string ToDate, bool IsLeaveOnly, Int64? paramUserId, string RequestMenuUser)
        {
            DateTime? startDateFormatted = null;
            DateTime? endDateFormatted = null;

            if (FromDate != "")
            {
                try
                {
                    startDateFormatted = DateTime.Parse(FromDate, new CultureInfo("en-GB", true));
                    endDateFormatted = DateTime.Parse(ToDate, new CultureInfo("en-GB", true));
                }
                catch (Exception)
                {
                    throw;
                }
            }
            if (FromDate == "" || ToDate == "")
            {
                startDateFormatted = System.DateTime.Now.Date;
                endDateFormatted = System.DateTime.Now.Date;
            }
            ManageTeamLeavesQueryModel qryMdl = new ManageTeamLeavesQueryModel();
            qryMdl.OnlyReportedToMe = OnlyReportedToMe;
            qryMdl.FromDate = startDateFormatted;
            qryMdl.ToDate = endDateFormatted;
            qryMdl.RequestMenuUser = RequestMenuUser;
            qryMdl.IsLeaveOnly = IsLeaveOnly;
            qryMdl.SearchUserID = paramUserId;
            IList<TeamLeaves> LeaveRequests = null;
            qryMdl.LeadId = UserId;
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetTeamLeaveHistory(qryMdl);
            }
            return PartialView("TeamHistoryDetailPartial", LeaveRequests);
        }

        [HttpPost]
        public ActionResult ChangeStatus(LeaveStatusModel obj)
        {
            obj.UserId = UserId;
            using (var client = new LeaveClient())
            {
                string Status = client.ChangeStatus(obj);
                if (Status == "Saved")
                {
                    string action = string.Empty;
                    if (obj.Status == "R")
                        action = "Rejected";
                    else if (obj.Status == "A")
                        action = "Approved";
                    else if (obj.Status == "C")
                        action = "Cancelled";
                    try
                    {
                        EmailHelper emailHelper = new EmailHelper();
                        //#if DEBUG
                        emailHelper.SendRequestEmail(Convert.ToInt64(obj.LeaveId), action);
                        //#else
                        //    BackgroundJob.Enqueue(() => emailHelper.SendEmail(Convert.ToInt64(obj.LeaveId), action));
                        //#endif
                    }
                    catch
                    {
                        throw;
                    }
                }
                return Json(Status);
            }
        }

        public ActionResult LoadLeaveSummaryFull(Int64 userId)
        {
            IList<LeaveSummary> LeaveRequests = null;
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetLeaveSumary(userId, System.DateTime.Now.Year);
            }
            return PartialView("LeaveSummaryFullPartial", LeaveRequests);
        }

        public ActionResult LoadApplyLeaveSummary()
        {
            IList<LeaveSummary> LeaveRequests = null;
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetLeaveSumary(this.UserId, System.DateTime.Now.Year);
            }
            return PartialView("LeaveSummaryFullPartial", LeaveRequests);
        }

        public ActionResult GetLeaveSumary()
        {
            IList<LeaveSummary> LeaveRequests = null;
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.GetLeaveSumary(UserId, System.DateTime.Now.Year);
            }
            return View(LeaveRequests);
        }

        public int GetHolidayCount(string startDate, string endDate)
        {
            DateTime startDateFormatted = System.DateTime.Now;
            DateTime endDateFormatted = System.DateTime.Now;//TODO check these conditions
            if (startDate != null)
            {
                if (startDate.Trim() != "")
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
            }

            if (startDateFormatted > endDateFormatted)
            {
                return 0;
            }

            int holidayCount = 0;

            using (var client = new LeaveClient())
            {
                holidayCount = client.GetHolidayCount(startDateFormatted, endDateFormatted, UserId);
            }
            return holidayCount;
        }

        [HttpPost]
        public decimal ReturnDuration(string LeaveFrom, string LeaveUpto, string LeaveFromTime, string LeaveUptoTime)
        {
            DateTime startDateFormatted = System.DateTime.Now;
            DateTime endDateFormatted = System.DateTime.Now;//TODO check these conditions
            if (LeaveFrom != null)
            {
                if (LeaveFrom.Trim() != "")
                {
                    try
                    {
                        startDateFormatted = DateTime.Parse(LeaveFrom, new CultureInfo("en-GB", true));
                        endDateFormatted = DateTime.Parse(LeaveUpto, new CultureInfo("en-GB", true));
                    }
                    catch
                    {
                    }
                }
            }
            if (startDateFormatted > endDateFormatted)
            {
                return 0;
            }

            decimal duration = 0;

            using (var client = new LeaveClient())
            {
                duration = client.ReturnDuration(startDateFormatted, endDateFormatted, LeaveFromTime, LeaveUptoTime, UserId);
            }

            return duration;
        }

        public ActionResult GetLeaveDetailCalculation(string LeaveFrom, string LeaveUpto, string LeaveFromTime, string LeaveUptoTime, Int64 LeaveTyp)
        {
            DateTime startDateFormatted = System.DateTime.Now;
            DateTime endDateFormatted = System.DateTime.Now;//TODO check these conditions
            if (LeaveFrom != null)
            {
                if (LeaveFrom.Trim() != "")
                {
                    try
                    {
                        startDateFormatted = DateTime.Parse(LeaveFrom, new CultureInfo("en-GB", true));
                        endDateFormatted = DateTime.Parse(LeaveUpto, new CultureInfo("en-GB", true));
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            IList<LeaveDtl> LeaveRequests = new List<LeaveDtl>();
            if (endDateFormatted >= startDateFormatted)
            {
                using (var client = new LeaveClient())
                {
                    LeaveRequests = client.GetLeaveDetailCalculation(startDateFormatted, endDateFormatted, LeaveFromTime, LeaveUptoTime, UserId, LeaveTyp);
                }
            }
            return PartialView("LeaveDetailSplitPartial", LeaveRequests);
        }

        public ActionResult ShowLeaveDetail(Int64 LeaveId)
        {
            IList<LeaveDetailModel> LeaveRequests = null;
            using (var client = new LeaveClient())
            {
                LeaveRequests = client.ShowLeaveDetail(LeaveId);
            }
            return PartialView("ShowLeaveDetailSplitPartial", LeaveRequests);
        }
    }
}