using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Repository;
using NLTD.EmployeePortal.LMS.Ux.AppHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NLTD.EmployeePortal.LMS.Ux.Controllers
{
    public class AttendanceController : BaseController
    {
        private IEmployeeAttendanceHelper EmployeeAttendanceHelper;

        // GET: Attendance
        public AttendanceController()
        {
            EmployeeAttendanceHelper = new EmplyeeAttendanceClient();
        }

        public ActionResult MyAttendance()
        {
            ViewBag.RequestLevelPerson = "My";
            return View();
        }
        public ActionResult AccessCardAttendance()
        {
            ViewBag.RequestLevelPerson = "Team";
            return View();
        }

        public ActionResult MyTeamAttendance()
        {
            ViewBag.RequestLevelPerson = "Team";
            return View("MyAttendance");
        }

        public ActionResult MyTimeSheet()
        {
            ViewBag.RequestLevelPerson = "My";
            return View("TimeSheet");
        }

        public ActionResult MyTeamTimeSheet()
        {
            ViewBag.RequestLevelPerson = "Team";
            return View("TimeSheet");
        }

        public ActionResult EmployeeTimeSheet()
        {
            ViewBag.RequestLevelPerson = "Admin";
            return View("TimeSheet");
        }

        public ActionResult EmployeeAttendance()
        {
            ViewBag.RequestLevelPerson = "Admin";
            return View("MyAttendance");
        }
    }
}