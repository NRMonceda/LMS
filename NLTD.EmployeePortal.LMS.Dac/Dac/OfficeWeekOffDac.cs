using System;
using System.Collections.Generic;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public class OfficeWeekOffDac : IDisposable
    {
        public void Dispose()
        {
            //Nothing to implement...
        }

        public List<string> GetEmployeeWeekOffDay(Int64 UserID)
        {
            List<string> weekOffDaysList = new List<string>();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    weekOffDaysList = (from ewf in context.EmployeeWeekOff
                                       join dw in context.DayOfWeek
                                        on ewf.DaysOfWeekId equals dw.DaysOfWeekId
                                       where ewf.UserId == UserID
                                       select dw.Day).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return weekOffDaysList;
        }
    }
}