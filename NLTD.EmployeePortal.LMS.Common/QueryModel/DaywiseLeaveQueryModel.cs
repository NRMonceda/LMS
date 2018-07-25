using System;

namespace NLTD.EmployeePortal.LMS.Common.QueryModel
{
    public class DaywiseLeaveQueryModel
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool IsLeaveOnly { get; set; }

        public bool OnlyReportedToMe { get; set; }

        public string Name { get; set; }

        public string DateRange { get; set; }

        public string ErrorMsg { get; set; }

        public bool DonotShowRejected { get; set; }

        public Int64? SearchUserID { get; set; }
    }
}