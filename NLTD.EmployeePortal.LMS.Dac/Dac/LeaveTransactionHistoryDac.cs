using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public class LeaveTransactionHistoryDac : IDisposable
    {
        public void Dispose()
        {
            //Nothing to dispose..
        }

        public IList<LeaveTransactionDetail> GetTransactionLog(string Name, string RequestMenuUser, long LeaduserId)
        {
            IList<LeaveTransactionDetail> retModel = new List<LeaveTransactionDetail>();
            LeaveDac lv = new LeaveDac();
            IList<Int64> empList = lv.GetEmployeesReporting(LeaduserId);
            try
            {
                using (var context = new NLTDDbContext())
                {
                    EmployeeDac employeeDac = new EmployeeDac();
                    long userId = 0;

                    if (RequestMenuUser != "My")
                        userId = employeeDac.GetUserId(Name);

                    if (userId > 0 || (RequestMenuUser == "My" && LeaduserId > 0))
                    {
                        string ReportingTo = (RequestMenuUser == "My" && LeaduserId > 0) ? employeeDac.ReportingToName(LeaduserId) : employeeDac.ReportingToName(userId);

                        List<LeaveTransactionHistoryModel> transactionDetails = new List<LeaveTransactionHistoryModel>();
                        if (RequestMenuUser == "My")
                        {
                            transactionDetails = getTransactionDetails(context, LeaduserId);
                        }
                        if (RequestMenuUser == "Team")
                        {
                            var leadinfo = (from emp in context.Employee
                                            join role in context.EmployeeRole on emp.EmployeeRoleId equals role.RoleId
                                            where emp.UserId == LeaduserId
                                            select new { RoleName = role.Role }).FirstOrDefault();

                            if (leadinfo.RoleName.ToUpper() == "ADMIN" || leadinfo.RoleName.ToUpper() == "HR")
                            {
                                transactionDetails = getTransactionDetails(context, userId);
                            }
                            else
                            {
                                var user = empList.Where(x => x == userId).FirstOrDefault();

                                //var found = FindControlRecursively(user, userId);

                                if (user > 0)
                                {
                                    transactionDetails = getTransactionDetails(context, userId);
                                }
                            }
                        }
                        
                        var groupedLeaveList = transactionDetails.GroupBy(u => u.LeaveTypeId)
                                                              .Select(grp => new { LeaveTypeId = grp.Key, leaveTransactionHistoryModel = grp.ToList() })
                                                              .ToList();

                        retModel = (from gv in groupedLeaveList
                                    select new LeaveTransactionDetail
                                    {
                                        ReportingTo = ReportingTo,
                                        LeaveTypeId = gv.LeaveTypeId,
                                        LeaveType = gv.leaveTransactionHistoryModel[0].Type,
                                        leaveTransactionHistoryModel = gv.leaveTransactionHistoryModel
                                    }).ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return retModel;
        }

        public static Employee FindControlRecursively(List<Employee> root, Int64 id)
        {
            try
            {
                foreach (Employee emp in root)
                {
                    if (emp.UserId == id)
                        return emp;
                    using (var context = new NLTDDbContext())
                    {
                        var child = (from e in context.Employee
                                     where e.ReportingToId == emp.UserId
                                     select e).ToList();
                        Employee found = FindControlRecursively(child, id);
                        if (found != null)
                            return found;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        public List<LeaveTransactionHistoryModel> getTransactionDetails(NLTDDbContext context, long userId)
        {
            var transactionDetails = (from lth in context.LeaveTransactionHistory
                                      join lt in context.LeaveType on lth.LeaveTypeId equals lt.LeaveTypeId
                                      join l in context.Leave on lth.LeaveId equals l.LeaveId into ps
                                      from p in ps.DefaultIfEmpty()
                                      join e in context.Employee on lth.UserId equals e.UserId
                                      where lth.UserId == userId && lt.IsTimeBased == false
                                      select new LeaveTransactionHistoryModel
                                      {
                                          LeaveId = lth.LeaveId,
                                          LeaveTypeId = lth.LeaveTypeId,
                                          TransactionType = lth.TransactionType == "C" ? "Credit" : "Debit",
                                          NumberOfDays = lth.NumberOfDays,
                                          UserId = lth.UserId,
                                          Remarks = lth.Remarks,
                                          TransactionDate = lth.TransactionDate,
                                          Type = lt.Type,
                                          FirstName = e.FirstName,
                                          LastName = e.LastName,
                                          EmployeeId = e.EmployeeId,
                                          StartDate = p.StartDate,
                                          EndDate = p.EndDate
                                      }).ToList().OrderByDescending(x => x.TransactionDate).ToList();
            return transactionDetails;
        }

        public List<EmployeeLeave> GetLeaveForEmployee(Int64 UserID, DateTime FromDate, DateTime ToDate)
        {
            List<EmployeeLeave> leaveList = new List<EmployeeLeave>();
            LeaveDac lv = new LeaveDac();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    leaveList = (from l in context.Leave
                                 join lt in context.LeaveType on l.LeaveTypeId equals lt.LeaveTypeId
                                 join ld in context.LeaveDetail on l.LeaveId equals ld.LeaveId
                                 where l.UserId == UserID && l.Status == "A" && ld.IsDayOff == false && ld.LeaveDate >= FromDate && ld.LeaveDate <= ToDate
                                 select new EmployeeLeave
                                 {
                                     UserId = l.UserId,
                                     StartDate = ld.LeaveDate,
                                     EndDate = ld.LeaveDate,
                                     LeaveType = lt.Type,
                                     LeaveDayQty = ld.LeaveDayQty,
                                     StartDateType=l.StartDateType,
                                     EndDateType=l.EndDateType,
                                     LeaveTypeId = l.LeaveTypeId,
                                     IsLeave=lt.IsLeave,
                                     WorkFromHomeDayQty= ld.LeaveDayQty
                                 }
                                 ).ToList();

                    List<EmployeeLeave> PermissionList = (from l in context.Leave
                                                          join lt in context.LeaveType on l.LeaveTypeId equals lt.LeaveTypeId
                                                          join ld in context.PermissionDetail on l.LeaveId equals ld.LeaveId
                                                          where l.UserId == UserID && l.Status == "A" && ld.PermissionDate >= FromDate && ld.PermissionDate <= ToDate
                                                          select new EmployeeLeave
                                                          {
                                                              UserId = l.UserId,
                                                              StartDate = ld.PermissionDate,
                                                              EndDate = ld.PermissionDate,
                                                              LeaveType = lt.Type,
                                                              TimeFrom = ld.TimeFrom,
                                                              TimeTo = ld.TimeTo,
                                                              LeaveTypeId=l.LeaveTypeId,
                                                              IsLeave=lt.IsLeave
                                                          }
                                 ).ToList();

                    PermissionList.ForEach(pl => pl.PermissionCount = Math.Round((decimal)(lv.calculateDuration(pl.TimeFrom, pl.TimeTo).TotalMinutes) / 60,2));

                    if (PermissionList.Count > 0)
                        leaveList.AddRange(PermissionList);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return leaveList;
        }
    }
}