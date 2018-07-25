using System;

namespace NLTD.EmployeePortal.LMS.Common.QueryModel
{
    public class EmployeeProfileSearchModel
    {
        public string Name { get; set; }

        public string RequestLevelPerson { get; set; }

        public bool OnlyReportedToMe { get; set; }

        public bool HideInactiveEmp { get; set; }

        public Int64? SearchUserID { get; set; }
    }
}