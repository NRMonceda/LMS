using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Dac.Dac;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.DbHelper
{
    public class TimesheetHelper : ITimesheetHelper
    {
        public List<ShiftQueryModel> GetShiftDetails(Int64 UserID)
        {
            return new List<ShiftQueryModel>();
        }

        public List<TimeSheetModel> GetMyTimeSheet(Int64 UserID, DateTime FromDate, DateTime ToDate)
        {
            ITimesheetHelper TimesheetHelperObj = new TimeSheetDac();
            List<TimeSheetModel> timeSheetModelList = TimesheetHelperObj.GetMyTimeSheet(UserID, FromDate, ToDate);
            return timeSheetModelList;
        }

        public List<TimeSheetModel> GetMyTeamTimeSheet(Int64 UserID, DateTime FromDate, DateTime ToDate, bool myDirectEmployees)
        {
            ITimesheetHelper TimesheetHelperObj = new TimeSheetDac();
            List<TimeSheetModel> timeSheetModelList = TimesheetHelperObj.GetMyTeamTimeSheet(UserID, FromDate, ToDate, myDirectEmployees);
            return timeSheetModelList;
        }
    }
}