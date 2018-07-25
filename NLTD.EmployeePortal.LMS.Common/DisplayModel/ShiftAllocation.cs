using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Common.DisplayModel
{
    public class ShiftAllocation
    {
        public string EmpId { get; set; }
        public string Name { get; set; }
        public Int64 UserId { get; set; }
        public Int64? ShiftID { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public string ShiftName { get; set; }
        public bool IsActive { get; set; }
        public Int64 ShiftMappingID { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
    }

    public class ShiftEmployees
    {
        public string EmpId { get; set; }
        public string Name { get; set; }
        public Int64 UserId { get; set; }
        public Int64 ShiftId { get; set; }
        public DateTime ShiftDate { get; set; }
        public Int64? ShiftMappingID { get; set; }
    }

    public class Shifts
    {
        public Int64 ShiftId { get; set; }
        public Int64 CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public string ShiftName { get; set; }
        public string Shift { get; set; }
    }

    public class SaveShiftEmployee
    {
        public List<int> UserId { get; set; }
        public Int64 Shift { get; set; }
        public DateTime Fromdate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class ShiftDetail
    {
        public string Month { get; set; }
        public List<ShiftAllocation> shiftAllocation { get; set; }
    }

    public class EmpShift
    {
        public string Name { get; set; }
        public string EmpId { get; set; }
        public string ReportingTo { get; set; }
        public List<ShiftDetail> shiftDetail { get; set; }
        public List<Shifts> Shifts { get; set; }
        public Int64 UserId { get; set; }
    }

    public class EmployeeShifts
    {
        public List<ShiftEmployees> shiftEmployees { get; set; }
        public List<Shifts> Shifts { get; set; }
    }
}