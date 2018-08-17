using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Dac.DbHelper
{
    public class LeaveHelper : ILeaveHelper
    {
        public string ChangeStatus(LeaveStatusModel status)
        {
            using (var dac = new LeaveDac())
            {
                return dac.ChangeStatus(status);
            }
        }

        public EmailDataModel ViewLeaveFromEmail(Int64 leaveId, Int64 userId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.ViewLeaveFromEmail(leaveId, userId);
            }
        }

        public void Dispose()
        {
            //Nothing to impement...
        }

        public IList<TeamLeaves> GetTeamLeaveHistory(ManageTeamLeavesQueryModel qryMdl)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetTeamLeaveHistory(qryMdl);
            }
        }

        public IList<TeamLeaves> GetLeaveRequests(ManageTeamLeavesQueryModel qryMdl)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetLeaveRequests(qryMdl);
            }
        }

        public EmailDataModel GetEmailDataAddLeave(Int64 userID, Int64? leaveTypeID)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetEmailDataAddLeave(userID, leaveTypeID);
            }
        }

        public EmailDataModel GetEmailData(Int64 leaveId, string actionName)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetEmailData(leaveId, actionName);
            }
        }

        public int GetHolidayCount(DateTime startDate, DateTime endDate, long empId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetHolidayCount(startDate, endDate, empId);
            }
        }

        public IList<DropDownItem> GetWeekOffs(Int64 UserId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetWeekOffs(UserId);
            }
        }

        public List<LeaveTypesModel> GetLeaveTypes(long OfficeId, Int64 userId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetLeaveTypes(OfficeId, userId);
            }
        }

        public List<DropDownItem> GetYearsFromLeaveBalance()
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetYearsFromLeaveBalance();
            }
        }

        public string SaveLeaveRequest(LeaveRequestModel request)
        {
            using (var dac = new LeaveDac())
            {
                return dac.SaveLeaveRequest(request);
            }
        }

        public IList<LeaveSummary> GetLeaveSumary(long UserId, Int32 summaryYear)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetLeaveSumary(UserId, summaryYear);
            }
        }

        public IList<HolidayModel> GetHolidaysDetails(long UserId, int holidayYear, ref bool previousYear, ref bool nextYear)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetHolidaysDetails(UserId, holidayYear, ref previousYear, ref nextYear);
            }
        }

        public IList<HolidayModel> GetHolidays(long UserId, Int32 holidayYear)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetHolidays(UserId, holidayYear);
            }
        }

        public IList<EmployeeWiseLeaveSummaryModel> GetEmployeeWiseLeaveSumary(Int64 UserId, int Year, string reqUsr, Int64? paramUserId, bool OnlyReportedToMe)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetEmployeeWiseLeaveSumary(UserId, Year, reqUsr, paramUserId, OnlyReportedToMe);
            }
        }

        public IList<DaywiseLeaveDtlModel> GetDaywiseLeaveDtl(DateTime? FromDate, DateTime? ToDate, bool IsLeaveOnly, Int64 LeadId, bool OnlyReportedToMe, Int64? paramUserId, string reqUsr, bool DonotShowRejected)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetDaywiseLeaveDtl(FromDate, ToDate, IsLeaveOnly, LeadId, OnlyReportedToMe, paramUserId, reqUsr, DonotShowRejected);
            }
        }

        public IList<MonthwiseLeavesCountModel> GetMonthwiseLeavesCount(Int32 year, Int64 LeadId, bool OnlyReportedToMe, Int64? paramUserId, string reqUsr)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetMonthwiseLeavesCount(year, LeadId, OnlyReportedToMe, paramUserId, reqUsr);
            }
        }

        public IList<LeaveDtl> GetLeaveDetailCalculation(DateTime LeaveFrom, DateTime LeaveUpto, string LeaveFromTime, string LeaveUptoTime, Int64 UserId, Int64 LeaveTypText)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetLeaveDetailCalculation(LeaveFrom, LeaveUpto, LeaveFromTime, LeaveUptoTime, UserId, LeaveTypText);
            }
        }

        public string ReturnWeekOff(long UserId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.ReturnWeekOff(UserId);
            }
        }

        public decimal ReturnDuration(DateTime LeaveFrom, DateTime LeaveUpto, string LeaveFromTime, string LeaveUptoTime, Int64 UserId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.ReturnDuration(LeaveFrom, LeaveUpto, LeaveFromTime, LeaveUptoTime, UserId);
            }
        }

        public IList<PermissionDetailsModel> GetPermissionDetail(Int64? paramUserId, string reqUsr, DateTime? startDate, DateTime? endDate, bool OnlyReportedToMe, Int64 LeadId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetPermissionDetail(paramUserId, reqUsr, startDate, endDate, OnlyReportedToMe, LeadId);
            }
        }

        public IList<PermissionDetailsModel> GetOverTimePermissionDetail(Int64? paramUserId, string reqUsr, DateTime? startDate, DateTime? endDate, bool OnlyReportedToMe, Int64 LeadId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetOverTimePermissionDetail(paramUserId, reqUsr, startDate, endDate, OnlyReportedToMe, LeadId);
            }
        }

        public DashBoardModel GetDashboardData(Int64 UserId, Int64 OfficeId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetDashboardData(UserId, OfficeId);
            }
        }

        public LeaveRequestModel ApplyLeaveCommonData(Int64 UserId, Int64 OfficeId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.ApplyLeaveCommonData(UserId, OfficeId);
            }
        }

        public IList<LeaveDetailModel> ShowLeaveDetail(Int64 LeaveId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.ShowLeaveDetail(LeaveId);
            }
        }

        public IList<EmployeeList> GetEmployeeList(string param, Int64 userId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetEmployeeList(param, userId);
            }
        }

        public List<TimeSheetModel> GetMyTeamTimeSheet(Int64 UserID)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetMyTeamTimeSheet(UserID);
            }
        }

        public int GetPendingApprovalCount(Int64 userId)
        {
            using (var dac = new LeaveDac())
            {
                return dac.GetPendingApprovalCount(userId);
            }
        }
    }
}