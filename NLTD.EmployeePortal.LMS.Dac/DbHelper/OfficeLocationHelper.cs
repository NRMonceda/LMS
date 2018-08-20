using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.Dac;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Dac.DbHelper
{
    public class OfficeLocationHelper : IOfficeLocationHelper, IDisposable
    {
        public void Dispose()
        {
            //Nothing to implement...
        }

        public List<DropDownItem> GetAllOfficeLocations()
        {
            using (var dac = new OfficeLocationDac())
            {
                return dac.GetAllOfficeLocations();
            }
        }
    }
}