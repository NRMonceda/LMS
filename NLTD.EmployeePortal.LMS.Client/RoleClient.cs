using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.DbHelper;
using NLTD.EmployeePortal.LMS.Repository;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Client
{
    public class RoleClient : IRoleHelper
    {
        public void Dispose()
        {
            //Nothing to implement...
        }

        public List<DropDownItem> GetAllRoles()
        {
            using (IRoleHelper helper = new RoleHelper())
            {
                return helper.GetAllRoles();
            }
        }
    }
}