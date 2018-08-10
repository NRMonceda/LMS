using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Dac;
using NLTD.EmployeePortal.LMS.Ux.AppHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
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
                /*Commented on 4 Jan 2018 as Email Address and Corp Id are not mandatory.
                if (employee.LogonId.Trim().Length < 5) {
                    employee.ErrorMesage = "Logon Id should start with CORP\\";
                    isValid = false;
                }
                else if (employee.LogonId.Substring(0, 5).ToUpper() != "CORP\\") {
                    employee.ErrorMesage = "Logon Id should start with CORP\\";
                    isValid = false;
                }
                */

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

        //Added by Tamil
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
            if (ModelState.IsValid)
            {
                using (var client = new EmployeeLeaveBalanceClient())
                {
                    long LoginUserId = this.UserId;
                    result = client.UpdateLeaveBalance(lst, EmpUserid, LoginUserId);
                }
            }

            return Json(result);
        }

        public ActionResult EarnedLeaveCredit()
        {
            EmployeeProfile mdl = new EmployeeProfile();
            using (var context = new NLTDDbContext())
            {
                List<LeaveTypesModel> LeaveTypes = (from l in context.LeaveType
                                                    where l.LastRun != null
                                                    orderby l.Createdon descending
                                                    select new LeaveTypesModel
                                                    {
                                                        LastRun = l.LastRun
                                                    }).ToList();
                mdl.LastRun = LeaveTypes.Select(s => s.LastRun).FirstOrDefault().ToString();
            }
            
            if (this.IsAuthorized == "NoAuth")
            {
                Response.Redirect("~/Home/Unauthorized");
                return null;
            }
            else
            {
                return View("EarnedLeaveCredit", mdl);
            }
        }
    }
}