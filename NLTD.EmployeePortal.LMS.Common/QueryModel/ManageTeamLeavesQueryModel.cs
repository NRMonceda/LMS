using System;
using System.ComponentModel;

namespace NLTD.EmployeePortal.LMS.Common.QueryModel
{
    public class ManageTeamLeavesQueryModel
    {
        public bool OnlyReportedToMe { get; set; }

        public bool ShowApprovedLeaves { get; set; }

        public bool IsLeaveOnly { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public Int64 LeadId { get; set; }

        public string RequestMenuUser { get; set; }

        [DisplayName("Date Range")]
        public string DateRange { get; set; }

        public string Name { get; set; }

        public string IsAuthorized { get; set; }

        public Int64? SearchUserID { get; set; }
    }
}