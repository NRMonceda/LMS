using System;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class EmployeeWiseLeaveSummaryModel
    {
        public Int64 UserId { get; set; }

        public string EmpID { get; set; }

        public string Name { get; set; }

        public string LeaveType { get; set; }

        public decimal TotalLeaves { get; set; }

        public decimal UsedLeaves { get; set; }

        public decimal PendingApproval { get; set; }

        public decimal BalanceLeaves { get; set; }

        public bool AdjustLeaveBalance { get; set; }

        public Int64? ReportingToId { get; set; }
    }
}