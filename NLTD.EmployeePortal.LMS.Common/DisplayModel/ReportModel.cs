using System;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class lateAndEarlyRpt
    {
        public string Name { get; set; }
        public Int64? UserID { get; set; }
        public string InOut { get; set; }
        public DateTime MinInOutDate { get; set; }
        public DateTime MaxInOutDate { get; set; }
        public DateTime OutTime { get; set; }
        public TimeSpan WorkingHours { get; set; }
        public string Status { get; set; }

        public string LMSStatus { get; set; }
    }

    public class NoOfLateInMonth
    {
        public string Name { get; set; }
        public Int64 UserID { get; set; }
        public string EmpId { get; set; }
        public Int32 NoOfLate { get; set; }
        public string ReportingTo { get; set; }
    }

    public class ReportLateMonth
    {
        public string Name { get; set; }
        public Int64 UserID { get; set; }
        public string EmpId { get; set; }
        public string ReportingTo { get; set; }
        public TimeSpan? LateEntry { get; set; }
    }
}