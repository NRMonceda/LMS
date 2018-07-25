using System;

namespace NLTD.EmployeePortal.LMS.Common.QueryModel
{
    public class TransactionHistoryModel
    {
        public Int64 EmployeeId { get; set; }

        public Int64 LeaveTypeId { get; set; }

        public Int64 LeaveId { get; set; }

        public DateTime TransactionDate { get; set; }

        public string TransactionType { get; set; }

        public decimal NumberOfDays { get; set; }

        public Int64 TransactionBy { get; set; }

        public string Remarks { get; set; }
    }
}