using System;

namespace NLTD.EmployeePortal.LMS.Common.QueryModel
{
    public class YearwiseLeaveSummaryQueryModel
    {
        public string Name { get; set; }

        public string ErrorMsg { get; set; }

        public int Year { get; set; }

        public bool OnlyReportedToMe { get; set; }

        public Int64? SearchUserID { get; set; }
    }
}