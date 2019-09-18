using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class LeaveType
    {
        public Int64 OfficeId { get; set; }
        public Int64 LeaveTypeId { get; set; }
        public String Type { get; set; }
        public Int32? MaximumPerMonth { get; set; }

        public Int32? MaximumPerRequest { get; set; }

        public Int32? MaximumPerYear { get; set; }

        public bool AdjustLeaveBalance { get; set; }

        public string ApplicableGender { get; set; }

        public bool IsLeave { get; set; }

        public bool IsTimeBased { get; set; }

        public int SortOrder { get; set; }

        public DateTime? lastCreditRun { get; set; }

        public bool LeaveCombinationAllowed { get; set; }

        public DateTime Createdon { get; set; }
        public DateTime Modifiedon { get; set; }

        public long CreatedBy { get; set; }

        public long ModifiedBy { get; set; }


    }
}