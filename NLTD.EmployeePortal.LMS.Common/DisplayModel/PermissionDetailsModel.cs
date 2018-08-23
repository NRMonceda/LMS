using System;
using System.ComponentModel.DataAnnotations;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class PermissionDetailsModel
    {
        public string EmpId { get; set; }

        public Int64 UserId { get; set; }

        public Int64? ReportingToId { get; set; }
        public string Name { get; set; }
        public Int64 PermissionDetailId { get; set; }

        public Int64 LeaveId { get; set; }

        public string PermissionType { get; set; }

        public int PermissionMonth { get; set; }

        public string Month { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime PermissionDate { get; set; }

        public string TimeFrom { get; set; }

        public string TimeTo { get; set; }

        public string Duration
        {
            get; set;
        }
        public string Reason { get; set; }

        public string Status { get; set; }

        public string ApproverComments { get; set; }

        public string ReasonShort { get; set; }

        public string CommentsShort { get; set; }

        public string ReportingManager { get; set; }
    }
}