using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public class OfficeLocationDac : IDisposable
    {
        public List<DropDownItem> GetAllOfficeLocations()
        {
            using (var context = new NLTDDbContext())
            {
                List<DropDownItem> OfficeLocations = (from office in context.OfficeLocation
                                                      select new DropDownItem
                                                      {
                                                          Key = office.OfficeId.ToString(),
                                                          Value = office.OfficeName
                                                      }).ToList();
                return OfficeLocations;
            }
        }

        public void Dispose()
        {
            //Nothing to dispose...
        }
    }
}