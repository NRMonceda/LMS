using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Dac.DbHelper
{
    public class ShiftHelper : IShiftHelper, IDisposable
    {
        public void Dispose()
        {
            //Nothing to dispose...
        }

        public List<ShiftAllocation> GetShiftAllocation(Int64 UserId, string RequestMenuUser)
        {
            using (var dac = new ShiftDac())
            {
                return dac.GetShiftAllocation(UserId, RequestMenuUser);
            }
        }

        public List<ShiftEmployees> GetShiftDetailsForUsers(Int64 userId, string RequestMenuUser)
        {
            using (var dac = new ShiftDac())
            {
                return dac.GetShiftDetailsForUsers(userId, RequestMenuUser);
            }
        }

        public string SaveEmployeeShift(List<Int64> UserId, int Shift, DateTime FromDate, DateTime ToDate, Int64 MgrId)
        {
            using (var dac = new ShiftDac())
            {
                return dac.SaveEmployeeShift(UserId, Shift, FromDate, ToDate, MgrId);
            }
        }

        public string SaveShiftMaster(int shiftId, string shiftName, TimeSpan fromTime, TimeSpan ToTime, Int64 MgrId)
        {
            using (var dac = new ShiftDac())
            {
                return dac.SaveShiftMaster(shiftId, shiftName, fromTime, ToTime, MgrId);
            }
        }

        public List<Shifts> GetShiftMaster()
        {
            using (var dac = new ShiftDac())
            {
                return dac.GetShiftMaster();
            }
        }

        public Shifts GetShiftMasterWithId(Int64 shiftId)
        {
            using (var dac = new ShiftDac())
            {
                return dac.GetShiftMasterWithId(shiftId);
            }
        }

        public EmpShift GetEmployeeShiftDetails(Int64 UserId, string RequestMenuUser, long LeaduserId)
        {
            using (var dac = new ShiftDac())
            {
                return dac.GetEmployeeShiftDetails(UserId, RequestMenuUser, LeaduserId);
            }
        }

        public string SaveIndividualEmployeeShift(Int64 UserId, int Shift, DateTime FromDate, DateTime ToDate, Int64 MgrId)
        {
            using (var dac = new ShiftDac())
            {
                return dac.SaveIndividualEmployeeShift(UserId, Shift, FromDate, ToDate, MgrId);
            }
        }
    }
}