using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class EmployeeRole
    {
        public Int64 RoleId { get; set; }
        public String Role { get; set; }
        public Boolean IsAdmin { get; set; }
        public Boolean IsHR { get; set; }
        public Boolean IsActive { get; set; }
    }
}