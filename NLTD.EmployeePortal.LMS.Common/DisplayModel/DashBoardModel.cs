using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class DashBoardModel
    {
        public IList<LeaveSummary> lstLeaveSummary { get; set; }

        public IList<HolidayModel> lstHolidayModel { get; set; }

        public IList<DropDownItem> lstWeekOffs { get; set; }

        public IList<TimeSheetModel> ListTimeSheetModel { get; set; }

        public int PendingApprovalCount { get; set; }
        public string UserRole { get; set; }
        public bool IsLMSApprover { get; set; }
        public int EmployeeCount { get; set; }

        public bool PreviousYear { get; set; }
        public bool NextYear { get; set; }
    }
}