using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.Dac;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Dac.DbHelper
{
    public class EmployeeAttendanceHelper : IEmployeeAttendanceHelper
    {
        private IEmployeeAttendanceHelper employeeAttendanceHelper;

        public EmployeeAttendanceHelper()
        {
            employeeAttendanceHelper = new EmployeeAttendanceDac();
        }

        public void Dispose()
        {
            //  throw new NotImplementedException();
        }

        public List<EmployeeAttendanceModel> GetAttendance(Int64 UserID)
        {
            return employeeAttendanceHelper.GetAttendance(UserID);
        }

        public List<EmployeeAttendanceModel> GetAttendanceForRange(Int64 UserID, DateTime FromDateTime, DateTime ToDateTime, string requestLevelUser, bool IsDirectEmployees, bool getBreakStatus)
        {
            return employeeAttendanceHelper.GetAttendanceForRange(UserID, FromDateTime, ToDateTime, requestLevelUser, IsDirectEmployees, getBreakStatus);
        }

        public List<EmployeeAttendanceModel> GetAccessCardAttendanceForRange(Int64 UserID, DateTime FromDateTime, DateTime ToDateTime, string requestLevelUser)
        {
            return employeeAttendanceHelper.GetAccessCardAttendanceForRange(UserID, FromDateTime, ToDateTime, requestLevelUser);
        }
    }
}