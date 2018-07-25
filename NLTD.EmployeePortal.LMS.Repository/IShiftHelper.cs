using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Repository
{
    public interface IShiftHelper : IDisposable
    {
        // For Maintain the shift master
        List<Shifts> GetShiftMaster();

        Shifts GetShiftMasterWithId(Int64 shiftId);

        string SaveShiftMaster(int shiftId, string shiftName, TimeSpan fromTime, TimeSpan toTime, Int64 MgrId);

        // For Assign the shifts
        List<ShiftAllocation> GetShiftAllocation(Int64 userId, string RequestMenuUser);

        List<ShiftEmployees> GetShiftDetailsForUsers(Int64 userId, string RequestMenuUser);

        string SaveEmployeeShift(List<Int64> UserId, int Shift, DateTime FromDate, DateTime ToDate, Int64 MgrId);

        EmpShift GetEmployeeShiftDetails(Int64 UserId, string RequestMenuUser, long LeaduserId);

        string SaveIndividualEmployeeShift(Int64 UserId, int Shift, DateTime FromDate, DateTime ToDate, Int64 MgrId);
    }
}