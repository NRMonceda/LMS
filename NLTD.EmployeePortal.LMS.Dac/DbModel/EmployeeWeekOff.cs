using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class EmployeeWeekOff
    {
        public Int64 EmployeeWeekOffId { get; set; }

        public Int64 UserId { get; set; }

        public Int64 DaysOfWeekId { get; set; }

        public Int64 CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Int64 ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}