using System;

namespace NLTD.EmployeePortal.LMS.Common.QueryModel
{
    public class EmployeeAttendanceQueryModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Name { get; set; }
        public Int64 UserID { get; set; }
        public string ErrorMsg { get; set; }
        public string DateRange { get; set; }
        public string RequestLevelPerson { get; set; }

        public bool DirectEmployees { get; set; }

        public Int64? CardID { get; set; }
    }
}