using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class EmployeeAttendance
    {
        public Int64 ID { get; set; }
        public Int64? UserID { get; set; }
        public Int64 CardID { get; set; }
        public DateTime InOutDate { get; set; }
        public bool InOut { get; set; }
    }
}