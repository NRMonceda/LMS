using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.DbHelper;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;

namespace NLTD.EmployeePortal.LMS.Client
{
    public class ShiftClient : IShiftHelper
    {
        public List<ShiftAllocation> GetShiftAllocation(Int64 userId, string RequestMenuUser)
        {
            using (IShiftHelper helper = new ShiftHelper())
            {
                return helper.GetShiftAllocation(userId, RequestMenuUser);
            }
        }

        public List<ShiftEmployees> GetShiftDetailsForUsers(Int64 userId, string RequestMenuUser)
        {
            using (IShiftHelper helper = new ShiftHelper())
            {
                return helper.GetShiftDetailsForUsers(userId, RequestMenuUser);
            }
        }

        public string SaveEmployeeShift(List<Int64> UserId, int Shift, DateTime FromDate, DateTime ToDate, Int64 MgrId)
        {
            using (IShiftHelper helper = new ShiftHelper())
            {
                return helper.SaveEmployeeShift(UserId, Shift, FromDate, ToDate, MgrId);
            }
        }

        public List<Shifts> GetShiftMaster()
        {
            using (IShiftHelper helper = new ShiftHelper())
            {
                return helper.GetShiftMaster();
            }
        }

        public Shifts GetShiftMasterWithId(Int64 shiftId)
        {
            using (IShiftHelper helper = new ShiftHelper())
            {
                return helper.GetShiftMasterWithId(shiftId);
            }
        }

        public string SaveShiftMaster(int shiftId, string shiftName, TimeSpan fromTime, TimeSpan toTime, Int64 MgrId)
        {
            using (IShiftHelper helper = new ShiftHelper())
            {
                return helper.SaveShiftMaster(shiftId, shiftName, fromTime, toTime, MgrId);
            }
        }

        public EmpShift GetEmployeeShiftDetails(Int64 UserId, string RequestMenuUser, long LeaduserId)
        {
            using (IShiftHelper helper = new ShiftHelper())
            {
                return helper.GetEmployeeShiftDetails(UserId, RequestMenuUser, LeaduserId);
            }
        }

        public string SaveIndividualEmployeeShift(Int64 UserId, int Shift, DateTime FromDate, DateTime ToDate, Int64 MgrId)
        {
            using (IShiftHelper helper = new ShiftHelper())
            {
                return helper.SaveIndividualEmployeeShift(UserId, Shift, FromDate, ToDate, MgrId);
            }
        }

        public void Dispose()
        {
            //Nothing to impement...
        }
    }
}