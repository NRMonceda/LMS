using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Dac.DbHelper
{
    public class ReportHelper : IReportHelper, IDisposable
    {
        public void Dispose()
        {
            //Nothing to dispose...
        }

        public List<lateAndEarlyRpt> GetLateAndEarlyEmployees(DateTime FromDate, DateTime ToDate, Int64 UserId, bool OnlyReportedToMe)
        {
            using (var dac = new ReportDac())
            {
                return dac.GetLateAndEarlyEmployees(FromDate, ToDate, UserId, OnlyReportedToMe);
            }
        }

        public List<NoOfLateInMonth> GetLateReport(Int64 UserID, DateTime FromDate, DateTime ToDate, bool myDirectEmployees)
        {
            using (var dac = new ReportDac())
            {
                return dac.GetLateReport(UserID, FromDate, ToDate, myDirectEmployees);
            }
        }
    }
}