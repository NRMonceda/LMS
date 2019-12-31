using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class EmploymentType
    {
        public Int64 EmploymentTypeId { get; set; }
        public string Code { get; set; }
        public string EmployeeIdPrefix { get; set; }
        public string Description { get; set; }
        public Int64 CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Int64 ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}