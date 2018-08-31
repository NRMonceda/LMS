﻿using Hangfire;
using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Dac;
using NLTD.EmployeePortal.LMS.Dac.DbModel;
using NLTD.EmployeePortal.LMS.Ux.AppHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace NLTD.EmployeePortal.LMS.Ux.Controllers
{
    public class ProfileController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        public ActionResult ViewProfile()
        {
            ViewBag.PageTile = "Employee Profile";
            ViewBag.TagLine = "";
            ViewBag.IsSelfProfile = true;
            Int64 userIdForProfile = 0;
            EmployeeProfile profile = null;
            if (TempData["UserId"] == null)
            {
                userIdForProfile = UserId;
            }
            else
                userIdForProfile = Convert.ToInt64(TempData["UserId"].ToString());

            using (var client = new EmployeeClient())
            {
                profile = client.GetEmployeeProfile(userIdForProfile);
            }
            using (var client = new ShiftClient())
            {
                ViewBag.ShiftList = client.GetShiftMaster();
            }
            using (var client = new OfficeLocationClient())
            {
                List<DropDownItem> lstOfc = client.GetAllOfficeLocations();
                DropDownItem di = new DropDownItem();
                ViewBag.EmpOffice = lstOfc.Where(x => x.Key == Convert.ToString(OfficeId)).ToList();
                di.Key = "";
                di.Value = "";
                lstOfc.Insert(0, di);
                ViewBag.OfficeLocationList = lstOfc;
            }
            using (var client = new RoleClient())
            {
                ViewBag.RoleList = client.GetAllRoles();
            }
            if (profile != null)
            {
                using (var client = new EmployeeClient())
                {
                    IList<DropDownItem> reptList = client.GetActiveEmpList(profile.OfficeId, userIdForProfile);
                    DropDownItem di = new DropDownItem();
                    di.Key = "";
                    di.Value = "";
                    reptList.Insert(0, di);
                    ViewBag.ReportToList = reptList;
                }
            }
            if (TempData["Mode"] == null)
                profile.Mode = "View";
            else
                profile.Mode = TempData["Mode"].ToString();

            return View("EmployeeProfile", profile);
        }

        public ActionResult AddNewEmployee()
        {
            EmployeeProfile profile = new EmployeeProfile();
            if (this.IsAuthorized == "NoAuth")
            {
                Response.Redirect("~/Home/Unauthorized");
                return null;
            }
            else
            {
                using (var client = new OfficeLocationClient())
                {
                    var lstOfc = client.GetAllOfficeLocations();
                    DropDownItem di = new DropDownItem();
                    ViewBag.EmpOffice = lstOfc.Where(x => x.Key == Convert.ToString(OfficeId)).ToList();
                    di.Key = "";
                    di.Value = "";
                    lstOfc.Insert(0, di);
                    ViewBag.OfficeLocationList = lstOfc;
                }
                using (var client = new RoleClient())
                {
                    ViewBag.RoleList = client.GetAllRoles();
                }
                using (var client = new ShiftClient())
                {
                    ViewBag.ShiftList = client.GetShiftMaster();
                }
                using (var client = new EmployeeClient())
                {
                    IList<DropDownItem> reptList = client.GetActiveEmpList(OfficeId, null);
                    DropDownItem di = new DropDownItem();
                    di.Key = "";
                    di.Value = "";
                    reptList.Insert(0, di);
                    ViewBag.ReportToList = reptList;
                }
                using (var client = new EmployeeClient())
                {
                    profile.EmployeeId = client.GetNewEmpId(OfficeId);
                }
                profile.IsActive = true;
                profile.Mode = "Add";
                profile.LogonId = "CORP\\";
                profile.Sunday = true;
                profile.Saturday = true;
                return View("EmployeeProfile", profile);
            }
        }

        public ActionResult UpdateEmployee()
        {
            YearwiseLeaveSummaryQueryModel emp = new YearwiseLeaveSummaryQueryModel();
            if (this.IsAuthorized == "NoAuth")
            {
                Response.Redirect("~/Home/Unauthorized");
                return null;
            }
            else
                return View("UpdateEmployee", emp);
        }

        public ActionResult CallProfileEdit(string name)
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
                TempData["UserId"] = userId;
                TempData["Mode"] = "Update";
                return Json(new { redirectToUrl = Url.Action("ViewProfile", "Profile") });
            }
        }

        public ActionResult CallProfileView(string name)
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
                TempData["UserId"] = userId;
                return Json(new { redirectToUrl = Url.Action("ViewEmployeeProfile", "Profile") });
            }
        }

        public ActionResult ViewEmployeeProfile()
        {
            ViewBag.PageTile = "Employee Profile";
            ViewBag.TagLine = "";
            ViewBag.IsSelfProfile = true;
            Int64 userIdForProfile = 0;
            ViewEmployeeProfileModel profile = null;
            if (TempData["UserId"] == null)
            {
                userIdForProfile = UserId;
            }
            else
                userIdForProfile = Convert.ToInt64(TempData["UserId"].ToString());
            using (var client = new EmployeeClient())
            {
                profile = client.ViewEmployeeProfile(userIdForProfile);
            }

            return View("ViewEmployeeProfile", profile);
        }

        public ActionResult SaveProfile(EmployeeProfile employee)
        {
            bool isValid = true;
            if (ModelState.IsValid)
            {
                Regex regex = new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");
                if (employee.EmailAddress != null)
                {
                    if (employee.EmailAddress.Trim() != "")
                    {
                        Match match = regex.Match(employee.EmailAddress);
                        if (!match.Success)
                        {
                            employee.ErrorMesage = "Invalid Email Address format.";
                            isValid = false;
                        }
                    }
                }
                if ((!employee.IsActive) && (employee.RelievingDate == null))
                {
                    employee.ErrorMesage = "Enter Relieving Date.";
                    isValid = false;
                }
                else if (employee.DOJ > employee.RelievingDate)
                {
                    employee.ErrorMesage = "Relieving Date should be greater than Joining Date.";
                    isValid = false;
                }
                else if (employee.DOJ > employee.ConfirmationDate)
                {
                    employee.ErrorMesage = "Confirmation Date should be greater than Joining Date.";
                    isValid = false;
                }
                if (employee.IsActive)
                {
                    employee.RelievingDate = null;
                }
                if (isValid)
                {
                    employee.LogonId = employee.LogonId.ToUpper();
                    employee.FirstName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(employee.FirstName.ToLower());
                    employee.LastName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(employee.LastName.ToLower());
                    employee.EmailAddress = employee.EmailAddress == null ? null : employee.EmailAddress.ToLower();

                    using (var client = new EmployeeClient())
                    {
                        string result = client.UpdateEmployeeProfile(employee, UserId);
                        if (result == "Saved")
                        {
                            employee.ErrorMesage = "Saved";
                        }
                        else if (result == "NeedRole")
                            employee.ErrorMesage = "Only the user with role 'HR' is allowed to do this action.";
                        else if (result == "noChanges")
                            employee.ErrorMesage = "No changes made to Employee Profile.";
                        else if (result == "Duplicate")
                            employee.ErrorMesage = "The employee Id already exists.";
                        else if (result == "DupCorp")
                            employee.ErrorMesage = "The logon id was already assigned to another employee.";
                        else if (result == "DupCard")
                            employee.ErrorMesage = "The card number was already assigned to another employee.";
                    }
                }
            }
            else
            {
                employee.ErrorMesage = "Fix the error messages shown and try to Save again.";
            }

            using (var client = new OfficeLocationClient())
            {
                var lstOfc = client.GetAllOfficeLocations();

                ViewBag.EmpOffice = lstOfc.Where(x => x.Key == Convert.ToString(OfficeId)).ToList();
                DropDownItem di = new DropDownItem();
                di.Key = "";
                di.Value = "";
                lstOfc.Insert(0, di);
                ViewBag.OfficeLocationList = lstOfc;
            }
            using (var client = new RoleClient())
            {
                ViewBag.RoleList = client.GetAllRoles();
            }
            using (var client = new ShiftClient())
            {
                ViewBag.ShiftList = client.GetShiftMaster();
            }
            using (var client = new EmployeeClient())
            {
                IList<DropDownItem> reptList = new List<DropDownItem>();
                if (employee.Mode == "Add")
                {
                    ViewBag.ReportToList = client.GetActiveEmpList(employee.OfficeId, null);
                }
                else
                {
                    ViewBag.ReportToList = client.GetActiveEmpList(employee.OfficeId, employee.UserId);
                }

                DropDownItem di = new DropDownItem();
                di.Key = "";
                di.Value = "";
                reptList.Insert(0, di);
            }
            return View("EmployeeProfile", employee);
        }

        public ActionResult MyLmsProfile()
        {
            EmployeeProfileSearchModel mdl = new EmployeeProfileSearchModel();
            mdl.RequestLevelPerson = "My";

            return View("SearchTeamLmsProfile", mdl);
        }

        public ActionResult TeamLmsProfile()
        {
            EmployeeProfileSearchModel mdl = new EmployeeProfileSearchModel();
            mdl.RequestLevelPerson = "Team";
            mdl.OnlyReportedToMe = true;
            using (var client = new ShiftClient())
            {
                ViewBag.ShiftList = client.GetShiftMaster();
            }
            return View("SearchTeamLmsProfile", mdl);
        }

        public ActionResult AdminLmsProfile()
        {
            EmployeeProfileSearchModel mdl = new EmployeeProfileSearchModel();
            mdl.RequestLevelPerson = "Admin";
            mdl.HideInactiveEmp = true;
            return View("SearchTeamLmsProfile", mdl);
        }

        public ActionResult TeamProfileData(bool onlyReportedToMe, Int64? paramUserId, string requestMenuUser, bool hideInactiveEmp)
        {
            IList<ViewEmployeeProfileModel> lstProfile = new List<ViewEmployeeProfileModel>();

            using (var client = new EmployeeClient())
            {
                lstProfile = client.GetTeamProfiles(this.UserId, onlyReportedToMe, paramUserId, requestMenuUser, hideInactiveEmp);
            }

            return PartialView("EmployeeLmsProfileNamesPartial", lstProfile);
        }

        public ActionResult SearchLeaveBalanceProfile()
        {
            EmployeeProfileSearchModel mdl = new EmployeeProfileSearchModel();
            if (this.IsAuthorized == "NoAuth")
            {
                Response.Redirect("~/Home/Unauthorized");
                return null;
            }
            else
            {
                return View("SearchLeaveBalanceProfile", mdl);
            }
        }

        public ActionResult EmployeeLeaveBalanceDetails(Int64 UserId)
        {
            IList<LeaveBalanceEmpProfile> lstProfile = new List<LeaveBalanceEmpProfile>();

            using (var client = new EmployeeLeaveBalanceClient())
            {
                lstProfile = client.GetLeaveBalanceEmpProfile(UserId);
            }

            return PartialView("EmployeeLeaveBalanceProfilePartial", lstProfile);
        }

        public ActionResult SaveLeaveBalance(List<EmployeeLeaveBalanceDetails> lst, Int64 EmpUserid)
        {
            string result = "";
            result = UpdateLeaveBalance(lst);
            return Json(result);
        }

        public ActionResult EarnedLeaveCredit()
        {
            DateTime lastCreditRun = GetlastCreditRunforEL();
            ViewBag.CurrentRun = GetCurrentRunforEL(lastCreditRun);
            ViewBag.lastCreditProcess = lastCreditRun.AddMonths(-1).ToString("MMM-yyyy");

            if (this.IsAuthorized == "NoAuth")
            {
                Response.Redirect("~/Home/Unauthorized");
                return null;
            }
            else
            {
                return View("EarnedLeaveCredit");
            }
        }

        public ActionResult GetEarnedLeaveMasterDetail()
        {
            IList<ElCreditModel> lstProfile = GetEmployeeELData();
            return PartialView("EarnedLeaveCreditPartial", lstProfile);
        }

        public static DateTime GetlastCreditRunforEL()
        {
            DateTime lastCreditRun;
            using (var context = new NLTDDbContext())
            {
                List<LeaveTypesModel> LeaveTypes = (from l in context.LeaveType
                                                    where l.Type == "Earned Leave"
                                                    select new LeaveTypesModel
                                                    {
                                                        lastCreditRun = l.lastCreditRun
                                                    }).ToList();
                lastCreditRun = LeaveTypes.Select(s => (DateTime)s.lastCreditRun).FirstOrDefault();
            }
            return lastCreditRun;
        }

        public static string GetCurrentRunforEL(DateTime lastCreditRun)
        {
            string currentRun;
            DateTime today = DateTime.Today;
            if (today.Month == lastCreditRun.Month)
            {
                currentRun = "";
            }
            else
            {
                DateTime prevMonth = new DateTime(today.Year, today.Month, 1).AddDays(-1);
                currentRun = lastCreditRun.ToString("MMM-yyyy") + " to " + prevMonth.ToString("MMM-yyyy");
            }
            return currentRun;
        }

        public IList<ElCreditModel> GetEmployeeELData()
        {
            IList<ElCreditModel> lstProfile = new List<ElCreditModel>();
            DateTime lastCreditRun = GetlastCreditRunforEL();
            using (var client = new EmployeeClient())
            {
                lstProfile = client.GetEmployeeProfilesforEL(lastCreditRun);
            }
            return lstProfile;
        }

        public ActionResult ExportExcelEarnedLeaveCreditDetails()
        {
            IList<ElCreditModel> lstProfile = GetEmployeeELData();
            List<ElCreditModel> excelData = new List<ElCreditModel>();
            excelData = lstProfile.ToList();
            if (excelData.Count > 0)
            {
                string[] columns = { "Emp Id", "Name", "Joining Date", "Confirmation Date", "Existing EL Balance", "EL Credit", "Total EL Balance" };
                byte[] filecontent = ExcelExportHelper.ExportExcelELCredit(excelData, "", false, columns);
                return File(filecontent, ExcelExportHelper.ExcelContentType, "ELCreditReport_" + System.DateTime.Now + ".xlsx");
            }
            else
            {
                ViewBag.ErrorMsg = "Excel file is not generated as no data returned.";
                return View("EarnedLeaveCredit");
            }
            return Json("Downloaded");
        }

        public ActionResult UpdateEarnedLeaves()
        {
            string result = "";
            DateTime lastCreditRun = GetlastCreditRunforEL();
            string remarks = "EL Credit for " + GetCurrentRunforEL(lastCreditRun);

            var lstProfile = GetEmployeeELData();

            var ELCreditList = (from l in lstProfile
                                select new EmployeeLeaveBalanceDetails
                                {
                                    UserId = l.UserId,
                                    CreditOrDebit = "C",
                                    LeaveTypeId = 2,
                                    EmployeeId = l.EmployeeId,
                                    BalanceDays = l.CurrentEL,
                                    NoOfDays = l.ELCredit,
                                    Remarks = remarks,
                                    TotalDays = l.NewELBalance,
                                    LeaveBalanceId = l.LeaveBalanceId
                                }).ToList();

            result = UpdateLeaveBalance(ELCreditList, true);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public string UpdateLeaveBalance(List<EmployeeLeaveBalanceDetails> lst, bool isElCredit = false)
        {
            string result = "";
            try
            {
                using (var client = new EmployeeLeaveBalanceClient())
                {
                    long LoginUserId = this.UserId;
                    result = client.UpdateLeaveBalance(lst, LoginUserId, isElCredit);
                }

                EmailHelper emailHelper = new EmailHelper();
                try
                {
                    emailHelper.SendEmailforAddLeave(lst);
                }
                catch { }
            }
            catch
            {
                throw;
            }
            return result;
        }
    }
}