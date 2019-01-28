using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class LeaveListModel
    {
        public long LeaveId { get; set; }
        public long LeaveDtlId { get; set; }

        public string PartOfDay { get; set; }

        public DateTime LeaveDate { get; set; }
    }
}
