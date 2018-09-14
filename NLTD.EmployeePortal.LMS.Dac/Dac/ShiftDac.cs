using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.Dac;
using NLTD.EmployeePortal.LMS.Dac.DbModel;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac
{
    public class ShiftDac : IShiftHelper
    {
        public List<ShiftAllocation> GetShiftAllocation(Int64 UserId, string RequestMenuUser)
        {
            List<ShiftAllocation> lstShiftAllocation = new List<ShiftAllocation>();

            try
            {
                using (var context = new NLTDDbContext())
                {
                    if (RequestMenuUser == "Admin")
                    {
                        EmployeeDac employeeDac = new EmployeeDac();
                        string leadRole = employeeDac.GetEmployeeRole(UserId);

                        if (leadRole == "ADMIN" || leadRole == "HR")
                        {
                            lstShiftAllocation = (from sm in context.ShiftMapping
                                                  join s in context.ShiftMaster on sm.ShiftID equals s.ShiftID
                                                  join e in context.Employee on sm.UserID equals e.UserId
                                                  where e.IsActive == true
                                                  select new ShiftAllocation
                                                  {
                                                      Name = e.FirstName + " " + e.LastName,
                                                      EmpId = e.EmployeeId,
                                                      UserId = sm.UserID,
                                                      ShiftID = s.ShiftID,
                                                      ShiftDate = sm.ShiftDate,
                                                      FromTime = s.FromTime,
                                                      ToTime = s.ToTime,
                                                      ShiftName = s.ShiftDescription,
                                                      IsActive = e.IsActive,
                                                      ShiftMappingID = sm.ShiftMappingID
                                                  }).ToList();
                        }
                    }
                    else
                    {
                        lstShiftAllocation = GetShiftEmployees(context, UserId);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstShiftAllocation;
        }

        public List<Shifts> GetShiftMaster()
        {
            List<Shifts> lstShift = new List<Shifts>();

            try
            {
                using (var context = new NLTDDbContext())
                {
                    lstShift = (from s in context.ShiftMaster.AsEnumerable()
                                select new Shifts
                                {
                                    ShiftId = s.ShiftID,
                                    ShiftName = s.ShiftDescription,
                                    FromTime = s.FromTime,
                                    ToTime = s.ToTime,
                                    Shift = string.Format("{0:hh\\:mm}", s.FromTime) + " - " + string.Format("{0:hh\\:mm}", s.ToTime)
                                }).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstShift;
        }

        public List<ShiftEmployees> GetShiftDetailsForUsers(Int64 UserId, string RequestMenuUser)
        {
            var lstShiftEmployees = new List<ShiftEmployees>();

            try
            {
                using (var context = new NLTDDbContext())
                {
                    EmployeeDac employeeDac = new EmployeeDac();
                    string leadRole = employeeDac.GetEmployeeRole(UserId);

                    if (leadRole == "ADMIN" || leadRole == "HR")
                    {
                        lstShiftEmployees = (from e in context.Employee
                                             where e.IsActive == true
                                             select new ShiftEmployees
                                             {
                                                 Name = e.FirstName + " " + e.LastName,
                                                 EmpId = e.EmployeeId,
                                                 UserId = e.UserId
                                             }).ToList();
                    }
                    else
                    {
                        lstShiftEmployees = GetEmployees(context, UserId);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lstShiftEmployees;
        }

        public string SaveEmployeeShift(List<Int64> UserId, int Shift, DateTime FromDate, DateTime ToDate, Int64 MgrId)
        {
            try
            {
                int isSaved = 0;

                using (var context = new NLTDDbContext())
                {
                    var shiftMapping = context.ShiftMapping.Where(c => UserId.Contains(c.UserID) && (c.ShiftDate >= FromDate && c.ShiftDate <= ToDate)).ToList();
                    shiftMapping.ForEach(u =>
                    {
                        u.ShiftID = Shift;
                        u.ModifiedBy = MgrId;
                        u.ModifiedDate = DateTime.Now;
                    });

                    isSaved = context.SaveChanges();

                    var shiftTransaction = from id in UserId
                                           select new ShiftTransaction
                                           {
                                               ShiftID = Shift,
                                               UserId = id,
                                               CreatedBy = MgrId,
                                               Createddate = DateTime.Now,
                                               FromDate = FromDate,
                                               ToDate = ToDate
                                           };

                    context.ShiftTransaction.AddRange(shiftTransaction);
                    isSaved = context.SaveChanges();
                }

                return isSaved > 0 ? "Saved" : "Failed";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private List<ShiftEmployees> GetEmployees(NLTDDbContext context, Int64 UserId)
        {
            var result = new List<ShiftEmployees>();

            try
            {
                var employees = (from e in context.Employee
                                 where e.ReportingToId == UserId && e.IsActive == true
                                 select new ShiftEmployees
                                 {
                                     Name = e.FirstName + " " + e.LastName,
                                     EmpId = e.EmployeeId,
                                     UserId = e.UserId
                                 }).ToList();

                foreach (var employee in employees)
                {
                    result.Add(employee);
                    result.AddRange(GetEmployees(context, employee.UserId));
                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        private List<ShiftAllocation> GetShiftEmployees(NLTDDbContext context, Int64 UserId)
        {
            var result = new List<ShiftAllocation>();

            try
            {
                var emp = (from e in context.Employee
                           where e.ReportingToId == UserId && e.IsActive == true
                           select e.UserId).ToList();

                var employees = (from sm in context.ShiftMapping
                                 join s in context.ShiftMaster on sm.ShiftID equals s.ShiftID
                                 join e in context.Employee on sm.UserID equals e.UserId
                                 where e.ReportingToId == UserId && e.IsActive == true
                                 select new ShiftAllocation
                                 {
                                     Name = e.FirstName + " " + e.LastName,
                                     EmpId = e.EmployeeId,
                                     UserId = sm.UserID,
                                     ShiftID = s.ShiftID,
                                     ShiftDate = sm.ShiftDate,
                                     FromTime = s.FromTime,
                                     ToTime = s.ToTime,
                                     ShiftName = s.ShiftDescription,
                                     IsActive = e.IsActive,
                                     ShiftMappingID = sm.ShiftMappingID
                                 }).ToList();

                if (employees != null && employees.Count > 0)
                    result.AddRange(employees);

                foreach (Int64 id in emp)
                {
                    result.AddRange(GetShiftEmployees(context, id));
                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public Shifts GetShiftMasterWithId(Int64 shiftId)
        {
            var objShifts = new Shifts();

            try
            {
                using (var context = new NLTDDbContext())
                {
                    if (shiftId > 0)
                    {
                        objShifts = (from s in context.ShiftMaster
                                     where s.ShiftID == shiftId
                                     select new Shifts
                                     {
                                         ShiftId = s.ShiftID,
                                         ShiftName = s.ShiftDescription,
                                         FromTime = s.FromTime,
                                         ToTime = s.ToTime
                                     }).SingleOrDefault();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return objShifts;
        }

        public string SaveShiftMaster(int shiftId, string shiftName, TimeSpan fromTime, TimeSpan toTime, Int64 mgrId)
        {
            try
            {
                int isSaved = 0;

                using (var context = new NLTDDbContext())
                {
                    var exists = context.ShiftMaster.Where(c => c.FromTime == fromTime && c.ToTime == toTime && c.ShiftID != shiftId).ToList();
                    if (exists != null && exists.Count > 0)
                    {
                        return "Shift Already Available.";
                    }
                    else
                    {
                        var objShiftMaster = context.ShiftMaster.Where(c => c.ShiftID == shiftId).SingleOrDefault();
                        if (objShiftMaster != null)
                        {
                            objShiftMaster = context.ShiftMaster.Where(c => c.ShiftID == shiftId).Single();
                            objShiftMaster.ShiftID = shiftId;
                            objShiftMaster.ShiftDescription = shiftName;
                            objShiftMaster.FromTime = fromTime;
                            objShiftMaster.ToTime = toTime;
                        }
                        else
                        {
                            objShiftMaster = new ShiftMaster();
                            objShiftMaster.ShiftDescription = shiftName;
                            objShiftMaster.FromTime = fromTime;
                            objShiftMaster.ToTime = toTime;
                            objShiftMaster.CreatedBy = mgrId;
                            objShiftMaster.CreatedDate = DateTime.Now;
                        }
                        context.ShiftMaster.AddOrUpdate(objShiftMaster);
                        isSaved = context.SaveChanges();
                    }
                }

                return isSaved > 0 ? "Saved" : "Failed";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public EmpShift GetEmployeeShiftDetails(Int64 UserId, string RequestMenuUser, long LeaduserId)
        {
            EmpShift retModel = new EmpShift();

            try
            {
                using (var context = new NLTDDbContext())
                {
                    EmployeeDac employeeDac = new EmployeeDac();
                    long userId = 0; string EmpId = "";
                    string Name = "";

                    if (RequestMenuUser != "My")
                    {
                        var empPrf = context.Employee
                            .Where(x => x.UserId == UserId)
                            .FirstOrDefault();
                        if (empPrf != null)
                        {
                            userId = empPrf.UserId;
                            EmpId = empPrf.EmployeeId;
                            Name = empPrf.FirstName + " " + empPrf.LastName;
                        }
                    }
                    else
                    {
                        var empPrf = context.Employee.Where(x => (x.UserId) == LeaduserId).FirstOrDefault();
                        if (empPrf != null)
                        {
                            userId = empPrf.UserId;
                            EmpId = empPrf.EmployeeId;
                            Name = empPrf.FirstName + " " + empPrf.LastName;
                        }
                    }

                    if (userId > 0 || (RequestMenuUser == "My" && LeaduserId > 0))
                    {
                        string ReportingTo = (RequestMenuUser == "My" && LeaduserId > 0) ? employeeDac.ReportingToName(LeaduserId) : employeeDac.ReportingToName(userId);

                        string leadRole = employeeDac.GetEmployeeRole(LeaduserId);

                        List<ShiftAllocation> shiftDetails = new List<ShiftAllocation>();
                        if (RequestMenuUser == "My")
                        {
                            shiftDetails = getShiftDetails(context, LeaduserId);
                        }
                        else if (leadRole == "ADMIN" || leadRole == "HR")
                        {
                            shiftDetails = getShiftDetails(context, userId);
                        }
                        else if (RequestMenuUser == "Team")
                        {
                            var user = (from e in context.Employee
                                        where e.ReportingToId == LeaduserId
                                        select e).ToList();

                            var found = LeaveTransactionHistoryDac.FindControlRecursively(user, userId);
                            if (found != null)
                            {
                                shiftDetails = getShiftDetails(context, userId);
                            }
                        }

                        var groupedLeaveList = shiftDetails.GroupBy(u => u.Month)
                                                              .Select(grp => new { Month = grp.Key, shiftAllocation = grp.ToList() })
                                                              .ToList();

                        List<ShiftDetail> lstshiftDetails = (from gv in groupedLeaveList
                                                             select new ShiftDetail
                                                             {
                                                                 Month = gv.Month,
                                                                 shiftAllocation = gv.shiftAllocation
                                                             }).ToList();
                        retModel.shiftDetail = lstshiftDetails;
                        retModel.ReportingTo = ReportingTo;

                        var lstShift = context.ShiftMaster.AsEnumerable().OrderBy(x => x.FromTime).Select(s => new Shifts
                        {
                            ShiftId = s.ShiftID,
                            ShiftName = string.Format("{0:hh\\:mm}", s.FromTime) + " - " + string.Format("{0:hh\\:mm}", s.ToTime),
                        }).ToList();

                        retModel.Shifts = lstShift;
                    }

                    retModel.Name = Name;
                    retModel.EmpId = EmpId;
                    retModel.UserId = userId;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return retModel;
        }

        public List<ShiftAllocation> getShiftDetails(NLTDDbContext context, long UserId)
        {
            var shiftDetails = (from sm in context.ShiftMapping.AsEnumerable()
                                join s in context.ShiftMaster.AsEnumerable() on sm.ShiftID equals s.ShiftID
                                join e in context.Employee.AsEnumerable() on sm.UserID equals e.UserId
                                where e.UserId == UserId && e.IsActive == true && sm.ShiftDate.Year == DateTime.Now.Year
                                select new ShiftAllocation
                                {
                                    Month = sm.ShiftDate.ToString("MMMM"),
                                    Year = sm.ShiftDate.Year,
                                    UserId = sm.UserID,
                                    ShiftID = s.ShiftID,
                                    ShiftDate = sm.ShiftDate,
                                    FromTime = s.FromTime,
                                    ToTime = s.ToTime,
                                    ShiftName = s.ShiftDescription,
                                    ShiftMappingID = sm.ShiftMappingID
                                }).ToList();
            return shiftDetails;
        }

        public string SaveIndividualEmployeeShift(Int64 UserId, int Shift, DateTime FromDate, DateTime ToDate, Int64 MgrId)
        {
            try
            {
                int isSaved = 0;
                using (var context = new NLTDDbContext())
                {
                    var shiftMapping = context.ShiftMapping.Where(c => c.UserID == UserId && (c.ShiftDate >= FromDate && c.ShiftDate <= ToDate)).ToList();
                    shiftMapping.ForEach(u =>
                    {
                        u.ShiftID = Shift;
                        u.ModifiedBy = MgrId;
                        u.ModifiedDate = DateTime.Now;
                    });

                    isSaved = context.SaveChanges();

                    ShiftTransaction shiftTransaction = new ShiftTransaction();
                    shiftTransaction.ShiftID = Shift;
                    shiftTransaction.UserId = UserId;
                    shiftTransaction.CreatedBy = MgrId;
                    shiftTransaction.Createddate = DateTime.Now;
                    shiftTransaction.FromDate = FromDate;
                    shiftTransaction.ToDate = ToDate;
                    context.ShiftTransaction.Add(shiftTransaction);
                    isSaved = context.SaveChanges();
                }

                return isSaved > 0 ? "Saved" : "Failed";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void Dispose()
        {
            //Nothing to implement...
        }
    }
}