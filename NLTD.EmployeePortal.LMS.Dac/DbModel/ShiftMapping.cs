using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class ShiftMapping
    {
        public int ShiftMappingID { get; set; }
        public Int64 UserID { get; set; }
        public int? ShiftID { get; set; }
        public DateTime ShiftDate { get; set; }
        public DateTime? Createddate { get; set; }
        public Int64? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Int64? ModifiedBy { get; set; }
    }
}