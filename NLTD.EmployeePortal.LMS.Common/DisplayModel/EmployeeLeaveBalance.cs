using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class EmployeeLeaveBalanceProfile
    {
        public String LogonId { get; set; }
        public String EmployeeId { get; set; }
        public Int64 UserId { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Int64? ReportedToId { get; set; }
        public String ReportedToName { get; set; }
    }

    public class EmployeeLeaveBalanceDetails
    {
        public Int64? OfficeId { get; set; }
        public Int64? LeaveTypeId { get; set; }
        public String Type { get; set; }
        public bool? AdjustLeaveBalance { get; set; }
        public Int64? LeaveBalanceId { get; set; }
        public Decimal? ExistingTotalDays { get; set; }
        public Int64? UserId { get; set; }
        public Int32? Year { get; set; }
        public Decimal? BalanceDays { get; set; }
        public Decimal? TotalDays { get; set; }

        [Required(ErrorMessage = "Enter No Of Days.")]
        [Display(Name = "No of Days")]
        public Decimal NoOfDays { get; set; }

        public String CreditOrDebit { get; set; }
        public string Remarks { get; set; }

        public String EmployeeId { get; set; }
    }

    public class LeaveBalanceEmpProfile
    {
        public string Name { get; set; }
        public EmployeeLeaveBalanceProfile employeeLeaveBalanceProfile { get; set; }
        public IList<EmployeeLeaveBalanceDetails> lstEmployeeLeaveBalance { get; set; }
    }
}