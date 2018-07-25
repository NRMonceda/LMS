using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Repository
{
    public interface ILeaveHelper : IDisposable
    {
        string SaveLeaveRequest(LeaveRequestModel request);

        List<LeaveTypesModel> GetLeaveTypes(Int64 OfficeId, Int64 userId);

        IList<EmployeeList> GetEmployeeList(string param, Int64 userId);

        List<DropDownItem> GetYearsFromLeaveBalance();

        IList<TeamLeaves> GetLeaveRequests(ManageTeamLeavesQueryModel qryMdl);

        string ChangeStatus(LeaveStatusModel status);

        //List<LeaveItem> GetLeaveHistory(Int64 UserId);
        IList<TeamLeaves> GetTeamLeaveHistory(ManageTeamLeavesQueryModel qryMdl);

        //List<LeaveItem> GetMyLeaveRequests(Int64 UserId);

        IList<LeaveSummary> GetLeaveSumary(Int64 UserId, Int32 summaryYear);

        IList<HolidayModel> GetHolidaysDetails(long UserId, Int32 holidayYear, ref bool previousYear, ref bool nextYear);

        IList<HolidayModel> GetHolidays(long UserId, Int32 holidayYear);

        IList<DropDownItem> GetWeekOffs(Int64 UserId);

        IList<EmployeeWiseLeaveSummaryModel> GetEmployeeWiseLeaveSumary(Int64 UserId, int Year, string reqUsr, Int64? paramUserId, bool OnlyReportedToMe);

        int GetHolidayCount(DateTime startDate, DateTime endDate, Int64 UserId);

        IList<DaywiseLeaveDtlModel> GetDaywiseLeaveDtl(DateTime? FromDate, DateTime? ToDate, bool IsLeaveOnly, Int64 LeadId, bool OnlyReportedToMe, Int64? paramUserId, string reqUsr, bool DonotShowRejected);

        IList<MonthwiseLeavesCountModel> GetMonthwiseLeavesCount(Int32 year, Int64 LeadId, bool OnlyReportedToMe, Int64? paramUserId, string reqUsr);

        IList<LeaveDtl> GetLeaveDetailCalculation(DateTime LeaveFrom, DateTime LeaveUpto, string LeaveFromTime, string LeaveUptoTime, Int64 UserId, Int64 LeaveTypText);

        string ReturnWeekOff(long UserId);

        DashBoardModel GetDashboardData(Int64 UserId, Int64 OfficeId);

        IList<PermissionDetailsModel> GetPermissionDetail(Int64? paramUserId, string reqUsr, DateTime? startDate, DateTime? endDate, bool OnlyReportedToMe, Int64 LeadId);

        IList<PermissionDetailsModel> GetOverTimePermissionDetail(Int64? paramUserId, string reqUsr, DateTime? startDate, DateTime? endDate, bool OnlyReportedToMe, Int64 LeadId);

        decimal ReturnDuration(DateTime LeaveFrom, DateTime LeaveUpto, string LeaveFromTime, string LeaveUptoTime, Int64 UserId);

        LeaveRequestModel ApplyLeaveCommonData(Int64 UserId, Int64 OfficeId);

        IList<LeaveDetailModel> ShowLeaveDetail(Int64 LeaveId);

        int GetPendingApprovalCount(Int64 userId);

        List<TimeSheetModel> GetMyTeamTimeSheet(Int64 UserID);

        EmailDataModel GetEmailData(Int64 leaveId, string actionName);

        //string ChangeStatusFromEmail(Int64 leaveId, Int64 userId, string action);

        EmailDataModel ViewLeaveFromEmail(Int64 leaveId, Int64 userId);
    }
}