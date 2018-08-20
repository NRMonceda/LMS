using System;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class CalendarEventItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public bool IsAllDayEvent { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}