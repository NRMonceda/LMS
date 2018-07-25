using NLTD.EmployeePortal.LMS.Client;
using System;
using System.Web.Mvc;

namespace NLTD.EmployeePortal.LMS.Ux.Controllers
{
    public class DataController : Controller
    {
        public ActionResult GetReportToList(Int64 LocationId)
        {
            using (var client = new EmployeeClient())
            {
                var ReportToList = client.GetReportToList(LocationId);
                return Json(ReportToList);
            }
        }
    }
}