using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.DbHelper;
using NLTD.EmployeePortal.LMS.Repository;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Client
{
    public class OfficeLocationClient : IOfficeLocationHelper
    {
        public void Dispose()
        {
            //Nothing to implement...
        }

        public List<DropDownItem> GetAllOfficeLocations()
        {
            IOfficeLocationHelper helper = new OfficeLocationHelper();
            return helper.GetAllOfficeLocations();
        }
    }
}