using System;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class EmployeeAttendanceModel
    {
        public string Name { get; set; }
        public Int64? UserID { get; set; }

        public Int64 CardID { get; set; }
        public DateTime InOutDate { get; set; }

        public string AttendanceDate { get; set; }
        public string InOutTime { get; set; }
        public string InOut { get; set; }

        public string ErrorMessage { get; set; }

        public string requestLevelPerson { get; set; }

        public string BreakDuration { get; set; }
    }
}