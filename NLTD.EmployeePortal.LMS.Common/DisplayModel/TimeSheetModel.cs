using System;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class TimeSheetModel
    {
        public string Name { get; set; }
        public Int64 userID { get; set; }
        public string Shift { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public TimeSpan WorkingHours { get; set; }
        public string Status { get; set; }

        public string Requests { get; set; }

        public TimeSpan LateIn { get; set; }
        public TimeSpan EarlyOut { get; set; }

        public decimal LeaveDayQty { get; set; }

        public decimal permissionCountOfficial { get; set; }
        public decimal permissionCountPersonal { get; set; }

        public string ReportingManager { get; set; }

        public String StartDateType { get; set; }
        public String EndDateType { get; set; }

        public decimal PermissionCount { get; set; }

        public decimal WorkFromHomeDayQty    { get; set; }

    }
}