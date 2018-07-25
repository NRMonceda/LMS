using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Repository
{
    public interface ILeaveTransactionHisotryHelper : IDisposable
    {
        IList<LeaveTransactionDetail> GetTransactionLog(string Name, string RequestMenuUser, long Userid);
    }
}