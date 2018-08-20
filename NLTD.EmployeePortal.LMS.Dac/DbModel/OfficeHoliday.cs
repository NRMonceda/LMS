using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class OfficeHoliday
    {
        public Int64 OfficeHolodayId { get; set; }
        public Int64 OfficeId { get; set; }
        public String Title { get; set; }
        public DateTime Holiday { get; set; }

        public Int32 Year { get; set; }
    }
}