using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class LeaveHeaderModel
    {
        public Int64 LeaveId { get; set; }

        public DateTime StartDate { get; set; }

        public string StartDateType { get; set; }

        public DateTime EndDate { get; set; }

        public string EndDateType { get; set; }

        public Int64 LeaveTypeId { get; set; }

        public string LeaveTypeName { get; set; }

        public decimal Duration { get; set; }

        public string Status { get; set; }

        public IList<LeaveDetailModel> lstDetail { get; set; }
    }

    public class LeaveDetailModel
    {
        public Int64 LeaveId { get; set; }

        public Int64 LeaveDetailId { get; set; }

        public DateTime LeaveDayItem { get; set; }

        public bool IsDayOff { get; set; }

        public decimal LeaveDayItemQty { get; set; }

        public string Remarks { get; set; }
    }
}