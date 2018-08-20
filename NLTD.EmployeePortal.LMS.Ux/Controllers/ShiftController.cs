using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Ux.AppHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NLTD.EmployeePortal.LMS.Ux.Controllers
{
    public class ShiftController : BaseController
    {
        // GET: TimeSheet

        public ActionResult TeamShiftAllocation()
        {
            EmployeeShifts shiftEmployees = new EmployeeShifts();
            ViewBag.RequestLevelPerson = "Team";
            //IList<Shifts> shiftMaster = null;
            using (var client = new ShiftClient())
            {
                shiftEmployees.Shifts = client.GetShiftMaster().OrderBy(p => p.FromTime).ToList();
                shiftEmployees.shiftEmployees = client.GetShiftDetailsForUsers(this.UserId, "Team");
            }
            return View("ShiftAllocation", shiftEmployees);
        }

        public ActionResult AdminShiftAllocation()
        {
            ViewBag.RequestLevelPerson = "Admin";
            EmployeeShifts shiftEmployees = new EmployeeShifts();

            using (var client = new ShiftClient())
            {
                shiftEmployees.Shifts = client.GetShiftMaster().OrderBy(p => p.FromTime).ToList();
                shiftEmployees.shiftEmployees = client.GetShiftDetailsForUsers(this.UserId, "Admin");
            }
            return View("ShiftAllocation", shiftEmployees);
        }

        public ActionResult ShiftMaster()
        {
            ViewBag.Authorize = this.IsAuthorized;
            return View();
        }

        public ActionResult GetShiftDetail(string RequestMenuUser)
        {
            IList<ShiftAllocation> shiftAllocation = null;

            using (var client = new ShiftClient())
            {
                shiftAllocation = client.GetShiftAllocation(this.UserId, RequestMenuUser);
            }

            return PartialView("ShiftAllocationDetailPartial", shiftAllocation);
        }

        public ActionResult GetShiftDetailsForUsers(string RequestMenuUser)
        {
            List<ShiftEmployees> shiftEmployees = null;

            using (var client = new ShiftClient())
            {
                shiftEmployees = client.GetShiftDetailsForUsers(this.UserId, RequestMenuUser);
            }

            return PartialView("AddOrEditShiftAllocation", shiftEmployees);
        }

        public ActionResult GetShiftMasterDetail()
        {
            IList<Shifts> shiftMaster = null;

            using (var client = new ShiftClient())
            {
                shiftMaster = client.GetShiftMaster();
            }
            return PartialView("ShiftMasterDetailPartial", shiftMaster);
        }

        public ActionResult GetShiftMasterDetailwithId(Int64 shiftId)
        {
            Shifts objShifts = new Shifts();
            if (shiftId != 0)
            {
                using (var client = new ShiftClient())
                {
                    objShifts = client.GetShiftMasterWithId(shiftId);
                }
            }
            return PartialView("AddOrEditShift", objShifts);
        }

        public ActionResult SaveEmployeeShift(List<Int64> UserId, int Shift, DateTime FromDate, DateTime ToDate, string RequestMenuUser)
        {
            string result = "";
            EmployeeProfile EmployeeProfileObj = (EmployeeProfile)Session["Profile"];

            if (ModelState.IsValid)
            {
                if (EmployeeProfileObj.RoleText == "Employee")
                {
                    if (RequestMenuUser == "Team" && (FromDate <= DateTime.Now.AddDays(-7) || ToDate <= DateTime.Now.AddDays(-7)))
                    {
                        result = "The system restricts modifying shifts earlier than 7 days. Please contact HR for any changes.";
                    }
                    else
                    {
                        using (var client = new ShiftClient())
                        {
                            result = client.SaveEmployeeShift(UserId, Shift, FromDate, ToDate, this.UserId);
                        }
                    }
                }
                else
                {
                    using (var client = new ShiftClient())
                    {
                        result = client.SaveEmployeeShift(UserId, Shift, FromDate, ToDate, this.UserId);
                    }
                }
            }

            return Json(result);
        }

        public ActionResult SaveShiftMaster(int shiftId, string shiftName, TimeSpan fromTime, TimeSpan toTime)
        {
            string result = "";

            if (ModelState.IsValid)
            {
                using (var client = new ShiftClient())
                {
                    result = client.SaveShiftMaster(shiftId, shiftName, fromTime, toTime, this.UserId);
                }
            }

            return Json(result);
        }

        public ActionResult EmployeeShiftAllocation()
        {
            ViewBag.RequestLevelPerson = "Admin";
            ManageTeamLeavesQueryModel qyMdl = new ManageTeamLeavesQueryModel();
            return View("EmployeeShiftAllocation", qyMdl);
        }

        public ActionResult MyShiftDetails()
        {
            ViewBag.RequestLevelPerson = "My";
            ManageTeamLeavesQueryModel qyMdl = new ManageTeamLeavesQueryModel();
            return View("EmployeeShiftAllocation", qyMdl);
        }

        public ActionResult TeamEmployeeShiftAllocation()
        {
            ViewBag.RequestLevelPerson = "Team";
            ManageTeamLeavesQueryModel qyMdl = new ManageTeamLeavesQueryModel();
            return View("EmployeeShiftAllocation", qyMdl);
        }

        public ActionResult GetEmployeeShiftDetails(Int64 UserId, string RequestMenuUser, string FromDate, string ToDate, string Shift)
        {
            EmpShift shiftDetail = null;

            using (var client = new ShiftClient())
            {
                if (RequestMenuUser == "My" && UserId == 0)
                    UserId = this.UserId;

                shiftDetail = client.GetEmployeeShiftDetails(UserId, RequestMenuUser, this.UserId);
                ViewBag.FromDate = FromDate;
                ViewBag.ToDate = ToDate;
                ViewBag.Shift = Shift;
                ViewBag.RequestLevelPerson = RequestMenuUser;
            }

            return PartialView("EmpShiftAllocationPartial", shiftDetail);
        }

        public ActionResult SaveIndividualEmployeeShift(DateTime FromDate, DateTime ToDate, int Shift, Int64 UserId, string RequestMenuUser)
        {
            string result = "";
            EmployeeProfile EmployeeProfileObj = (EmployeeProfile)Session["Profile"];
            if (ModelState.IsValid)
            {
                if (EmployeeProfileObj.RoleText == "Employee")
                {
                    if (RequestMenuUser == "Team" && (FromDate <= DateTime.Now.AddDays(-7) || ToDate <= DateTime.Now.AddDays(-7)))
                    {
                        result = "The system restricts modifying shifts earlier than 7 days. Please contact HR for any changes.";
                    }
                    else
                    {
                        using (var client = new ShiftClient())
                        {
                            result = client.SaveIndividualEmployeeShift(UserId, Shift, FromDate, ToDate, this.UserId);
                        }
                    }
                }
                else
                {
                    using (var client = new ShiftClient())
                    {
                        result = client.SaveIndividualEmployeeShift(UserId, Shift, FromDate, ToDate, this.UserId);
                    }
                }
            }

            return Json(result);
        }
    }
}