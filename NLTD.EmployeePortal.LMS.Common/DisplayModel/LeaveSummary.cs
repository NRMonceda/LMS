using System;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class LeaveSummary
    {
        public string LeaveType { get; set; }

        public decimal TotalLeaves { get; set; }

        public decimal LeavesTaken { get; set; }

        public decimal LeavesPendingApproval { get; set; }

        public decimal LeavesBalance { get; set; }
        public Int64 LeaveTypeId { get; set; }

        public String Avatar { get; set; }

        public bool AdjustLeaveBalance { get; set; }

        public bool IsTimeBased { get; set; }
    }
}