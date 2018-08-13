using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Ux.Models;
using System;
using System.Net;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.SessionState;

namespace NLTD.EmployeePortal.LMS.Ux.AppHelpers
{
    [SessionState(SessionStateBehavior.Default)]
    public class BaseController : Controller
    {
        public Int64 UserId { get; set; }
        public Int64 OfficeId { get; set; }

        public bool IsLMSApprover { get; set; }

        public string ReportingToName { get; set; }

        public string Role { get; set; }

        public string IsAuthorized { get; set; }

        public BaseController()
        {
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            using (var client = new EmployeeClient())
            {
                var identity = (WindowsIdentity)System.Web.HttpContext.Current.User.Identity;
                string menu = string.Empty;

                var windowsLoginName = identity.Name.ToUpper();
                #if DEBUG
                //windowsLoginName = "CORP\\XYZ";
                #endif
                if (Request.QueryString["Username"] != null)
                {
                    windowsLoginName = Convert.ToString(Request.QueryString["Username"]).Replace(".", "\\");
                }
                if (windowsLoginName != "")
                {
                    ViewBag.DisplayName = windowsLoginName;

                    EmployeeProfile profile = new EmployeeProfile();
                    if (Session["Profile"] == null)
                    {
                        profile = client.GetEmployeeLoginProfile(windowsLoginName);
                        Session["Profile"] = profile;
                    }
                    else
                    {
                        profile = (EmployeeProfile)Session["Profile"];
                    }

                    if (profile != null)
                    {
                        UserId = profile.UserId;
                        OfficeId = profile.OfficeId;
                        ReportingToName = profile.ReportedToName;
                        IsLMSApprover = profile.IsHandleMembers;
                        Role = profile.RoleText;

                        if (profile.RoleId.HasValue)
                        {
                            if (Session["MenuItemsList"] == null || Session["MenuItemsList"] == "")
                            {
                                menu = MenuHelper.GetMenu(profile.RoleText, profile.IsHandleMembers);
                                ViewBag.MenuText = menu;
                                Session["MenuItemsList"] = menu;
                            }
                            else
                            {
                                ViewBag.MenuText = (string)Session["MenuItemsList"];
                            }
                        }
                        else
                        {
                            ViewBag.MenuText = MenuHelper.GetMenu("", false);
                        }
                        ViewBag.DisplayName = profile.FirstName + " " + profile.LastName;
                        ViewBag.EmpId = profile.EmployeeId;

                        string requestUrl = Request.Url.AbsolutePath;
                        string menuTxt = (string)ViewBag.MenuText;

                        if (menuTxt.Contains(requestUrl)) { }
                        else
                        {
                            IsAuthorized = "NoAuth";
                        }
                    }
                    else
                    {
                        this.UserId = 0;
                        this.OfficeId = 0;
                        ViewBag.MenuText = "NoProfile";
                    }
                }
                else
                {
                    this.UserId = 0;
                    this.OfficeId = 0;
                    ViewBag.MenuText = "NoProfile";
                }
            }
        }
    }
}