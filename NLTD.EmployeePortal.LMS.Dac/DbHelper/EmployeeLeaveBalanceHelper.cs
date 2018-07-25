using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.Dac;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Dac.DbHelper
{
    public class EmployeeLeaveBalanceHelper : IEmployeeLeaveBalanceHelper, IDisposable
    {
        public void Dispose()
        {
            //Nothing to implement...
        }

        public IList<LeaveBalanceEmpProfile> GetLeaveBalanceEmpProfile(Int64 UserId)
        {
            using (var dac = new EmployeeLeaveBalanceDac())
            {
                return dac.GetLeaveBalanceEmpProfile(UserId);
            }
        }

        public string UpdateLeaveBalance(List<EmployeeLeaveBalanceDetails> empLeaveBalanceDetails, Int64 UserId, Int64 LoginUserId)
        {
            using (var dac = new EmployeeLeaveBalanceDac())
            {
                return dac.UpdateLeaveBalance(empLeaveBalanceDetails, UserId, LoginUserId);
            }
        }
    }
}