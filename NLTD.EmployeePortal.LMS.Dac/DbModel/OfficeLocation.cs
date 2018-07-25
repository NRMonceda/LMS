using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class OfficeLocation
    {
        public Int64 OfficeId { get; set; }
        public String OfficeName { get; set; }
        public Boolean IsActive { get; set; }
    }
}