using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class Leave
    {
        public Int64 LeaveId { get; set; }
        public Int64 UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public String StartDateType { get; set; }
        public String EndDateType { get; set; }
        public Int64 LeaveTypeId { get; set; }
        public Decimal Duration { get; set; }
        public String Remarks { get; set; }
        public String Comments { get; set; }
        public String Status { get; set; }
        public Int64 AppliedBy { get; set; }
        public DateTime AppliedAt { get; set; }
        public Int64? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public bool? IsExceptionLeave { get; set; }
    }
}