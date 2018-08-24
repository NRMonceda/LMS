using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Repository
{
    public interface IEmployeeHelper : IDisposable
    {
        EmployeeProfile GetEmployeeProfile(Int64 userId);

        string UpdateEmployeeProfile(EmployeeProfile profile, Int64 ModifiedBy);

        List<DropDownItem> GetReportToList(Int64 OfficeId);

        Int64 GetEmployeeId(String LogonId);

        string GetNewEmpId(Int64 OfficeId);

        List<DropDownItem> GetActiveEmpList(Int64 OfficeId, Int64? exceptUserId);

        EmployeeProfile GetEmployeeLoginProfile(string LogonId);

        ViewEmployeeProfileModel ViewEmployeeProfile(Int64 userId);

        long GetUserId(string name);

        string ReportingToName(Int64 userId);

        IList<ViewEmployeeProfileModel> GetTeamProfiles(Int64 userId, bool onlyReportedToMe, Int64? paramUserId, string requestMenuUser, bool hideInactiveEmp);

        IList<ElCreditModel> GetEmployeeProfilesforEL(DateTime lastCreditRun);
    }
}