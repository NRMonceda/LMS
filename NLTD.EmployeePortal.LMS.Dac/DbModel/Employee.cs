using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class Employee
    {
        public Int64 UserId { get; set; }
        public String EmployeeId { get; set; }
        public String LoginId { get; set; }
        public bool IsActive { get; set; }
        public Int64 OfficeId { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String EmailAddress { get; set; }
        public String MobileNumber { get; set; }
        public String Gender { get; set; }
        public String AvatarUrl { get; set; }
        public Int64? ReportingToId { get; set; }
        public Int64 OfficeHolidayId { get; set; }
        public Int64? EmployeeRoleId { get; set; }

        //public Boolean IsHandleMembers { get; set; }
        public Boolean IsInProbationaryPeriod { get; set; }

        public Boolean CanAvailYearlyLeave { get; set; }

        public Int64 CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Int64 ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        public Int32? ShiftId { get; set; }
        public Int64? Cardid { get; set; }
        public DateTime? DOJ { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public DateTime? RelievingDate { get; set; }

        public int? AvailedLeavePolicyException { get; set; }

        public Boolean? SkipTimesheetCompliance { get; set; }
    }
}