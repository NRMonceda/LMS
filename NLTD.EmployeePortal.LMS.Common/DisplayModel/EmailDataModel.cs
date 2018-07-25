using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class EmailDataModel
    {
        public Int64 LeaveId { get; set; }
        public string RequestFor { get; set; }

        public string AuthError { get; set; }
        public string EmpId { get; set; }
        public string RequestorEmailId { get; set; }
        public IList<string> CcEmailIds { get; set; }

        public string ToEmailId { get; set; }

        public string LeaveTypeText { get; set; }

        public Int64? ReportingToId { get; set; }
        public string ReportingToName { get; set; }

        public bool IsTimeBased { get; set; }

        public string Date { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string FromType { get; set; }

        public string ToType { get; set; }

        public decimal NoOfDays { get; set; }
        public string Duration { get; set; }

        public string Reason { get; set; }

        public string ApproverComments { get; set; }

        public string ApproverAction { get; set; }
    }
}