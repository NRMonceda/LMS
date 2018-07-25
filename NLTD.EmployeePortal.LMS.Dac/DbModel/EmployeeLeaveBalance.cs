using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class EmployeeLeaveBalance
    {
        public Int64 LeaveBalanceId { get; set; }
        public Int64 LeaveTypeId { get; set; }
        public Decimal? TotalDays { get; set; }
        public Decimal? LeaveTakenDays { get; set; }

        public Decimal? PendingApprovalDays { get; set; }

        public Int64 UserId { get; set; }

        public Int32 Year { get; set; }

        public Decimal? BalanceDays { get; set; }

        public Int64? CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        public Int64? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}