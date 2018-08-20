using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class TeamLeaves
    {
        public Int64 UserId { get; set; }
        public string Name { get; set; }
        public IList<LeaveItem> TeamLeaveList { get; set; }
    }

    public class LeaveItem
    {
        public Int64 LeaveId { get; set; }
        public Int64 UserId { get; set; }
        public String RequesterName { get; set; }
        public String Avatar { get; set; }
        public DateTime LeaveFromDate { get; set; }
        public DateTime LeaveUptoDate { get; set; }
        public String LeaveFromType { get; set; }
        public String LeaveUptoType { get; set; }
        public String LeaveTypeText { get; set; }

        public bool IsLeave { get; set; }

        public bool isTimeBased { get; set; }

        public string PermissionInMonth { get; set; }
        public Decimal NumberOfDaysRequired { get; set; }
        public String Reason { get; set; }

        public String Comments { get; set; }
        public String Status { get; set; }
        public DateTime RequestDate { get; set; }

        public Int64? ApprovedById { get; set; }
        public String ApprovedByName { get; set; }
        public Int64? ReportingToId { get; set; }
        public String ReportingToName { get; set; }

        public DateTime? ApprovedDateFromLinq { get; set; }
        public DateTime ApprovedDate { get; set; }

        public string NoOfLeavesDisplay { get; set; }

        public string PermissionTime { get; set; }

        public string AppliedByName { get; set; }

        public Int64? AppliedById { get; set; }

        public long LeaveTypeId { get; set; }
    }
}