using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class LeaveDetail
    {
        public Int64 LeaveDetailId { get; set; }

        public Int64 LeaveId { get; set; }

        public DateTime LeaveDate { get; set; }

        public decimal LeaveDayQty { get; set; }

        public bool IsDayOff { get; set; }

        public string Remarks { get; set; }

        public string PartOfDay { get; set; }
    }
}