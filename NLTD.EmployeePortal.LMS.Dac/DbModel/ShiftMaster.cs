using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class ShiftMaster
    {
        public int ShiftID { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }

        public Int64 CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
        public string ShiftDescription { get; set; }
    }
}