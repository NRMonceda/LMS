using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.DbHelper;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Client
{
    public class EmployeeLeaveBalanceClient : IEmployeeLeaveBalanceHelper
    {
        public void Dispose()
        {
            //Nothing to implement...
        }

        public IList<LeaveBalanceEmpProfile> GetLeaveBalanceEmpProfile(Int64 UserId)
        {
            using (IEmployeeLeaveBalanceHelper helper = new EmployeeLeaveBalanceHelper())
            {
                return helper.GetLeaveBalanceEmpProfile(UserId);
            }
        }

        public string UpdateLeaveBalance(List<EmployeeLeaveBalanceDetails> empLeaveBalanceDetails, Int64 UserId, Int64 LoginUserId)
        {
            using (IEmployeeLeaveBalanceHelper helper = new EmployeeLeaveBalanceHelper())
            {
                return helper.UpdateLeaveBalance(empLeaveBalanceDetails, UserId, LoginUserId);
            }
        }
    }
}