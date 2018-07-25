using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Repository
{
    public interface IReportHelper : IDisposable
    {
        // For Maintain the shift master
        List<lateAndEarlyRpt> GetLateAndEarlyEmployees(DateTime FromDate, DateTime ToDate, Int64 UserId, bool OnlyReportedToMe);

        List<NoOfLateInMonth> GetLateReport(Int64 UserID, DateTime FromDate, DateTime ToDate, bool myDirectEmployees);
    }
}