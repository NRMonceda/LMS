using System;

namespace NLTD.EmployeePortal.LMS.Common.QueryModel
{
    public class PermissionQueryModel
    {
        public string Name { get; set; }

        public string EmpId { get; set; }

        public Int64 UserId { get; set; }

        public int Year { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool IsLeaveOnly { get; set; }

        public bool OnlyReportedToMe { get; set; }

        public string DateRange { get; set; }

        public string ErrorMsg { get; set; }

        public Int64? SearchUserID { get; set; }
    }
}