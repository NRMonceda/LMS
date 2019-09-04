using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.Dac;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Dac.DbHelper
{
    public class EmployeeHelper : IEmployeeHelper
    {
        public void Dispose()
        {
            //Nothing to dispose...
        }

        public long GetEmployeeId(string LogonId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetEmployeeId(LogonId);
            }
        }

        public string ReportingToName(Int64 userId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.ReportingToName(userId);
            }
        }

        public long GetUserId(string name)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetUserId(name);
            }
        }

        public string GetNewEmpId(Int64 OfficeId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetNewEmpId(OfficeId);
            }
        }
        public string GetNewEmpId(Int64 OfficeId,long employmentTypeId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetNewEmpId(OfficeId, employmentTypeId);
            }
        }
        public IList<DropDownItem> GetEmploymentTypes()
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetEmploymentTypes();
            }
        }

        public EmployeeProfile GetEmployeeProfile(Int64 userId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetEmployeeProfile(userId);
            }
        }

        public ViewEmployeeProfileModel ViewEmployeeProfile(Int64 userId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.ViewEmployeeProfile(userId);
            }
        }

        public EmployeeProfile GetEmployeeLoginProfile(string LogonId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetEmployeeLoginProfile(LogonId);
            }
        }

        public List<DropDownItem> GetReportToList(long OfficeId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetReportToList(OfficeId);
            }
        }

        public List<DropDownItem> GetActiveEmpList(long OfficeId, Int64? exceptUserId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetActiveEmpList(OfficeId, exceptUserId);
            }
        }

        public IList<ViewEmployeeProfileModel> GetTeamProfiles(Int64 userId, bool onlyReportedToMe, Int64? paramUserId, string requestMenuUser, bool hideInactiveEmp)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetTeamProfiles(userId, onlyReportedToMe, paramUserId, requestMenuUser, hideInactiveEmp);
            }
        }

        public IList<LeaveCreditModel> GetEmployeeProfilesforEL(DateTime lastCreditRun)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetEmployeeProfilesforEL(lastCreditRun);
            }
        }
        public IList<LeaveCreditModel> GetEmployeeProfilesforCLSL(long leaveTypeId)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.GetEmployeeProfilesforCLSL(leaveTypeId);
            }
        }        
        public string UpdateEmployeeProfile(EmployeeProfile profile, Int64 ModifiedBy)
        {
            using (var dac = new EmployeeDac())
            {
                return dac.UpdateEmployeeProfile(profile, ModifiedBy);
            }
        }
    }
}