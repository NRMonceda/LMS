using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class LeaveTransactionHistory
    {
        public Int64 TransactionId { get; set; }
        public Int64 UserId { get; set; }

        public Int64 LeaveId { get; set; }
        public Int64 LeaveTypeId { get; set; }
        public DateTime TransactionDate { get; set; }
        public String TransactionType { get; set; }
        public Decimal NumberOfDays { get; set; }

        public Int64 TransactionBy { get; set; }

        public string Remarks { get; set; }
    }
}