using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class ShiftTransaction
    {
        public int ShiftTransactionID { get; set; }
        public int? ShiftID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? Createddate { get; set; }
        public Int64 CreatedBy { get; set; }
        public Int64 UserId { get; set; }
    }
}