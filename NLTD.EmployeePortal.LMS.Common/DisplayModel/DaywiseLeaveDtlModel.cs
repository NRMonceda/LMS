using System;
using System.ComponentModel.DataAnnotations;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class DaywiseLeaveDtlModel
    {
        public Int64 UserId { get; set; }

        public string EmpId { get; set; }

        public string Name { get; set; }

        public string LeaveType { get; set; }

        public Int64 LeaveTypeId { get; set; }

        public bool IsLeave { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime LeaveDate { get; set; }

        public string PartOfDay { get; set; }

        public bool IsDayOff { get; set; }

        public decimal Duration { get; set; }

        public string LeaveStatus { get; set; }

        public Int64? ReportingToId { get; set; }

        public bool AdjustLeaveBalance { get; set; }

        public string LeaveReason { get; set; }

        public string ApproverComments { get; set; }

        public string ReasonShort { get; set; }

        public string CommentsShort { get; set; }
    }
}