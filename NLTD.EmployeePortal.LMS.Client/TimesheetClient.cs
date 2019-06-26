using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.DbHelper;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Client
{
    public class TimesheetClient : ITimesheetHelper
    {
        private ITimesheetHelper TimesheetHelper;

        public TimesheetClient()
        {
            TimesheetHelper = new TimesheetHelper();
        }

        public List<TimeSheetModel> GetMyTimeSheet(Int64 UserID, DateTime FromDate, DateTime ToDate)
        {
            List<TimeSheetModel> timeSheetModelList = TimesheetHelper.GetMyTimeSheet(UserID, FromDate, ToDate);
            return timeSheetModelList;
        }

        public List<TimeSheetModel> GetMyTeamTimeSheet(Int64 UserID, DateTime FromDate, DateTime ToDate, bool myDirectEmployees)
        {
            List<TimeSheetModel> timeSheetModelList = TimesheetHelper.GetMyTeamTimeSheet(UserID, FromDate, ToDate, myDirectEmployees);
            return timeSheetModelList;
        }
    }
}