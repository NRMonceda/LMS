using System;
using System.ComponentModel.DataAnnotations;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class ElCreditModel
    {
        public Int64 UserId { get; set; }
        public String EmployeeId { get; set; }

        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? DOJ { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? ConfirmationDate { get; set; }

        public Int64 CurrentEL { get; set; }

        public Int64 ELCredit { get; set; }

        public Int64 NewELBalance { get; set; }

        public Int64? LeaveBalanceId { get; set; }
    }
    public class LeaveCreditModel
    {
        public Int64 UserId { get; set; }

        public String EmployeeId { get; set; }

        public string Name { get; set; }
        
        public DateTime? DOJ { get; set; }
        
        public DateTime? ConfirmationDate { get; set; }

        public Int64 CurrentLeave { get; set; }

        public Int64 LeaveCredit { get; set; }

        public Int64 NewELBalance { get; set; }

        public Int64? LeaveBalanceId { get; set; }

        public Decimal? TotalDays { get; set; }
    }
}