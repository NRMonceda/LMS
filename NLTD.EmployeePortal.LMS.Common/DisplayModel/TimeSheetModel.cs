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
        public string HolidayStatus { get; set; }
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

        public decimal WorkFromHomeDayQty { get; set; }
    }

    public class WeeklyDateBlocksModel
    {
        public DateTime WeekDayStartDate { get; set; }

        public DateTime WeekDayEndDate { get; set; }
    }

    public class WeeklyTimeNotMaintainedModel
    {
        public TimeSpan TotalWeekWorkingHours { get; set; }

        public string TotalWeekWorkingHoursFormatted { get; set; }

        public string TotalWeekExpectedHoursFormatted { get; set; }
        public long UserId { get; set; }

        public DateTime WeekDayStartDate { get; set; }

        public DateTime WeekDayEndDate { get; set; }

        public string DateRange { get; set; }

        public decimal WorkFromHomeQty { get; set; }

        public string Permissions { get; set; }

        public decimal Requests { get; set; }

        public bool IsWeeklyTimeMet { get; set; }
    }

    public class DailyTimeNotMaintainedModel
    {
        public TimeSpan TotalDayWorkingHours { get; set; }

        public string TotalDayWorkingHoursFormatted { get; set; }

        public string ExpectedDayWorkingHoursFormatted { get; set; }
        public long UserId { get; set; }

        public DateTime WorkingDay { get; set; }

        public string WorkingDayText { get; set; }

        public string Shift { get; set; }

        public string InTime { get; set; }

        public string OutTime { get; set; }

        public string Status { get; set; }

        public string Request { get; set; }
    }

    public class UserEmailListModel
    {
        public long UserId { get; set; }

        public string EmployeeEmailAddress { get; set; }

        public string ReportingToEmailAddress { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Boolean SkipTimesheetCompliance { get; set; }
    }
}