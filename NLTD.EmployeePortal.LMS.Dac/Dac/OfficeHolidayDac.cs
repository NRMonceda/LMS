using NLTD.EmployeePortal.LMS.Dac.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public class OfficeHolidayDac : IDisposable
    {
        public void Dispose()
        {
            //Nothing to implement...
        }

        public List<OfficeHoliday> GetOfficeHoliday(Int64 userID = 0)
        {
            List<OfficeHoliday> officeHolidayList = new List<OfficeHoliday>();
            using (var context = new NLTDDbContext())
            {
                officeHolidayList = (from oh in context.OfficeHoliday
                                     join e in context.Employee on oh.OfficeId equals e.OfficeHolidayId
                                     where e.UserId == userID || userID == 0
                                     select oh).ToList();
            }
            return officeHolidayList;
        }
    }
}