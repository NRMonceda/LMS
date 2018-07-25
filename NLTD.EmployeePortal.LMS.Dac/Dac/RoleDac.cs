using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public class RoleDac : IDisposable
    {
        public List<DropDownItem> GetAllRoles()
        {
            using (var context = new NLTDDbContext())
            {
                List<DropDownItem> RoleList = (from role in context.EmployeeRole
                                               select new DropDownItem
                                               {
                                                   Key = role.RoleId.ToString(),
                                                   Value = role.Role
                                               }).ToList();
                return RoleList;
            }
        }

        public Int64 GetRoleOfEmployee(Int64 EmployeeId)
        {
            using (var context = new NLTDDbContext())
            {
                var employee = context.Employee.Where(e => e.UserId == EmployeeId).FirstOrDefault();
                if (employee != null)
                    return employee.EmployeeRoleId.Value;
                else
                    return 0;
            }
        }

        public void Dispose()
        {
            //Nothing to implement...
        }
    }
}