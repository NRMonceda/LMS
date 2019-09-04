using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class EmployeeProfile
    {
        [Display(Name = "Corp Id")]
        public String LogonId { get; set; }

        [Required(ErrorMessage = "Enter Employee Id.")]
        [Display(Name = "Employee Id")]
        public String EmployeeId { get; set; }

        public Int64 UserId { get; set; }

        [Required(ErrorMessage = "Enter First Name.")]
        [Display(Name = "First Name")]
        public String FirstName { get; set; }

        [Required(ErrorMessage = "Enter Last Name.")]
        [Display(Name = "Last Name")]
        public String LastName { get; set; }

        [Display(Name = "Email")]
        public String EmailAddress { get; set; }

        [Display(Name = "Mobile Number")]
        [StringLength(10, ErrorMessage = "Should not exceed 10 characters.")]
        public String MobileNumber { get; set; }

        [Display(Name = "Gender")]
        [Required(ErrorMessage = "Select Gender.")]
        public String Gender { get; set; }

        [Display(Name = "LMS Role")]
        public Int64? RoleId { get; set; }

        public String RoleText { get; set; }

        [Display(Name = "Reporting To")]
        public Int64? ReportedToId { get; set; }

        public String ReportedToName { get; set; }

        [Required(ErrorMessage = "Select Holiday Calendar.")]
        public Int64 OfficeHolidayId { get; set; }

        public Int64 OfficeId { get; set; }
        public String LocationText { get; set; }
        public Int64 EmploymentTypeId { get; set; }

        public Int32? ShiftId { get; set; }

        public Int64? CardId { get; set; }
        public String Avatar { get; set; }

        [Display(Name = "Is LMS Approver")]
        public Boolean IsHandleMembers { get; set; }

        [Required(ErrorMessage = "Enter Joining Date.")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? DOJ { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? ConfirmationDate { get; set; }

        public DateTime? RelievingDate { get; set; }
        public bool Sunday { get; set; }

        public bool Monday { get; set; }

        public bool Tuesday { get; set; }

        public bool Wednesday { get; set; }

        public bool Thursday { get; set; }

        public bool Friday { get; set; }

        public bool Saturday { get; set; }

        public bool IsActive { get; set; }

        public IList<string> WeekOffs { get; set; }

        public string ErrorMesage { get; set; }

        public string Mode { get; set; }

        public Int64 CurrentEL { get; set; }

        public Int64 ELCredit { get; set; }

        public Int64 NewELBalance { get; set; }

        public string Name { get; set; }

        public Int64? LeaveBalanceId { get; set; }
    }

    public class TeamEmpProfile
    {
        public string Name { get; set; }

        public IList<EmployeeProfile> lstEmpProfiles { get; set; }
    }
}