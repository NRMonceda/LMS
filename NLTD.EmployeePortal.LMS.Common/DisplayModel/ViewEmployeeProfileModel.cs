using System;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class ViewEmployeeProfileModel
    {
        public Int64? ReportedToId { get; set; }
        public String LogonId { get; set; }

        public String EmployeeId { get; set; }
        public String FirstName { get; set; }

        public String LastName { get; set; }

        public Int64 UserId { get; set; }

        public String EmailAddress { get; set; }

        public String MobileNumber { get; set; }
        public String Gender { get; set; }

        public Int32 ShiftId { get; set; }
        public String Shift { get; set; }
        public Int64? CardId { get; set; }
        public String RoleText { get; set; }

        public String ReportedToName { get; set; }

        public String OfficeName { get; set; }
        public String HolidayOfficeName { get; set; }

        public bool Sunday { get; set; }

        public bool Monday { get; set; }

        public bool Tuesday { get; set; }

        public bool Wednesday { get; set; }

        public bool Thursday { get; set; }

        public bool Friday { get; set; }

        public bool Saturday { get; set; }

        public bool IsActive { get; set; }

        public Int64 HolidayOfficeId { get; set; }

        public string Name { get; set; }

        public String DOJ { get; set; }
        public String ConfirmationDate { get; set; }
        public String RelievingDate { get; set; }
        public Decimal? CurrentEL { get; set; }

        public Decimal? ELCredit { get; set; }

        public Decimal? NewELBalance { get; set; }

        public string lastCreditRun { get; set; }

        public Int64? LeaveBalanceId { get; set; }
    }
}