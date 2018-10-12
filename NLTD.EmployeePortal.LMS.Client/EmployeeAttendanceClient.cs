using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.DbHelper;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Client
{
    public class EmployeeAttendanceClient : IEmployeeAttendanceHelper
    {
        private IEmployeeAttendanceHelper employeeAttendanceHelper;

        public EmployeeAttendanceClient()
        {
            employeeAttendanceHelper = new EmployeeAttendanceHelper();
        }

        public void Dispose()
        {
        }

        public List<EmployeeAttendanceModel> GetAttendance(Int64 EmployeeID)
        {
            return employeeAttendanceHelper.GetAttendance(EmployeeID);
        }

        public List<EmployeeAttendanceModel> GetAttendanceForRange(Int64 EmployeeID, DateTime FromDateTime, DateTime ToDateTime, string requestLevelPerson, bool IsDirectEmployees, bool getBreakStatus)
        {
            return employeeAttendanceHelper.GetAttendanceForRange(EmployeeID, FromDateTime, ToDateTime, requestLevelPerson, IsDirectEmployees, getBreakStatus);
        }

        public List<EmployeeAttendanceModel> GetAccessCardAttendanceForRange(Int64 EmployeeID, DateTime FromDateTime, DateTime ToDateTime, string requestLevelPerson)
        {
            return employeeAttendanceHelper.GetAccessCardAttendanceForRange(EmployeeID, FromDateTime, ToDateTime, requestLevelPerson);
        }
    }
}