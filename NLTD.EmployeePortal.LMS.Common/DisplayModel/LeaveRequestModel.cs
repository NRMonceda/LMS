using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class LeaveRequestModel
    {
        public string WeekOffs { get; set; }

        public string holidayDates { get; set; }

        public string TimebasedLeaveTypeIds { get; set; }

        //public string Holidays { get; set; }

        public Int64 RequestId { get; set; }
        public Int64 UserId { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime LeaveFrom { get; set; }

        public DateTime LeaveUpto { get; set; }
        public Int64 LeaveType { get; set; }
        public String LeaveFromTime { get; set; }
        public String LeaveUptoTime { get; set; }
        public decimal NumberOfDays { get; set; }
        public String Reason { get; set; }
        public String ManagersComment { get; set; }

        public string ErrorMesage { get; set; }

        public IList<LeaveSummary> lstSummary { get; set; }

        public IList<LeaveTypesModel> lstLeavTypes { get; set; }

        public IList<LeaveDtl> leaveDetail = new List<LeaveDtl>();

        public String ReportingToName { get; set; }

        public string IsTimeBased { get; set; }

        public string PermissionTime { get; set; }

        public string PermissionTimeFrom { get; set; }

        public string PermissionTimeTo { get; set; }

        public string ViewTitle { get; set; }

        public string ApplyMode { get; set; }

        public Int64 ApplyForUserId { get; set; }

        public Int64 AppliedByUserId { get; set; }
    }

    public class LeaveDtl
    {
        public Int64 LeaveId { get; set; }
        public DateTime LeaveDayItem { get; set; }

        public bool IsDayOff { get; set; }

        public decimal LeaveDayItemQty { get; set; }

        public string Remarks { get; set; }

        public Decimal Total { get; set; }

        public string PartOfDay { get; set; }
    }
}