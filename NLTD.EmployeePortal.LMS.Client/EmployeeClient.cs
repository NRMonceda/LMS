using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.DbHelper;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Client
{
    public class EmployeeClient : IEmployeeHelper
    {
        public void Dispose()
        {
            //Nothing to dispose...
        }

        public long GetEmployeeId(string LogonId)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetEmployeeId(LogonId);
            }
        }

        public string ReportingToName(Int64 userId)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.ReportingToName(userId);
            }
        }

        public string GetNewEmpId(Int64 OfficeId)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetNewEmpId(OfficeId);
            }
        }

        public long GetUserId(string name)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetUserId(name);
            }
        }

        public EmployeeProfile GetEmployeeProfile(Int64 userId)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetEmployeeProfile(userId);
            }
        }

        public ViewEmployeeProfileModel ViewEmployeeProfile(Int64 userId)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.ViewEmployeeProfile(userId);
            }
        }

        public EmployeeProfile GetEmployeeLoginProfile(string LogonId)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetEmployeeLoginProfile(LogonId);
            }
        }

        public List<DropDownItem> GetReportToList(long OfficeId)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetReportToList(OfficeId);
            }
        }

        public List<DropDownItem> GetActiveEmpList(long OfficeId, Int64? exceptUserId)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetActiveEmpList(OfficeId, exceptUserId);
            }
        }

        public string UpdateEmployeeProfile(EmployeeProfile profile, Int64 ModifiedBy)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.UpdateEmployeeProfile(profile, ModifiedBy);
            }
        }

        public IList<ViewEmployeeProfileModel> GetTeamProfiles(Int64 userId, bool onlyReportedToMe, Int64? paramUserId, string requestMenuUser, bool hideInactiveEmp)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetTeamProfiles(userId, onlyReportedToMe, paramUserId, requestMenuUser, hideInactiveEmp);
            }
        }

        public IList<ElCreditModel> GetEmployeeProfilesforEL(DateTime lastCreditRun)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetEmployeeProfilesforEL(lastCreditRun);
            }
        }
        public IList<LeaveCreditModel> GetEmployeeProfilesforCLSL(long leaveTypeId)
        {
            using (IEmployeeHelper helper = new EmployeeHelper())
            {
                return helper.GetEmployeeProfilesforCLSL(leaveTypeId);
            }
        }
       
    }
}