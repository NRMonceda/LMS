﻿using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Dac.Dac;
using NLTD.EmployeePortal.LMS.Dac.DbModel;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NLTD.EmployeePortal.LMS.Dac
{
    public class LeaveDac : ILeaveHelper
    {
        private readonly int BSB = Convert.ToInt32(ConfigurationManager.AppSettings["BeforeShiftBuffer"]);

        public string ChangeStatus(LeaveStatusModel status)
        {
            int isSaved = 0;
            string retValue = string.Empty;
            string existingStatus = string.Empty;

            using (var context = new NLTDDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var leave = context.Leave.Where(e => e.LeaveId == status.LeaveId).FirstOrDefault();
                        existingStatus = leave.Status;
                        if (leave != null)
                        {
                            if (existingStatus == "A")
                            {
                                leave.Comments = leave.Comments + "   (Cancellation Comments :" + status.Comment + ")";
                            }
                            else
                            {
                                leave.Comments = status.Comment;
                            }
                            leave.ApprovedBy = status.UserId;
                            leave.ApprovedAt = DateTime.Now;
                            leave.Status = status.Status;

                            isSaved = context.SaveChanges();
                            if (isSaved > 0)
                            {
                                retValue = "Saved";
                                var empLeaveBal = context.EmployeeLeaveBalance.Where(e => e.UserId == leave.UserId && e.LeaveTypeId == leave.LeaveTypeId && e.Year == leave.StartDate.Year).FirstOrDefault();
                                var adjustBal = context.LeaveType.Where(e => e.LeaveTypeId == leave.LeaveTypeId).FirstOrDefault();
                                if (adjustBal.IsTimeBased == false)
                                {
                                    if (empLeaveBal != null)
                                    {
                                        if (existingStatus == "P")
                                        {
                                            if (leave.Status == "A")
                                                empLeaveBal.LeaveTakenDays = (empLeaveBal.LeaveTakenDays ?? 0) + leave.Duration;
                                            else
                                            {
                                                if (adjustBal.AdjustLeaveBalance)
                                                    empLeaveBal.BalanceDays = (empLeaveBal.BalanceDays ?? 0) + leave.Duration;
                                            }
                                        }
                                        if (existingStatus == "P")
                                            empLeaveBal.PendingApprovalDays = (empLeaveBal.PendingApprovalDays ?? 0) - leave.Duration;
                                        else
                                        {
                                            empLeaveBal.LeaveTakenDays = (empLeaveBal.LeaveTakenDays ?? 0) - leave.Duration;
                                            if (adjustBal.AdjustLeaveBalance)
                                                empLeaveBal.BalanceDays = (empLeaveBal.BalanceDays ?? 0) + leave.Duration;
                                        }

                                        empLeaveBal.ModifiedBy = status.UserId;
                                        empLeaveBal.ModifiedOn = DateTime.Now;
                                        isSaved = context.SaveChanges();
                                        if (leave.IsExceptionLeave == true)
                                        {
                                            if (existingStatus == "P" || existingStatus == "A")
                                            {
                                                if (leave.Status == "R" || leave.Status == "C")
                                                {
                                                    //Reduce 1 from the AvailedLeavePolicyException
                                                    var emp = context.Employee.Where(x => x.UserId == leave.UserId).FirstOrDefault();
                                                    emp.AvailedLeavePolicyException = (emp.AvailedLeavePolicyException ?? 0) - 1;
                                                    emp.ModifiedBy = status.UserId;
                                                    emp.ModifiedOn = DateTime.Now;
                                                    isSaved = context.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                }
                                if (isSaved > 0)
                                {
                                    if (adjustBal.IsTimeBased == false)
                                    {
                                        TransactionHistoryModel hist = new TransactionHistoryModel
                                        {
                                            EmployeeId = leave.UserId,
                                            LeaveTypeId = leave.LeaveTypeId,
                                            LeaveId = leave.LeaveId,
                                            NumberOfDays = leave.Duration,
                                            TransactionBy = status.UserId
                                        };
                                        if (leave.Status == "A")
                                        {
                                            hist.Remarks = "Approved";
                                            hist.TransactionType = "D";
                                        }
                                        else if (leave.Status == "C")
                                        {
                                            hist.Remarks = "Cancelled";
                                            hist.TransactionType = "C";
                                        }
                                        else
                                        {
                                            hist.Remarks = "Rejected";
                                            hist.TransactionType = "C";
                                        }

                                        LeaveTransactionHistory histRec = new LeaveTransactionHistory
                                        {
                                            UserId = hist.EmployeeId,
                                            LeaveTypeId = hist.LeaveTypeId,
                                            LeaveId = hist.LeaveId,
                                            TransactionDate = DateTime.Now,
                                            TransactionType = hist.TransactionType,
                                            NumberOfDays = hist.NumberOfDays,
                                            TransactionBy = hist.TransactionBy,
                                            Remarks = hist.Remarks
                                        };
                                        context.LeaveTransactionHistory.Add(histRec);
                                        isSaved = context.SaveChanges();
                                        //}
                                    }
                                }
                                else
                                {
                                    retValue = "NotSaved";
                                }
                            }
                            else
                            {
                                retValue = "NotSaved";
                            }
                        }
                        else
                        {
                            return "NotSaved";
                        }
                        if (retValue == "Saved")
                        {
                            transaction.Commit();
                            retValue = "Saved";
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            return retValue;
        }

        public IList<LeaveSummary> GetLeaveSumary(long UserId, Int32 summaryYear)
        {
            IList<LeaveSummary> lstSummary;

            using (var context = new NLTDDbContext())
            {
                var LeaveItemsquery = (from emp in context.Employee
                                       join bal in context.EmployeeLeaveBalance on emp.UserId equals bal.UserId
                                       join types in context.LeaveType on bal.LeaveTypeId equals types.LeaveTypeId
                                       where emp.UserId == UserId && bal.Year == summaryYear && types.IsTimeBased == false
                                       select new LeaveSummary
                                       {
                                           Avatar = emp.AvatarUrl,
                                           LeaveTypeId = bal.LeaveTypeId,
                                           LeaveType = types.Type,
                                           AdjustLeaveBalance = types.AdjustLeaveBalance,
                                           IsTimeBased = types.IsTimeBased,
                                           TotalLeaves = bal.TotalDays ?? 0,
                                           LeavesTaken = bal.LeaveTakenDays ?? 0,
                                           LeavesPendingApproval = bal.PendingApprovalDays ?? 0,
                                           LeavesBalance = bal.BalanceDays ?? 0
                                       }).AsQueryable();
                lstSummary = LeaveItemsquery.ToList();
            }
            return lstSummary;
        }

        public IList<EmployeeWiseLeaveSummaryModel> GetEmployeeWiseLeaveSumary(Int64 UserId, int Year, string reqUsr, Int64? paramUserId, bool OnlyReportedToMe)
        {
            IList<Int64> empList = GetEmployeesReporting(UserId);
            IList<EmployeeWiseLeaveSummaryModel> lstSummary = new List<EmployeeWiseLeaveSummaryModel>();
            using (var context = new NLTDDbContext())
            {
                var LeaveItemsquery = (from emp in context.Employee
                                       join bal in context.EmployeeLeaveBalance on emp.UserId equals bal.UserId
                                       join types in context.LeaveType on bal.LeaveTypeId equals types.LeaveTypeId
                                       where bal.Year == Year && types.IsTimeBased == false
                                       orderby emp.FirstName
                                       select new EmployeeWiseLeaveSummaryModel
                                       {
                                           UserId = emp.UserId,
                                           EmpID = emp.EmployeeId,
                                           Name = emp.FirstName + " " + emp.LastName,
                                           ReportingToId = emp.ReportingToId,
                                           LeaveType = types.Type,
                                           TotalLeaves = bal.TotalDays ?? 0,
                                           UsedLeaves = bal.LeaveTakenDays ?? 0,
                                           PendingApproval = bal.PendingApprovalDays ?? 0,
                                           BalanceLeaves = bal.BalanceDays ?? 0,
                                           AdjustLeaveBalance = types.AdjustLeaveBalance
                                       }).AsQueryable();
                if (LeaveItemsquery != null)
                {
                    if (reqUsr == "My")
                    {
                        lstSummary = LeaveItemsquery.Where(x => x.UserId == UserId).ToList();
                    }
                    else
                    {
                        EmployeeDac employeeDac = new EmployeeDac();
                        string leadRole = employeeDac.GetEmployeeRole(UserId);

                        if (leadRole != "")
                        {
                            if (reqUsr == "Team")
                            {
                                if (leadRole == "ADMIN" || leadRole == "HR")
                                {
                                    if (paramUserId > 0)
                                    {
                                        lstSummary = LeaveItemsquery.Where(x => x.UserId == paramUserId).ToList();
                                    }
                                    else
                                    {
                                        if (OnlyReportedToMe)
                                        {
                                            lstSummary = LeaveItemsquery.Where(x => x.ReportingToId == UserId).ToList();
                                        }
                                        else
                                        {
                                            lstSummary = LeaveItemsquery.ToList();
                                        }
                                    }
                                }
                                else
                                {
                                    if (paramUserId > 0)
                                    {
                                        lstSummary = LeaveItemsquery.Where(t => empList.Contains(t.UserId)).ToList();
                                        if (lstSummary.Count > 0)
                                            lstSummary = lstSummary.Where(x => x.UserId == paramUserId).ToList();
                                    }
                                    else
                                    {
                                        if (OnlyReportedToMe)
                                        {
                                            lstSummary = LeaveItemsquery.Where(x => x.ReportingToId == UserId).ToList();
                                        }
                                        else
                                        {
                                            lstSummary = LeaveItemsquery.Where(t => empList.Contains(t.UserId)).ToList();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    lstSummary = lstSummary.OrderBy(x => x.Name).ToList();
                }
            }
            if (lstSummary.Count > 0)
                lstSummary = lstSummary.OrderBy(x => x.EmpID).ToList();

            return lstSummary;
        }

        public int GetHolidayCount(DateTime startDate, DateTime endDate, long userId)
        {
            int retCount = 0;
            using (var context = new NLTDDbContext())
            {
                var holidayCount = (from emp in context.Employee
                                    join h in context.OfficeHoliday on emp.OfficeHolidayId equals h.OfficeId
                                    where emp.UserId == userId && (h.Holiday >= startDate && h.Holiday <= endDate)
                                    select new
                                    {
                                        HolidayId = h.OfficeHolidayId
                                    }).ToList();
                if (holidayCount.Any())
                {
                    retCount = holidayCount.Count;
                }
                else
                {
                    retCount = 0;
                }
            }
            return retCount;
        }

        public IList<HolidayModel> GetHolidaysDetails(long UserId, Int32 holidayYear, ref bool previousYear, ref bool nextYear)
        {
            IList<HolidayModel> holidays;
            int previousYearCount = 0;
            int nextYearCount = 0;
            int previousHolidayYear = holidayYear - 1;
            int nextHolidayYear = holidayYear + 1;
            previousYear = false;
            nextYear = false;
            using (var context = new NLTDDbContext())
            {
                var holidayQry = (from emp in context.Employee
                                  join h in context.OfficeHoliday on emp.OfficeHolidayId equals h.OfficeId
                                  join o in context.OfficeLocation on h.OfficeId equals o.OfficeId
                                  where emp.UserId == UserId && h.Year == holidayYear
                                  orderby h.Holiday ascending
                                  select new HolidayModel
                                  {
                                      HolidayDate = h.Holiday,
                                      HolidayText = h.Title,
                                      HolidayOfficeName = o.OfficeName
                                  }).AsQueryable();
                holidays = holidayQry.ToList();

                var previousQry = (from emp in context.Employee
                                   join h in context.OfficeHoliday on emp.OfficeHolidayId equals h.OfficeId
                                   join o in context.OfficeLocation on h.OfficeId equals o.OfficeId
                                   where emp.UserId == UserId && h.Year == previousHolidayYear
                                   orderby h.Holiday ascending
                                   select new
                                   {
                                       HolidayDate = h.Holiday
                                   }).AsQueryable();

                previousYearCount = previousQry.ToList().Count();

                var nextQry = (from emp in context.Employee
                               join h in context.OfficeHoliday on emp.OfficeHolidayId equals h.OfficeId
                               join o in context.OfficeLocation on h.OfficeId equals o.OfficeId
                               where emp.UserId == UserId && h.Year == nextHolidayYear
                               orderby h.Holiday ascending
                               select new
                               {
                                   HolidayDate = h.Holiday
                               }).AsQueryable();

                nextYearCount = nextQry.ToList().Count();
            }

            previousYear = previousYearCount > 0;
            nextYear = nextYearCount > 0;

            return holidays;
        }

        public IList<HolidayModel> GetHolidays(long userId, Int32 holYear)
        {
            IList<HolidayModel> holidays;
            using (var context = new NLTDDbContext())
            {
                var holidayQry = (from emp in context.Employee
                                  join h in context.OfficeHoliday on emp.OfficeHolidayId equals h.OfficeId
                                  join o in context.OfficeLocation on h.OfficeId equals o.OfficeId
                                  where emp.UserId == userId && h.Year == holYear
                                  orderby h.Holiday ascending
                                  select new HolidayModel
                                  {
                                      HolidayDate = h.Holiday,
                                      HolidayText = h.Title,
                                      HolidayOfficeName = o.OfficeName
                                  }).AsQueryable();
                holidays = holidayQry.ToList();
            }
            return holidays;
        }

        public string GetHolidayDates(long userId, Int32 holYear)
        {
            IList<HolidayModel> holidays;
            string retStr = string.Empty;
            StringBuilder holStr = new StringBuilder();
            using (var context = new NLTDDbContext())
            {
                var holidayQry = (from emp in context.Employee
                                  join h in context.OfficeHoliday on emp.OfficeHolidayId equals h.OfficeId
                                  join o in context.OfficeLocation on h.OfficeId equals o.OfficeId
                                  where emp.UserId == userId && h.Year == holYear
                                  select new HolidayModel
                                  {
                                      HolidayDate = h.Holiday
                                  }).AsQueryable();
                holidays = holidayQry.ToList();
                foreach (var item in holidays)
                {
                    holStr.Append(item.HolidayDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) + ",");
                }
                retStr = holStr.ToString();
                if (holStr.Length > 1)
                {
                    retStr = retStr.Substring(0, holStr.Length - 1);
                }
            }
            return retStr;
        }

        public IList<DropDownItem> GetWeekOffs(long userId)
        {
            IList<DropDownItem> weekOffs;
            using (var context = new NLTDDbContext())
            {
                var holidayQry = (from emp in context.Employee
                                  join ew in context.EmployeeWeekOff on emp.UserId equals ew.UserId
                                  join w in context.DayOfWeek on ew.DaysOfWeekId equals w.DaysOfWeekId
                                  where emp.UserId == userId
                                  select new DropDownItem
                                  {
                                      Key = w.Day
                                  }).AsQueryable();
                weekOffs = holidayQry.ToList();
            }
            return weekOffs;
        }

        public int CheckHoliday(DateTime dateValue, long userId)
        {
            int retCount = 0;
            using (var context = new NLTDDbContext())
            {
                var holidayCount = (from emp in context.Employee
                                    join h in context.OfficeHoliday on emp.OfficeId equals h.OfficeId
                                    where emp.UserId == userId && h.Holiday == dateValue
                                    select new
                                    {
                                        HolidayId = h.OfficeHolidayId
                                    }).ToList();
                if (holidayCount.Any())
                {
                    retCount = holidayCount.Count;
                }
                else
                {
                    retCount = 0;
                }
            }
            return retCount;
        }

        public IList<EmployeeList> GetEmployeeList(string param, Int64 userId)
        {
            IList<EmployeeList> employeeList = new List<EmployeeList>();
            List<Employee> directEmployees = new List<Employee>();
            List<Int64> reportingEmployees = new List<Int64>();

            EmployeeDac employeeDac = new EmployeeDac();
            string leadRole = employeeDac.GetEmployeeRole(userId);

            using (var context = new NLTDDbContext())
            {
                employeeList = (from emp in context.Employee
                                where (emp.FirstName + " " + emp.LastName).Contains(param.ToUpper())
                                select new EmployeeList
                                {
                                    UserId = emp.UserId,
                                    Name = emp.FirstName + " " + emp.LastName
                                }
                                   ).ToList();

                if (leadRole != "ADMIN" && leadRole != "HR")
                {
                    directEmployees = context.Employee
                        .Where(e => e.ReportingToId == userId && e.IsActive == true)
                                      .ToList();

                    foreach (var employee in directEmployees)
                    {
                        reportingEmployees.Add(employee.UserId);
                        reportingEmployees.AddRange(GetEmployeesReporting(employee.UserId));
                    }
                    employeeList = employeeList.Where(t => reportingEmployees.Contains(t.UserId)).ToList();
                }
            }
            return employeeList.OrderBy(e => e.Name).ToList();
        }

        public IList<Int64> GetEmployeesReporting(long leadId)
        {
            var result = new List<Int64>();
            using (var context = new NLTDDbContext())
            {
                var employees = context.Employee
                                       .Where(e => e.ReportingToId == leadId && e.IsActive == true)
                                       .ToList();
                foreach (var employee in employees)
                {
                    result.Add(employee.UserId);
                    result.AddRange(GetEmployeesReporting(employee.UserId));
                }
            }
            return result;
        }

        public IList<string> GetHigherApproversEmailIds(long? leadId)
        {
            var result = new List<string>();
            using (var context = new NLTDDbContext())
            {
                var reporting = context.Employee
                                       .Where(e => e.UserId == leadId)
                                       .FirstOrDefault();
                if (reporting != null)
                {
                    result.Add(reporting.EmailAddress);
                    if (reporting.ReportingToId != null)
                        result.AddRange(GetHigherApproversEmailIds(reporting.ReportingToId));
                }
            }
            return result;
        }

        public IList<TeamLeaves> GetLeaveRequests(ManageTeamLeavesQueryModel qryMdl)
        {
            IList<Int64> empList = GetEmployeesReporting(qryMdl.LeadId);
            string qryStatus = string.Empty;
            if (qryMdl.ShowApprovedLeaves)
            {
                qryStatus = "A";
            }
            else
            {
                qryStatus = "P";
            }
            using (var context = new NLTDDbContext())
            {
                DateTime QryFromDate = qryMdl.FromDate ?? Convert.ToDateTime("01/01/1900");
                DateTime QryToDate = qryMdl.ToDate ?? Convert.ToDateTime("01/01/9999");

                var LeaveItemsquery = (from leave in context.Leave
                                       join types in context.LeaveType on leave.LeaveTypeId equals types.LeaveTypeId
                                       join user in context.Employee on leave.UserId equals user.UserId
                                       where leave.Status == qryStatus &&
                                       ((qryMdl.FromDate == null || qryMdl.ToDate == null) || (
                                       (leave.StartDate >= QryFromDate && leave.StartDate <= QryToDate) ||
                                       (leave.EndDate >= QryFromDate && leave.EndDate <= QryToDate) ||
                                       (leave.StartDate <= QryFromDate && leave.EndDate >= QryToDate)))
                                       select new LeaveItem
                                       {
                                           Avatar = user.AvatarUrl,
                                           UserId = user.UserId,
                                           LeaveFromDate = leave.StartDate,
                                           LeaveFromType = leave.StartDateType,
                                           LeaveId = leave.LeaveId,
                                           LeaveTypeText = types.Type,
                                           LeaveUptoDate = leave.EndDate,
                                           LeaveUptoType = leave.EndDateType,
                                           NumberOfDaysRequired = leave.Duration,
                                           Reason = leave.Remarks,
                                           RequestDate = leave.AppliedAt,
                                           RequesterName = user.FirstName + " " + user.LastName,
                                           Status = leave.Status,
                                           ReportingToId = user.ReportingToId,
                                           isTimeBased = types.IsTimeBased,
                                           Comments = leave.Comments,
                                           AppliedById = leave.AppliedBy,
                                           LeaveTypeId = types.LeaveTypeId,
                                           IsExceptionTypeLeave = leave.IsExceptionLeave
                                       }).AsQueryable();

                List<LeaveItem> LeaveItems = new List<LeaveItem>();
                if (qryMdl.OnlyReportedToMe)
                {
                    LeaveItems = LeaveItemsquery.Where(x => (x.ReportingToId == qryMdl.LeadId)).ToList();
                }
                else
                {
                    if (context.Employee.Where(x => x.UserId == qryMdl.LeadId).FirstOrDefault().ReportingToId == null)
                    {
                        empList.Add(qryMdl.LeadId);
                    }
                    LeaveItems = LeaveItemsquery.Where(t => empList.Contains(t.UserId)).ToList();
                }

                LeaveItems = (from l in LeaveItems
                              join e in context.Employee on l.AppliedById equals e.UserId
                              select new LeaveItem
                              {
                                  UserId = l.UserId,
                                  LeaveFromDate = l.LeaveFromDate,
                                  LeaveFromType = l.LeaveFromType,
                                  LeaveId = l.LeaveId,
                                  LeaveTypeText = l.LeaveTypeText,
                                  LeaveUptoDate = l.LeaveUptoDate,
                                  LeaveUptoType = l.LeaveUptoType,
                                  NumberOfDaysRequired = l.NumberOfDaysRequired,
                                  Reason = l.Reason,
                                  RequestDate = l.RequestDate,
                                  RequesterName = l.RequesterName,
                                  Status = l.Status,
                                  ReportingToId = l.ReportingToId,
                                  isTimeBased = l.isTimeBased,
                                  Comments = l.Comments,
                                  PermissionInMonth = (l.isTimeBased == false) ? "" : ReturnPermissionHoursPerMonthInString(l.LeaveFromDate.Month, l.UserId, l.LeaveTypeId, false),
                                  AppliedByName = e.FirstName + " " + e.LastName,
                                  IsExceptionTypeLeave = l.IsExceptionTypeLeave
                              }).ToList();

                var pdLeaveId = (from items in LeaveItems
                                 where items.isTimeBased == true
                                 select items.LeaveId).Distinct().ToList();

                var pdTime = (from pd in context.PermissionDetail
                              where pdLeaveId.Contains(pd.LeaveId)
                              select new
                              {
                                  leaveId = pd.LeaveId,
                                  fromTime = pd.TimeFrom,
                                  toTime = pd.TimeTo
                              }).ToList();

                for (int i = 0; i < LeaveItems.Count; i++)
                {
                    if (LeaveItems[i].isTimeBased == true)
                    {
                        var pdRec = pdTime.Where(x => x.leaveId == LeaveItems[i].LeaveId).FirstOrDefault();
                        LeaveItems[i].PermissionTime = pdRec.fromTime + " to " + pdRec.toTime;
                    }
                }

                IList<TeamLeaves> retList = new List<TeamLeaves>();
                var groupedLeavesList = LeaveItems.GroupBy(u => u.UserId)
                                                      .Select(grp => new { UserId = grp.Key, teamLeaves = grp.ToList() })
                                                      .ToList();
                TeamLeaves newList;
                foreach (var item in groupedLeavesList)
                {
                    newList = new TeamLeaves
                    {
                        UserId = item.UserId,
                        TeamLeaveList = item.teamLeaves.OrderByDescending(x => x.LeaveFromDate).ToList(),
                        Name = item.teamLeaves[0].RequesterName
                    };
                    retList.Add(newList);
                }
                retList = retList.OrderBy(x => x.Name).ToList();
                return retList;
            }
        }

        public string ReturnPermissionHoursPerMonthInString(int month, Int64 userId, long leaveTypeId, bool includePending)
        {
            string retSring = string.Empty;
            TimeSpan totalDuration = TimeSpan.Zero;
            totalDuration = ReturnPermissionHoursPerMonth(month, userId, leaveTypeId, includePending);
            if (totalDuration == TimeSpan.Zero)
            {
                retSring = "00:00";
            }
            else
            {
                retSring = totalDuration.ToString(@"hh\:mm");
            }
            return retSring;
        }

        public TimeSpan ReturnPermissionHoursPerMonth(int month, Int64 userId, long leaveTypeId, bool includePending)
        {
            string retSring = string.Empty;
            TimeSpan totalDuration = TimeSpan.Zero;
            using (var context = new NLTDDbContext())
            {
                var permissions = (from l in context.Leave
                                   join lp in context.PermissionDetail on l.LeaveId equals lp.LeaveId
                                   where lp.PermissionDate.Month == month && l.UserId == userId &&
                                    ((includePending && l.Status == "P") || l.Status == "A")
                                    && l.StartDate.Year == DateTime.Now.Year
                                    && l.LeaveTypeId == leaveTypeId
                                   select new { lp.TimeFrom, lp.TimeTo }
                                     )
                                     .ToList();

                if (permissions.Count > 0)
                {
                    for (int i = 0; i < permissions.Count; i++)
                    {
                        totalDuration = totalDuration + CalculateDuration(permissions[i].TimeFrom, permissions[i].TimeTo);
                    }
                }
            }
            return totalDuration;
        }

        public TimeSpan CalculateDuration(string permissionTimeFrom, string permissionTimeTo)
        {
            TimeSpan timeFrom;
            DateTime permissionDateFromTime;
            TimeSpan timeTo;
            DateTime permissionDateToTime;
            TimeSpan duration = TimeSpan.Zero;

            permissionDateFromTime = Convert.ToDateTime(permissionTimeFrom);
            permissionDateToTime = Convert.ToDateTime(permissionTimeTo);
            timeFrom = Convert.ToDateTime(permissionTimeFrom).TimeOfDay;
            timeTo = Convert.ToDateTime(permissionTimeTo).TimeOfDay;
            if ((timeFrom.Hours >= 0 && timeFrom.Hours < 12) && timeTo.Hours < 12)
            {
                permissionDateFromTime = permissionDateFromTime.AddDays(1);
            }
            if (timeTo.Hours >= 0 && timeTo.Hours < 12)
            {
                permissionDateToTime = permissionDateToTime.AddDays(1);
            }

            duration = permissionDateToTime.Subtract(permissionDateFromTime);

            return duration;
        }

        public IList<TeamLeaves> GetTeamLeaveHistory(ManageTeamLeavesQueryModel qryMdl)
        {
            IList<Int64> empList = GetEmployeesReporting(qryMdl.LeadId);
            using (var context = new NLTDDbContext())
            {
                DateTime QryFromDate = qryMdl.FromDate ?? Convert.ToDateTime("01/01/1900");
                DateTime QryToDate = qryMdl.ToDate ?? Convert.ToDateTime("01/01/9999");

                var LeaveItemsquery = (from leave in context.Leave
                                       join types in context.LeaveType on leave.LeaveTypeId equals types.LeaveTypeId
                                       join user in context.Employee on leave.UserId equals user.UserId
                                       join apprv in context.Employee on leave.ApprovedBy equals apprv.UserId into leaves//TODO check suresh
                                       where
                                       ((qryMdl.FromDate == null || qryMdl.ToDate == null) || (
                                       (leave.StartDate >= QryFromDate && leave.StartDate <= QryToDate) ||
                                       (leave.EndDate >= QryFromDate && leave.EndDate <= QryToDate) ||
                                       (leave.StartDate <= QryFromDate && leave.EndDate >= QryToDate)))
                                       from l in leaves.DefaultIfEmpty()
                                       select new LeaveItem
                                       {
                                           UserId = user.UserId,
                                           LeaveFromDate = leave.StartDate,
                                           LeaveFromType = leave.StartDateType,
                                           LeaveId = leave.LeaveId,
                                           LeaveTypeText = types.Type,
                                           LeaveUptoDate = leave.EndDate,
                                           LeaveUptoType = leave.EndDateType,
                                           NumberOfDaysRequired = leave.Duration,
                                           Reason = leave.Remarks,
                                           RequestDate = leave.AppliedAt,
                                           RequesterName = user.FirstName + " " + user.LastName,
                                           Status = leave.Status,
                                           ReportingToId = user.ReportingToId,
                                           ApprovedById = leave.ApprovedBy,
                                           ApprovedByName = l.FirstName + " " + l.LastName,
                                           ApprovedDateFromLinq = leave.ApprovedAt,
                                           Comments = leave.Comments,
                                           IsLeave = types.IsLeave,
                                           isTimeBased = types.IsTimeBased,
                                           AppliedById = leave.AppliedBy,
                                           IsExceptionTypeLeave = leave.IsExceptionLeave
                                       }).AsQueryable();

                if (qryMdl.IsLeaveOnly)
                {
                    LeaveItemsquery = LeaveItemsquery.Where(x => x.IsLeave == true);
                }

                List<LeaveItem> LeaveItems = new List<LeaveItem>();
                if (qryMdl.RequestMenuUser == "My")
                {
                    LeaveItems = LeaveItemsquery.Where(x => x.UserId == qryMdl.LeadId).ToList();
                }
                else
                {
                    EmployeeDac employeeDac = new EmployeeDac();
                    string leadRole = employeeDac.GetEmployeeRole(qryMdl.LeadId);

                    if (leadRole != "")
                    {
                        if (leadRole == "ADMIN" || leadRole == "HR")
                        {
                            if (qryMdl.SearchUserID > 0)
                            {
                                LeaveItems = LeaveItemsquery.Where(x => x.UserId == qryMdl.SearchUserID).ToList();
                            }
                            else
                            {
                                if (qryMdl.OnlyReportedToMe)
                                {
                                    LeaveItems = LeaveItemsquery.Where(t => t.ReportingToId == qryMdl.LeadId).ToList();
                                }
                                else
                                {
                                    LeaveItems = LeaveItemsquery.ToList();
                                }
                            }
                        }
                        else
                        {
                            if (qryMdl.SearchUserID > 0)
                            {
                                LeaveItems = LeaveItemsquery.Where(t => empList.Contains(t.UserId)).ToList();
                                if (LeaveItems.Count > 0)
                                    LeaveItems = LeaveItems.Where(x => x.UserId == qryMdl.SearchUserID).ToList();
                            }
                            else
                            {
                                if (qryMdl.OnlyReportedToMe)
                                {
                                    LeaveItems = LeaveItemsquery.Where(t => t.ReportingToId == qryMdl.LeadId).ToList();
                                }
                                else
                                {
                                    LeaveItems = LeaveItemsquery.Where(t => empList.Contains(t.UserId)).ToList();
                                }
                            }
                        }
                    }
                }
                if (qryMdl.ShowApprovedLeaves)
                {
                    LeaveItems = LeaveItems.Where(x => x.Status == "A").ToList();
                }
                if (qryMdl.SearchUserID != null && qryMdl.SearchUserID != 0)
                {
                    if (LeaveItems.Count > 0)
                    {
                        LeaveItems = LeaveItems.Where(x => x.UserId == qryMdl.SearchUserID).ToList();
                    }
                }
                LeaveItems = (from l in LeaveItems
                              join e in context.Employee on l.AppliedById equals e.UserId
                              select new LeaveItem
                              {
                                  UserId = l.UserId,
                                  LeaveFromDate = l.LeaveFromDate,
                                  LeaveFromType = l.LeaveFromType,
                                  LeaveId = l.LeaveId,
                                  LeaveTypeText = l.LeaveTypeText,
                                  LeaveUptoDate = l.LeaveUptoDate,
                                  LeaveUptoType = l.LeaveUptoType,
                                  NumberOfDaysRequired = l.NumberOfDaysRequired,
                                  Reason = l.Reason,
                                  RequestDate = l.RequestDate,
                                  RequesterName = l.RequesterName,
                                  Status = l.Status,
                                  ReportingToId = l.ReportingToId,
                                  ApprovedById = l.ApprovedById,
                                  ApprovedByName = l.ApprovedByName,
                                  ApprovedDate = l.ApprovedDateFromLinq ?? Convert.ToDateTime("01/01/1900"),
                                  Comments = l.Comments,
                                  IsLeave = l.IsLeave,
                                  isTimeBased = l.isTimeBased,
                                  AppliedById = l.AppliedById,
                                  AppliedByName = e.FirstName + " " + e.LastName,
                                  IsExceptionTypeLeave = l.IsExceptionTypeLeave
                              }).ToList();

                var pdLeaveId = (from items in LeaveItems
                                 where items.isTimeBased == true
                                 select items.LeaveId).Distinct().ToList();

                var pdTime = (from pd in context.PermissionDetail
                              where pdLeaveId.Contains(pd.LeaveId)
                              select new
                              {
                                  leaveId = pd.LeaveId,
                                  fromTime = pd.TimeFrom,
                                  toTime = pd.TimeTo
                              }).ToList();

                for (int i = 0; i < LeaveItems.Count; i++)
                {
                    if (LeaveItems[i].isTimeBased == true)
                    {
                        var pdRec = pdTime.Where(x => x.leaveId == LeaveItems[i].LeaveId).FirstOrDefault();
                        LeaveItems[i].PermissionTime = pdRec.fromTime + " to " + pdRec.toTime;
                    }
                }

                IList<TeamLeaves> retList = new List<TeamLeaves>();
                var groupedLeaveList = LeaveItems.GroupBy(u => u.UserId)
                                                      .Select(grp => new { UserId = grp.Key, teamLeaves = grp.ToList() })
                                                      .ToList();
                TeamLeaves newList;
                foreach (var item in groupedLeaveList)
                {
                    newList = new TeamLeaves
                    {
                        UserId = item.UserId,
                        TeamLeaveList = item.teamLeaves.OrderByDescending(x => x.LeaveFromDate).ToList(),
                        Name = item.teamLeaves[0].RequesterName
                    };
                    retList.Add(newList);
                }

                retList = retList.OrderBy(x => x.Name).ToList();
                return retList;
            }
        }

        public List<LeaveTypesModel> GetLeaveTypes(long OfficeId, Int64 userId)
        {
            using (var context = new NLTDDbContext())
            {
                List<LeaveTypesModel> LeaveTypes = (from types in context.LeaveType
                                                    join emp in context.Employee on types.OfficeId equals emp.OfficeId
                                                    where types.OfficeId == OfficeId && emp.UserId == userId && (emp.Gender == types.ApplicableGender || types.ApplicableGender == "A")
                                                    orderby types.SortOrder ascending
                                                    select new LeaveTypesModel
                                                    {
                                                        LeaveTypeId = types.LeaveTypeId,
                                                        LeaveTypeText = types.Type,
                                                        IsTimeBased = types.IsTimeBased
                                                    }).ToList();
                return LeaveTypes;
            }
        }

        public string GetTimeBasedLeaveTypesString(long OfficeId, Int64 userId)
        {
            StringBuilder strTypes = new StringBuilder();
            string retTypes = string.Empty;
            using (var context = new NLTDDbContext())
            {
                List<LeaveTypesModel> LeaveTypes = (from types in context.LeaveType
                                                    join emp in context.Employee on types.OfficeId equals emp.OfficeId
                                                    where types.OfficeId == OfficeId && emp.UserId == userId && (emp.Gender == types.ApplicableGender || types.ApplicableGender == "A") && types.IsTimeBased == true
                                                    select new LeaveTypesModel
                                                    {
                                                        LeaveTypeId = types.LeaveTypeId
                                                    }).ToList();
                if (LeaveTypes.Count > 0)
                {
                    foreach (var item in LeaveTypes)
                    {
                        strTypes.Append(Convert.ToString(item.LeaveTypeId) + ",");
                    }
                }
            }
            retTypes = strTypes.ToString();
            if (retTypes.IndexOf(',') >= 0)
                retTypes = retTypes.Remove(retTypes.LastIndexOf(","));
            return retTypes;
        }

        public List<DropDownItem> GetYearsFromLeaveBalance()
        {
            using (var context = new NLTDDbContext())
            {
                var qry = (from types in context.EmployeeLeaveBalance
                           select new DropDownItem
                           {
                               Key = types.Year.ToString(),
                               Value = types.Year.ToString()
                           }).Distinct();
                List<DropDownItem> years = qry.OrderByDescending(x => x.Value).ToList();
                return years;
            }
        }

        public string SaveLeaveRequest(LeaveRequestModel request)
        {
            int isSaved = 0;
            Int64 newId = 0;
            bool isExceptionLeave = false;
            using (var context = new NLTDDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var adjustBal = context.LeaveType.Where(e => e.LeaveTypeId == request.LeaveType).FirstOrDefault();
                        bool isTimeBased = adjustBal.IsTimeBased;
                        string duplicateRequest = string.Empty;
                        int daysBeforeApplied = 0;
                        decimal leaveDuration = 0;
                        string todayDate = System.DateTime.Now.Date.ToString("ddMMyyyy", CultureInfo.InvariantCulture);
                        int numberOfLeaveExceptionsAllowed = Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfLeaveExceptionsAllowed"].ToString());
                        DateTime leavePolicyRelaxationDate = DateTime.ParseExact(ConfigurationManager.AppSettings["leavePolicyRelaxationDate"].ToString(), "ddMMyyyy", CultureInfo.InvariantCulture);

                        if (isTimeBased)
                        {
                            request.LeaveUpto = request.LeaveFrom;
                            request.IsExceptionTypeLeave = false;
                        }
                        if (isTimeBased == false)
                        {
                            request.leaveDetail = GetLeaveDetailCalculation(request.LeaveFrom, request.LeaveUpto, request.LeaveFromTime, request.LeaveUptoTime, request.UserId, request.LeaveType);
                            leaveDuration = request.leaveDetail[0].Total;

                            #region Duplicate Leave Logic

                            var chkLeave = context.Leave
                            .Where(h => h.UserId == request.UserId && (h.Status == "A" || h.Status == "P") && ((request.LeaveFrom >= h.StartDate && request.LeaveFrom <= h.EndDate) || (request.LeaveUpto >= h.StartDate && request.LeaveUpto <= h.EndDate) || (request.LeaveFrom <= h.StartDate && request.LeaveUpto >= h.EndDate))).ToList();
                            if (chkLeave.Any())
                            {
                                foreach (var item in chkLeave)
                                {
                                    if (item.StartDateType == "A" && item.EndDateType == "A")
                                    {
                                        if (item.Duration > 0)
                                        {
                                            if (isTimeBased == false)
                                                duplicateRequest = "Duplicate";
                                        }
                                    }
                                    else
                                    {
                                        if (isTimeBased == false) //No half day duplicate validation for time based requests such as Permission
                                        {
                                            if (request.LeaveFrom.Date == item.StartDate.Date || request.LeaveFrom.Date == item.EndDate.Date || request.LeaveUpto.Date == item.StartDate.Date || request.LeaveUpto.Date == item.EndDate.Date)
                                            {
                                                if (request.LeaveFrom.Date == item.StartDate.Date)
                                                {
                                                    if (request.LeaveFromTime == "A")
                                                    {
                                                        duplicateRequest = "Duplicate";
                                                    }
                                                    else if (request.LeaveFromTime == "F")
                                                    {
                                                        if (item.StartDateType == "F")
                                                            duplicateRequest = "Duplicate";
                                                    }
                                                    else if (request.LeaveFromTime == "S")
                                                    {
                                                        if (item.StartDateType == "S")
                                                            duplicateRequest = "Duplicate";
                                                    }
                                                }
                                                if (request.LeaveFrom.Date == item.EndDate.Date)
                                                {
                                                    if (request.LeaveFromTime == "A")
                                                    {
                                                        duplicateRequest = "Duplicate";
                                                    }
                                                    else if (request.LeaveFromTime == "F")
                                                    {
                                                        if (item.EndDateType == "F")
                                                            duplicateRequest = "Duplicate";
                                                    }
                                                    else if (request.LeaveFromTime == "S")
                                                    {
                                                        if (item.EndDateType == "S")
                                                            duplicateRequest = "Duplicate";
                                                    }
                                                }
                                                if (request.LeaveUpto.Date == item.StartDate.Date)
                                                {
                                                    if (request.LeaveUptoTime == "A")
                                                    {
                                                        duplicateRequest = "Duplicate";
                                                    }
                                                    else if (request.LeaveUptoTime == "F")
                                                    {
                                                        if (item.StartDateType == "F")
                                                            duplicateRequest = "Duplicate";
                                                    }
                                                    else if (request.LeaveUptoTime == "S")
                                                    {
                                                        if (item.StartDateType == "S")
                                                            duplicateRequest = "Duplicate";
                                                    }
                                                }
                                                if (request.LeaveUpto.Date == item.EndDate.Date)
                                                {
                                                    if (request.LeaveUptoTime == "A")
                                                    {
                                                        duplicateRequest = "Duplicate";
                                                    }
                                                    else if (request.LeaveUptoTime == "F")
                                                    {
                                                        if (item.EndDateType == "F")
                                                            duplicateRequest = "Duplicate";
                                                    }
                                                    else if (request.LeaveUptoTime == "S")
                                                    {
                                                        if (item.EndDateType == "S")
                                                            duplicateRequest = "Duplicate";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                duplicateRequest = "Duplicate";
                                            }
                                        }
                                    }
                                    if (duplicateRequest == "Duplicate")
                                    {
                                        break;
                                    }
                                }

                                //duplicateRequest = "Duplicate";
                                if (duplicateRequest == "Duplicate")
                                    return "Duplicate";
                            }

                            #endregion Duplicate Leave Logic

                            if (adjustBal.LeaveCombinationAllowed == false)
                            {
                                isExceptionLeave = CheckExceptionLeave(request.LeaveFrom, request.LeaveUpto, request.LeaveFromTime, request.LeaveUptoTime, request.UserId, request.LeaveType);
                                if (isExceptionLeave == true)
                                {
                                    if (adjustBal.Type == "Casual Leave")
                                    {
                                        return "CLExceptionNotAllowed";
                                    }
                                    else if (request.IsExceptionTypeLeave == false)
                                    {
                                        return "CheckException";
                                    }
                                }
                                else
                                {
                                    if (request.IsExceptionTypeLeave == true)
                                    {
                                        return "UnCheckException";
                                    }
                                }
                            }
                        }
                        var empProfile = context.Employee.Where(x => x.UserId == request.UserId).FirstOrDefault();

                        //2019 Leave Policy restrictions
                        if (DateTime.ParseExact(todayDate, "ddMMyyyy", CultureInfo.InvariantCulture).Date > leavePolicyRelaxationDate)
                        {
                            daysBeforeApplied = (request.LeaveFrom.Date - System.DateTime.Now.Date).Days;

                            if (adjustBal.Type == "Casual Leave")
                            {
                                //Apply 3 days before
                                if (daysBeforeApplied < 3)
                                {
                                    return "MinDaysForCL";
                                }
                            }
                            else if (adjustBal.Type == "Earned Leave")
                            {
                                //Apply 14 days before
                                if (daysBeforeApplied < 14)
                                {
                                    return "MinDaysForEL";
                                }
                                if (leaveDuration < 3)
                                {
                                    return "BelowMinPerRequest";
                                }
                            }
                            else if (adjustBal.Type == "Sick/Unplanned Leave")
                            {
                                //Apply for past days only
                                if ((request.LeaveUpto.Date - System.DateTime.Now.Date).Days > 0)
                                {
                                    return "MinDaysForSL";
                                }
                            }
                            //Check if Leave Exception already availed
                            if (isExceptionLeave)
                            {
                                if (empProfile.AvailedLeavePolicyException >= numberOfLeaveExceptionsAllowed)
                                {
                                    //If LeaveWithoutPay with Exception checked, allow leave else prompt message
                                    if (adjustBal.Type.ToUpper() != "LEAVE WITHOUT PAY")
                                    {
                                        return "MaxExceptionsAvailed:" + numberOfLeaveExceptionsAllowed.ToString();
                                    }
                                }
                            }
                        }
                        if (adjustBal.AdjustLeaveBalance)
                        {
                            var chkLeaveBalRec = context.EmployeeLeaveBalance.Where(e => e.UserId == request.UserId && e.LeaveTypeId == request.LeaveType && e.Year == request.LeaveFrom.Year).FirstOrDefault();
                            if (chkLeaveBalRec == null)
                                return "Leave balance profile not created";
                        }
                        IList<LeaveSummary> lstSummary = new List<LeaveSummary>();
                        lstSummary = GetLeaveSumary(request.UserId, request.LeaveFrom.Year);
                        decimal nonAdjLeavesTaken = 0;

                        bool isHalfdayRequest = false;
                        if (request.LeaveFromTime == "F")
                            isHalfdayRequest = true;

                        if (request.LeaveFromTime == "S" && (request.LeaveFrom == request.LeaveUpto))
                            isHalfdayRequest = true;

                        if (lstSummary.Count > 0)
                        {
                            var chkLeaveBal = lstSummary.Where(l => l.LeaveTypeId == request.LeaveType).FirstOrDefault();
                            if (chkLeaveBal == null)
                                nonAdjLeavesTaken = 0;
                            else
                                nonAdjLeavesTaken = chkLeaveBal.LeavesTaken + chkLeaveBal.LeavesPendingApproval;
                            if (adjustBal.AdjustLeaveBalance)
                            {
                                if (request.NumberOfDays > chkLeaveBal.LeavesBalance)
                                {
                                    return "LeaveExceeded#" + chkLeaveBal.LeavesBalance;
                                }
                            }
                        }
                        Int32 daysDiff = (request.LeaveUpto - request.LeaveFrom).Days;
                        IList<HolidayModel> holidayList = GetHolidays(request.UserId, request.LeaveFrom.Year);
                        if (holidayList.Where(x => x.HolidayDate.Date == request.LeaveFrom.Date).FirstOrDefault() != null)
                        {
                            return "HolidayFromDate";
                        }
                        if (isTimeBased == false)
                        {
                            if (holidayList.Where(x => x.HolidayDate.Date == request.LeaveUpto.Date).FirstOrDefault() != null)
                            {
                                return "HolidayToDate";
                            }
                        }
                        if (isTimeBased)
                        {
                            if (request.LeaveFrom.Date != request.LeaveUpto.Date)
                            {
                                return "PermissionDateTobeSame";
                            }
                            if (daysDiff > 0)
                                return "PermissionDateTobeSame";
                            try
                            {
                                TimeSpan timeFrom = Convert.ToDateTime(request.PermissionTimeFrom).TimeOfDay;
                                DateTime permissionDateFromTime = request.LeaveFrom + timeFrom;
                                TimeSpan timeTo = Convert.ToDateTime(request.PermissionTimeTo).TimeOfDay;
                                DateTime permissionDateToTime = request.LeaveUpto + timeTo;
                                if ((timeFrom.Hours >= 0 && timeFrom.Hours < 12) && timeTo.Hours < 12)
                                    permissionDateFromTime = permissionDateFromTime.AddDays(1);
                                if (timeTo.Hours >= 0 && timeTo.Hours < 12)
                                    permissionDateToTime = permissionDateToTime.AddDays(1);
                                if (permissionDateToTime > permissionDateFromTime)
                                {
                                    TimeSpan appliedPermissionHoursPerMonth = ReturnPermissionHoursPerMonth(request.LeaveFrom.Date.Month, request.UserId, request.LeaveType, true);
                                    if ((permissionDateToTime - permissionDateFromTime).TotalMinutes + appliedPermissionHoursPerMonth.TotalMinutes > (adjustBal.MaximumPerMonth * 60))
                                    {
                                        return "PermissionDurationTime";
                                    }
                                }
                                else
                                {
                                    return "PermissionProperTime";
                                }
                            }
                            catch
                            {
                                return "PermissionProperTime";
                            }
                        }

                        if (adjustBal.MaximumPerRequest != null)
                        {
                            if (leaveDuration > adjustBal.MaximumPerRequest)
                                return "ExceedMaxPerRequest" + adjustBal.MaximumPerRequest;
                        }

                        Leave leave = new Leave
                        {
                            AppliedAt = DateTime.Now,
                            AppliedBy = request.AppliedByUserId,
                            ApprovedAt = null,
                            ApprovedBy = null,
                            Comments = null,
                            Duration = leaveDuration,
                            UserId = request.UserId
                        };
                        if (isHalfdayRequest)
                        {
                            leave.EndDateType = null;
                        }

                        leave.EndDate = request.LeaveUpto;

                        leave.LeaveId = request.RequestId;
                        leave.LeaveTypeId = request.LeaveType;
                        leave.Remarks = request.Reason;
                        if (isTimeBased)
                        {
                            leave.EndDateType = "A";
                            leave.StartDateType = "A";
                        }
                        else
                        {
                            leave.EndDateType = request.LeaveUptoTime;
                            leave.StartDateType = request.LeaveFromTime;
                        }
                        leave.StartDate = request.LeaveFrom;

                        if (empProfile.ReportingToId == null)
                        {
                            leave.Status = "A";
                            leave.Comments = "Auto Approved on Apply.";
                            leave.ApprovedAt = DateTime.Now;
                            leave.ApprovedBy = request.UserId;
                        }
                        else
                        {
                            leave.Status = "P";
                        }
                        leave.IsExceptionLeave = request.IsExceptionTypeLeave;
                        context.Leave.Add(leave);
                        isSaved = context.SaveChanges();
                        newId = leave.LeaveId;
                        if (isSaved > 0)
                        {
                            if (isTimeBased == false)
                            {
                                LeaveDetail ld;
                                foreach (var item in request.leaveDetail)
                                {
                                    ld = new LeaveDetail
                                    {
                                        LeaveId = newId,
                                        LeaveDate = item.LeaveDayItem,
                                        PartOfDay = item.PartOfDay,
                                        LeaveDayQty = item.LeaveDayItemQty,
                                        IsDayOff = item.IsDayOff,
                                        Remarks = item.Remarks
                                    };
                                    context.LeaveDetail.Add(ld);
                                    isSaved = context.SaveChanges();
                                }
                            }
                            else
                            {
                                PermissionDetail pd = new PermissionDetail
                                {
                                    LeaveId = newId,
                                    PermissionDate = request.LeaveFrom.Date,
                                    TimeFrom = request.PermissionTimeFrom,
                                    TimeTo = request.PermissionTimeTo
                                };
                                context.PermissionDetail.Add(pd);
                                isSaved = context.SaveChanges();
                            }
                            if (isTimeBased == false)//check this suresh OCT 25
                            {
                                if (isSaved > 0)
                                {
                                    EmployeeLeaveBalance leaveBalRec = null;
                                    leaveBalRec = context.EmployeeLeaveBalance.Where(e => e.UserId == request.UserId && e.LeaveTypeId == request.LeaveType && e.Year == request.LeaveFrom.Year).FirstOrDefault();
                                    if (leaveBalRec == null)
                                    {
                                        EmployeeLeaveBalance empBal = new EmployeeLeaveBalance
                                        {
                                            UserId = request.UserId,
                                            Year = request.LeaveFrom.Year,
                                            LeaveTypeId = request.LeaveType,
                                            PendingApprovalDays = leaveDuration,
                                            CreatedBy = request.AppliedByUserId,
                                            CreatedOn = DateTime.Now,
                                            ModifiedBy = -1,
                                            ModifiedOn = DateTime.Now
                                        };
                                        context.EmployeeLeaveBalance.Add(empBal);
                                        isSaved = context.SaveChanges();
                                    }
                                    else
                                    {
                                        if (empProfile.ReportingToId == null)
                                        {
                                            leaveBalRec.LeaveTakenDays = (leaveBalRec.LeaveTakenDays ?? 0) + leaveDuration;
                                        }
                                        else
                                        {
                                            leaveBalRec.PendingApprovalDays = (leaveBalRec.PendingApprovalDays ?? 0) + leaveDuration;
                                        }
                                        if (adjustBal.AdjustLeaveBalance)
                                        {
                                            leaveBalRec.BalanceDays = (leaveBalRec.BalanceDays ?? 0) - leaveDuration;
                                        }
                                        leaveBalRec.ModifiedBy = request.AppliedByUserId;
                                        leaveBalRec.ModifiedOn = DateTime.Now;
                                        isSaved = context.SaveChanges();
                                    }

                                    if (isSaved > 0)
                                    {
                                        TransactionHistoryModel hist = new TransactionHistoryModel
                                        {
                                            EmployeeId = request.UserId,
                                            LeaveTypeId = leave.LeaveTypeId,
                                            LeaveId = leave.LeaveId,
                                            TransactionType = "D",
                                            NumberOfDays = leaveDuration,
                                            TransactionBy = request.UserId
                                        };
                                        if (empProfile.ReportingToId == null)
                                        {
                                            hist.Remarks = "Auto Approved";
                                        }
                                        else
                                        {
                                            hist.Remarks = "Pending";
                                        }
                                        //bool retRes = SaveTransactionLog(hist);

                                        LeaveTransactionHistory histRec = new LeaveTransactionHistory
                                        {
                                            UserId = hist.EmployeeId,
                                            LeaveTypeId = hist.LeaveTypeId,
                                            LeaveId = hist.LeaveId,
                                            TransactionDate = DateTime.Now,
                                            TransactionType = hist.TransactionType,
                                            NumberOfDays = hist.NumberOfDays,
                                            TransactionBy = hist.TransactionBy,
                                            Remarks = hist.Remarks
                                        };
                                        context.LeaveTransactionHistory.Add(histRec);
                                        isSaved = context.SaveChanges();
                                    }
                                }
                            }
                            if (request.IsExceptionTypeLeave)
                            {
                                //Add 1 to the AvailedLeavePolicyException
                                var emp = context.Employee.Where(x => x.UserId == leave.UserId).FirstOrDefault();
                                emp.AvailedLeavePolicyException = (emp.AvailedLeavePolicyException ?? 0) + 1;
                                emp.ModifiedBy = request.UserId;
                                emp.ModifiedOn = DateTime.Now;
                                isSaved = context.SaveChanges();
                            }
                        }
                        if (isSaved != -1)
                        {
                            transaction.Commit();
                            return "Saved$" + newId;
                        }
                        else
                        {
                            transaction.Rollback();
                            return "NotSaved";
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool CheckExceptionLeave(DateTime LeaveFrom, DateTime LeaveUpto, string LeaveFromTime, string LeaveUptoTime, Int64 UserId, Int64 LeaveType)
        {
            bool isExceptionType = false;
            bool nextWorkingDay = false;
            bool prevWorkingDay = false;
            DateTime leaveDate = LeaveFrom;

            using (var context = new NLTDDbContext())
            {
                var adjustBal = context.LeaveType.Where(e => e.LeaveTypeId == LeaveType).FirstOrDefault();
                if (adjustBal.IsLeave == false)
                {
                    return false;
                }
            }
            Int32 leaveDuration = (LeaveUpto - LeaveFrom).Days;
            DayOfWeek dw;
            string partOfDay = string.Empty;
            int addDays = 0;
            IList<HolidayModel> holidayList = GetHolidays(UserId, LeaveFrom.Year);
            IList<WeekOffDayModel> lstOffDays = ReturnWeekOffDays(UserId);
            bool isFromDayHalf = false;
            bool isToDayHalf = false;
            bool isFromDayPrvDayHalf = false;
            bool isToDayNextDayHalf = false;
            bool isNextDayFullDayLeave = false;
            bool isPrvDayFullDayLeave = false;
            bool isToDayFullDayLeave = false;
            string fromDayHalf = "";
            string toDayHalf = "";
            string fromDayPrvDayHalf = "";
            string toDayNextDayHalf = "";
            IList<LeaveListModel> lstLeaves = new List<LeaveListModel>();
            using (var context = new NLTDDbContext())
            {
                lstLeaves = (from l in context.Leave
                             join ld in context.LeaveDetail on l.LeaveId equals ld.LeaveId
                             join lt in context.LeaveType on l.LeaveTypeId equals lt.LeaveTypeId
                             where (l.Status == "P" || l.Status == "A") && ld.LeaveDate.Year == LeaveFrom.Year && ld.IsDayOff == false
                             && lt.IsLeave == true && l.UserId == UserId && lt.LeaveCombinationAllowed == false
                             select new LeaveListModel { LeaveId = l.LeaveId, LeaveDtlId = ld.LeaveDetailId, PartOfDay = ld.PartOfDay, LeaveDate = ld.LeaveDate }
             ).ToList();
            }
            if (LeaveFromTime == "A")
            {
                isToDayFullDayLeave = true;
            }
            else
            {
                if (LeaveFromTime == "F" && LeaveUptoTime == "F")
                {
                    isFromDayHalf = true;
                    fromDayHalf = "F";
                    isToDayHalf = true;
                    toDayHalf = "F";
                }
                else if (LeaveFromTime == "S" && LeaveUptoTime == "S")
                {
                    isFromDayHalf = true;
                    fromDayHalf = "S";
                    isToDayHalf = true;
                    toDayHalf = "S";
                }
                else if (LeaveFromTime == "S" && LeaveUptoTime == "F")
                {
                    isFromDayHalf = true;
                    fromDayHalf = "S";
                    isToDayHalf = true;
                    toDayHalf = "F";
                }
            }

            //Check if half day leave exists on that From day
            var leaveFrom = lstLeaves.Where(x => x.LeaveDate.Date == LeaveFrom.Date).FirstOrDefault();

            if (leaveFrom != null)
            {
                isExceptionType = true;
            }
            if (LeaveUpto > LeaveFrom)
            {
                //Check if half day leave exists on that To day
                var leaveUpto = lstLeaves.Where(x => x.LeaveDate.Date == LeaveUpto.Date).FirstOrDefault();
                if (leaveUpto != null)
                {
                    isExceptionType = true;
                }
            }
            if (isExceptionType == false)
            {
                addDays++;// To check next working day leave status
                do
                {
                    if (LeaveFrom.Date.AddDays(addDays) == LeaveFrom.Date)
                    {
                        if (LeaveFromTime == "A")
                        {
                            partOfDay = "A";
                        }
                        else
                        {
                            partOfDay = LeaveFromTime;
                        }
                    }
                    else if (LeaveFrom.Date.AddDays(addDays) == LeaveUpto.Date)
                    {
                        if (LeaveUptoTime == "A")
                        {
                            partOfDay = "A";
                        }
                        else
                        {
                            partOfDay = LeaveUptoTime;
                        }
                    }

                    dw = LeaveUpto.Date.AddDays(addDays).DayOfWeek;
                    var holDay = holidayList.Where(x => x.HolidayDate.Date == LeaveUpto.Date.AddDays(addDays)).FirstOrDefault();
                    var offDay = lstOffDays.Where(x => x.Day.ToUpper() == dw.ToString().ToUpper()).FirstOrDefault();
                    if (offDay != null)
                    {
                        //Nothing
                    }
                    else if (holDay != null)
                    {
                        //Nothing
                    }
                    else
                    {
                        //Coming to this block is working day
                        nextWorkingDay = true;
                        leaveDate = LeaveUpto.AddDays(addDays);
                        //check if any leave exists, pending/approved. SL, EL, CL
                        var leave = lstLeaves.Where(x => x.LeaveDate.Date == leaveDate.Date).FirstOrDefault();
                        if (leave != null)
                        {
                            if (leave.PartOfDay == "S")
                            {
                                isToDayNextDayHalf = true;
                                toDayNextDayHalf = "S";
                            }
                            else if (leave.PartOfDay == "F")
                            {
                                isToDayNextDayHalf = true;
                                toDayNextDayHalf = "F";
                            }
                            else
                            {
                                isNextDayFullDayLeave = true;
                            }
                        }
                        nextWorkingDay = true;
                    }
                    addDays++;
                }
                while (nextWorkingDay == false);
                //If current day half and first half, next day off can be any
                //If current day half and second half, next day off shouldn't be F/A

                if (isToDayHalf)
                {
                    if (toDayHalf == "S")
                    {
                        if (isToDayNextDayHalf)
                        {
                            if (toDayNextDayHalf == "F")
                            {
                                isExceptionType = true;
                            }
                        }
                        else if (isNextDayFullDayLeave)
                        {
                            isExceptionType = true;
                        }
                    }
                }
                //If current day is full, next day off shouldn't be F/A
                if (isToDayFullDayLeave)
                {
                    if (isToDayNextDayHalf)
                    {
                        if (toDayNextDayHalf == "F")
                        {
                            isExceptionType = true;
                        }
                    }
                    else if (isNextDayFullDayLeave)
                    {
                        isExceptionType = true;
                    }
                }
            }
            leaveDate = LeaveFrom;
            if (isExceptionType == false)
            {
                addDays = -1;
                do
                {
                    if (LeaveFrom.Date.AddDays(addDays) == LeaveFrom.Date)
                    {
                        if (LeaveFromTime == "A")
                        {
                            partOfDay = "A";
                        }
                        else
                        {
                            partOfDay = LeaveFromTime;
                        }
                    }
                    else if (LeaveFrom.Date.AddDays(addDays) == LeaveUpto.Date)
                    {
                        if (LeaveUptoTime == "A")
                        {
                            partOfDay = "A";
                        }
                        else
                        {
                            partOfDay = LeaveUptoTime;
                        }
                    }

                    dw = LeaveFrom.Date.AddDays(addDays).DayOfWeek;
                    var holDay = holidayList.Where(x => x.HolidayDate.Date == LeaveFrom.Date.AddDays(addDays)).FirstOrDefault();
                    var offDay = lstOffDays.Where(x => x.Day.ToUpper() == dw.ToString().ToUpper()).FirstOrDefault();
                    if (offDay != null)
                    {
                        //Nothing
                    }
                    else if (holDay != null)
                    {
                        //Nothing
                    }
                    else
                    {
                        leaveDate = leaveDate.AddDays(addDays);
                        //Coming to this block is working day
                        nextWorkingDay = true;
                        //check if any leave exists, pending/approved. SL, EL, CL
                        var leave = lstLeaves.Where(x => x.LeaveDate.Date == leaveDate.Date).FirstOrDefault();
                        if (leave != null)
                        {
                            if (leave.PartOfDay == "S")
                            {
                                isFromDayPrvDayHalf = true;
                                fromDayPrvDayHalf = "S";
                            }
                            else if (leave.PartOfDay == "F")
                            {
                                isFromDayPrvDayHalf = true;
                                fromDayPrvDayHalf = "F";
                            }
                            else
                            {
                                isPrvDayFullDayLeave = true;
                            }
                        }

                        prevWorkingDay = true;
                    }
                    addDays--;
                }
                while (prevWorkingDay == false);

                //If current day half and first half, previous day off shouldn't be S/A
                if (isFromDayHalf)
                {
                    if (fromDayHalf == "F")
                    {
                        if (isFromDayPrvDayHalf)
                        {
                            if (fromDayPrvDayHalf == "S")
                            {
                                isExceptionType = true;
                            }
                        }
                        else if (isPrvDayFullDayLeave)
                        {
                            isExceptionType = true;
                        }
                    }
                }

                //If current day half and second half, previous day off can be any
                //If current day is full, previous day off shouldn't be S/A
                if (isToDayFullDayLeave)
                {
                    if (isFromDayPrvDayHalf)
                    {
                        if (fromDayPrvDayHalf == "S")
                        {
                            isExceptionType = true;
                        }
                    }
                    else if (isPrvDayFullDayLeave)
                    {
                        isExceptionType = true;
                    }
                }
            }
            return isExceptionType;
        }

        public IList<LeaveDtl> GetLeaveDetailCalculation(DateTime LeaveFrom, DateTime LeaveUpto, string LeaveFromTime, string LeaveUptoTime, Int64 UserId, Int64 LeaveTyp)
        {
            string LeaveTypText = string.Empty;
            using (var context = new NLTDDbContext())
            {
                var adjustBal = context.LeaveType.Where(e => e.LeaveTypeId == LeaveTyp).FirstOrDefault();
                LeaveTypText = adjustBal.Type;
            }

            IList<LeaveDtl> leaveDetail = new List<LeaveDtl>();
            Int32 leaveDuration = (LeaveUpto - LeaveFrom).Days;
            DayOfWeek dw;
            LeaveDtl dtl;
            string partOfDay = string.Empty;
            int addDays = 0;
            decimal leaveQty = 0;
            decimal totalCount = 0;
            IList<HolidayModel> holidayList = GetHolidays(UserId, LeaveFrom.Year);
            IList<WeekOffDayModel> lstOffDays = ReturnWeekOffDays(UserId);
            do
            {
                if (LeaveFrom.Date.AddDays(addDays) == LeaveFrom.Date)
                {
                    if (LeaveFromTime == "A")
                    {
                        leaveQty = 1;
                        partOfDay = "A";
                    }
                    else
                    {
                        leaveQty = 0.5M;
                        partOfDay = LeaveFromTime;
                    }
                }
                else if (LeaveFrom.Date.AddDays(addDays) == LeaveUpto.Date)
                {
                    if (LeaveUptoTime == "A")
                    {
                        leaveQty = 1;
                        partOfDay = "A";
                    }
                    else
                    {
                        leaveQty = 0.5M;
                        partOfDay = LeaveUptoTime;
                    }
                }
                else
                {
                    leaveQty = 1;
                }
                dw = LeaveFrom.Date.AddDays(addDays).DayOfWeek;
                var holDay = holidayList.Where(x => x.HolidayDate.Date == LeaveFrom.Date.AddDays(addDays)).FirstOrDefault();
                var offDay = lstOffDays.Where(x => x.Day.ToUpper() == dw.ToString().ToUpper()).FirstOrDefault();
                if (offDay != null)
                {
                    dtl = new LeaveDtl
                    {
                        LeaveDayItem = LeaveFrom.AddDays(addDays),
                        IsDayOff = true,
                        LeaveDayItemQty = 0,
                        Remarks = "Week Off - " + dw.ToString()
                    };
                    leaveDetail.Add(dtl);
                }
                else if (holDay != null)
                {
                    dtl = new LeaveDtl
                    {
                        LeaveDayItem = LeaveFrom.AddDays(addDays),
                        IsDayOff = true,
                        LeaveDayItemQty = 0,
                        Remarks = "Holiday - " + holDay.HolidayText
                    };
                    leaveDetail.Add(dtl);
                }
                else
                {
                    dtl = new LeaveDtl
                    {
                        LeaveDayItem = LeaveFrom.AddDays(addDays),
                        IsDayOff = false,
                        LeaveDayItemQty = leaveQty
                    };
                    totalCount = totalCount + leaveQty;
                    if (LeaveTypText.Trim() == "")
                        dtl.Remarks = "Leave";
                    else
                        dtl.Remarks = LeaveTypText;
                    if (leaveQty == 1)
                        partOfDay = "A";
                    dtl.PartOfDay = partOfDay;
                    leaveDetail.Add(dtl);
                }
                addDays++;
            }
            while (addDays <= leaveDuration);
            if (leaveDetail.Count > 0)
                leaveDetail[0].Total = totalCount;
            return leaveDetail;
        }

        //public bool SaveTransactionLog(TransactionHistoryModel histModel)
        //{
        //    int isSaved = 0;
        //    using (var context = new NLTDDbContext())
        //    {
        //        LeaveTransactionHistory hist = new LeaveTransactionHistory();
        //        hist.UserId = histModel.EmployeeId;
        //        hist.LeaveTypeId = histModel.LeaveTypeId;
        //        hist.LeaveId = histModel.LeaveId;
        //        hist.TransactionDate = DateTime.Now;
        //        hist.TransactionType = histModel.TransactionType;
        //        hist.NumberOfDays = histModel.NumberOfDays;
        //        hist.TransactionBy = histModel.TransactionBy;
        //        hist.Remarks = histModel.Remarks;
        //        context.LeaveTransactionHistory.Add(hist);
        //        isSaved = context.SaveChanges();
        //    }
        //    if (isSaved > 0)
        //        return true;
        //    else
        //        return false;
        //}

        public IList<DaywiseLeaveDtlModel> GetDaywiseLeaveDtl(DateTime? FromDate, DateTime? ToDate, bool IsLeaveOnly, Int64 LeadId, bool OnlyReportedToMe, Int64? paramUserId, string reqUsr, bool DonotShowRejected)
        {
            IList<Int64> empList = GetEmployeesReporting(LeadId);
            IList<DaywiseLeaveDtlModel> retList;
            using (var context = new NLTDDbContext())
            {
                var dtlQry = (from emp in context.Employee
                              join lv in context.Leave on emp.UserId equals lv.UserId
                              join lvd in context.LeaveDetail on lv.LeaveId equals lvd.LeaveId
                              join lt in context.LeaveType on lv.LeaveTypeId equals lt.LeaveTypeId
                              where lvd.IsDayOff == false && (lvd.LeaveDate >= FromDate && lvd.LeaveDate <= ToDate) && lt.IsTimeBased == false
                              orderby emp.FirstName
                              select new DaywiseLeaveDtlModel
                              {
                                  UserId = emp.UserId,
                                  EmpId = emp.EmployeeId,
                                  Name = emp.FirstName + " " + emp.LastName,
                                  LeaveType = lt.Type,
                                  IsLeave = lt.IsLeave,
                                  LeaveDate = lvd.LeaveDate,
                                  IsDayOff = lvd.IsDayOff,
                                  Duration = lvd.LeaveDayQty,
                                  LeaveStatus = lv.Status,
                                  ReportingToId = emp.ReportingToId,
                                  AdjustLeaveBalance = lt.AdjustLeaveBalance,
                                  PartOfDay = lvd.PartOfDay,
                                  LeaveReason = lv.Remarks,
                                  ApproverComments = lv.Comments
                              }
                            ).AsQueryable();
                if (IsLeaveOnly)
                {
                    retList = dtlQry.Where(x => x.IsLeave == true).ToList();
                }
                else
                {
                    retList = dtlQry.ToList();
                }
                if (reqUsr == "My")
                {
                    retList = retList.Where(x => x.UserId == LeadId).ToList();
                }
                else
                {
                    EmployeeDac employeeDac = new EmployeeDac();
                    string leadRole = employeeDac.GetEmployeeRole(LeadId);

                    if (leadRole != "")
                    {
                        if (reqUsr == "Team")
                        {
                            if (leadRole == "ADMIN" || leadRole == "HR")
                            {
                                if (paramUserId > 0)
                                {
                                    retList = retList.Where(x => x.UserId == paramUserId).ToList();
                                }
                                else
                                {
                                    if (OnlyReportedToMe)
                                    {
                                        retList = retList.Where(x => x.ReportingToId == LeadId).ToList();
                                    }
                                    else
                                    {
                                        if (DonotShowRejected)
                                            retList = retList.Where(x => x.LeaveStatus != "R" && x.LeaveStatus != "C").ToList();
                                    }
                                }
                            }
                            else
                            {
                                if (paramUserId > 0)
                                {
                                    retList = retList.Where(t => empList.Contains(t.UserId)).OrderBy(t => t.Name).ToList();
                                    if (retList.Count > 0)
                                        retList = retList.Where(x => x.UserId == paramUserId).ToList();
                                }
                                else
                                {
                                    if (OnlyReportedToMe)
                                    {
                                        retList = retList.Where(x => x.ReportingToId == LeadId).ToList();
                                    }
                                    else
                                    {
                                        retList = retList.Where(t => empList.Contains(t.UserId)).OrderBy(t => t.Name).ToList();
                                    }
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < retList.Count; i++)
                {
                    retList[i].LeaveStatus = ReturnStatus(retList[i].LeaveStatus);
                    if (retList[i].PartOfDay == "A")
                        retList[i].PartOfDay = "Full Day";
                    else if (retList[i].PartOfDay == "F")
                        retList[i].PartOfDay = "First Half";
                    else if (retList[i].PartOfDay == "S")
                        retList[i].PartOfDay = "Second Half";
                }
            }
            if (retList.Count > 0)
                retList = retList.OrderBy(x => x.EmpId).ThenByDescending(x => x.LeaveDate).ToList();
            return retList;
        }

        public string ReturnStatus(string status)
        {
            if (status == "P")
                return "Pending";
            if (status == "A")
                return "Approved";
            if (status == "R")
                return "Rejected";
            if (status == "C")
                return "Cancelled";
            return status;
        }

        public IList<LeaveHeaderModel> GetLeaveHederDtl(Int64 UserId)
        {
            IList<LeaveHeaderModel> lstRetMdl = new List<LeaveHeaderModel>();
            using (var context = new NLTDDbContext())
            {
                var leaves = context.Leave
                                    .Where(x => x.UserId == UserId)
                                    .GroupJoin(
                                        context.LeaveDetail,
                                        o => o.LeaveId,
                                        so => so.LeaveId,
                                        (o, so) => new { headMdl = o, lstDetail = so })
                                    .ToList()
                                    .Select(r => new LeaveHeaderModel
                                    {
                                        LeaveId = r.headMdl.LeaveId,
                                        StartDate = r.headMdl.StartDate,
                                        StartDateType = r.headMdl.StartDateType,
                                        EndDate = r.headMdl.EndDate,
                                        EndDateType = r.headMdl.EndDateType,
                                        LeaveTypeId = r.headMdl.LeaveTypeId,
                                        Status = r.headMdl.Status,
                                        lstDetail = r.lstDetail.Select(so => new LeaveDetailModel
                                        {
                                            LeaveDetailId = so.LeaveDetailId,
                                            LeaveId = so.LeaveId,
                                            LeaveDayItem = so.LeaveDate,
                                            IsDayOff = so.IsDayOff,
                                            LeaveDayItemQty = so.LeaveDayQty,
                                            Remarks = so.Remarks
                                        }).ToList()
                                    }).ToList();
                var leaveTypes = (from typ in context.LeaveType
                                  select new { typ.LeaveTypeId, LeaveTypeName = typ.Type }
                                  ).ToList();

                foreach (var item in leaves)
                {
                    item.LeaveTypeName = leaveTypes.Where(x => x.LeaveTypeId == item.LeaveTypeId).FirstOrDefault().LeaveTypeName;
                    lstRetMdl.Add(item);
                }
            }

            return lstRetMdl;
        }

        public string ReturnWeekOff(Int64 UserId)
        {
            string retOffs = string.Empty;
            using (var context = new NLTDDbContext())
            {
                var offsQry = (from emp in context.Employee
                               join ew in context.EmployeeWeekOff on emp.UserId equals ew.UserId
                               join dw in context.DayOfWeek on ew.DaysOfWeekId equals dw.DaysOfWeekId
                               where emp.UserId == UserId
                               select new { OffId = dw.DayIdNo }
                             ).ToList();
                foreach (var item in offsQry)
                {
                    retOffs = retOffs + "," + item.OffId;
                }
                if (retOffs.Length > 1)
                    retOffs = retOffs.Substring(1);
                return retOffs;
            }
        }

        public IList<WeekOffDayModel> ReturnWeekOffDays(Int64 UserId)
        {
            IList<WeekOffDayModel> lstDayOff;

            using (var context = new NLTDDbContext())
            {
                var qry = (from emp in context.Employee
                           join ew in context.EmployeeWeekOff on emp.UserId equals ew.UserId
                           join dw in context.DayOfWeek on ew.DaysOfWeekId equals dw.DaysOfWeekId
                           where emp.UserId == UserId
                           select new WeekOffDayModel { Day = dw.Day }
                             ).ToList();
                lstDayOff = qry.ToList();
            }
            return lstDayOff;
        }

        public IList<PermissionDetailsModel> GetPermissionDetail(Int64? paramUserId, string reqUsr, DateTime? startDate, DateTime? endDate, bool OnlyReportedToMe, Int64 LeadId)
        {
            IList<PermissionDetailsModel> permissions = new List<PermissionDetailsModel>();
            IList<Int64> empList = GetEmployeesReporting(LeadId);
            using (var context = new NLTDDbContext())
            {
                var qry = (from emp in context.Employee
                           join l in context.Leave on emp.UserId equals l.UserId
                           join lt in context.LeaveType on l.LeaveTypeId equals lt.LeaveTypeId
                           join lp in context.PermissionDetail on l.LeaveId equals lp.LeaveId
                           where lt.IsTimeBased == true && lt.LeaveTypeId != 12
                           orderby l.StartDate descending
                           select new PermissionDetailsModel
                           {
                               EmpId = emp.EmployeeId,
                               UserId = emp.UserId,
                               ReportingToId = emp.ReportingToId,
                               Name = emp.FirstName + " " + emp.LastName,
                               PermissionDetailId = lp.PermissionDetailId,
                               LeaveId = l.LeaveId,
                               PermissionMonth = lp.PermissionDate.Month,
                               PermissionDate = lp.PermissionDate,
                               TimeFrom = lp.TimeFrom,
                               TimeTo = lp.TimeTo,
                               PermissionType = lt.Type,
                               Reason = l.Remarks,
                               ApproverComments = l.Comments,
                               Status = l.Status
                           }
                             ).AsQueryable();
                DateTime QryFromDate = startDate ?? new DateTime(DateTime.Now.Year, 1, 1);
                DateTime QryToDate = endDate ?? DateTime.Now;
                qry = qry.Where(x => x.PermissionDate >= QryFromDate && x.PermissionDate <= QryToDate);
                if (qry != null)
                {
                    if (reqUsr == "My")
                    {
                        permissions = qry.Where(x => x.UserId == LeadId).ToList();
                    }

                    EmployeeDac employeeDac = new EmployeeDac();
                    string leadRole = employeeDac.GetEmployeeRole(LeadId);

                    if (leadRole != "")
                    {
                        if (reqUsr == "Team")
                        {
                            if (leadRole == "ADMIN" || leadRole == "HR")
                            {
                                if (paramUserId > 0)
                                {
                                    permissions = qry.Where(x => x.UserId == paramUserId).ToList();
                                }
                                else
                                {
                                    if (OnlyReportedToMe)
                                    {
                                        permissions = qry.Where(x => x.ReportingToId == LeadId).ToList();
                                    }
                                    else
                                    {
                                        permissions = qry.ToList();
                                    }
                                }
                            }
                            else
                            {
                                if (paramUserId > 0)
                                {
                                    permissions = qry.Where(t => empList.Contains(t.UserId)).ToList();
                                    if (permissions.Count > 0)
                                        permissions = permissions.Where(x => x.UserId == paramUserId).ToList();
                                }
                                else
                                {
                                    if (OnlyReportedToMe)
                                    {
                                        permissions = qry.Where(x => x.ReportingToId == LeadId).ToList();
                                    }
                                    else
                                    {
                                        permissions = qry.Where(t => empList.Contains(t.UserId)).ToList();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (permissions.Count > 0)
            {
                TimeSpan timeFrom;
                DateTime permissionDateFromTime;
                TimeSpan timeTo;
                DateTime permissionDateToTime;
                for (int i = 0; i < permissions.Count; i++)
                {
                    permissions[i].Month = ReturnMonthName(permissions[i].PermissionMonth);
                    permissionDateFromTime = Convert.ToDateTime(permissions[i].TimeFrom);
                    permissionDateToTime = Convert.ToDateTime(permissions[i].TimeTo);
                    timeFrom = Convert.ToDateTime(permissions[i].TimeFrom).TimeOfDay;
                    timeTo = Convert.ToDateTime(permissions[i].TimeTo).TimeOfDay;
                    if ((timeFrom.Hours >= 0 && timeFrom.Hours < 12) && timeTo.Hours < 12)
                        permissionDateFromTime = permissionDateFromTime.AddDays(1);
                    if (timeTo.Hours >= 0 && timeTo.Hours < 12)
                        permissionDateToTime = permissionDateToTime.AddDays(1);
                    permissions[i].Duration = permissionDateToTime.Subtract(permissionDateFromTime).ToString(@"hh\:mm");
                    permissions[i].Status = ReturnStatus(permissions[i].Status);
                    if (permissions[i].PermissionType.Contains('-'))
                    {
                        string[] strPermissionType = permissions[i].PermissionType.Split('-');
                        permissions[i].PermissionType = strPermissionType[1].ToString().Trim();
                    }
                }
            }
            return permissions;
        }

        public IList<PermissionDetailsModel> GetOverTimePermissionDetail(Int64? paramUserId, string reqUsr, DateTime? startDate, DateTime? endDate, bool OnlyReportedToMe, Int64 LeadId)
        {
            IList<PermissionDetailsModel> permissions = new List<PermissionDetailsModel>();
            IList<Int64> empList = GetEmployeesReporting(LeadId);
            using (var context = new NLTDDbContext())
            {
                var qry = (from emp in context.Employee
                           join l in context.Leave on emp.UserId equals l.UserId
                           join lt in context.LeaveType on l.LeaveTypeId equals lt.LeaveTypeId
                           join lp in context.PermissionDetail on l.LeaveId equals lp.LeaveId
                           where lt.IsTimeBased == true && lt.LeaveTypeId == 12
                           orderby l.StartDate descending
                           select new PermissionDetailsModel
                           {
                               EmpId = emp.EmployeeId,
                               UserId = emp.UserId,
                               ReportingToId = emp.ReportingToId,
                               Name = emp.FirstName + " " + emp.LastName,
                               PermissionDetailId = lp.PermissionDetailId,
                               LeaveId = l.LeaveId,
                               PermissionMonth = lp.PermissionDate.Month,
                               PermissionDate = lp.PermissionDate,
                               TimeFrom = lp.TimeFrom,
                               TimeTo = lp.TimeTo,
                               PermissionType = lt.Type,
                               Reason = l.Remarks,
                               ApproverComments = l.Comments,
                               Status = l.Status
                           }
                             ).AsQueryable();
                DateTime QryFromDate = startDate ?? new DateTime(DateTime.Now.Year, 1, 1);
                DateTime QryToDate = endDate ?? DateTime.Now;
                qry = qry.Where(x => x.PermissionDate >= QryFromDate && x.PermissionDate <= QryToDate);
                qry = qry.Where(x => x.Status == "A");
                if (qry != null)
                {
                    if (reqUsr == "My")
                    {
                        permissions = qry.Where(x => x.UserId == LeadId).ToList();
                    }

                    EmployeeDac employeeDac = new EmployeeDac();
                    string leadRole = employeeDac.GetEmployeeRole(LeadId);

                    if (leadRole != "")
                    {
                        if (reqUsr == "Team")
                        {
                            if (leadRole == "ADMIN" || leadRole == "HR")
                            {
                                if (paramUserId > 0)
                                {
                                    permissions = qry.Where(x => x.UserId == paramUserId).ToList();
                                }
                                else
                                {
                                    if (OnlyReportedToMe)
                                    {
                                        permissions = qry.Where(x => x.ReportingToId == LeadId).ToList();
                                    }
                                    else
                                    {
                                        permissions = qry.ToList();
                                    }
                                }
                            }
                            else
                            {
                                if (paramUserId > 0)
                                {
                                    permissions = qry.Where(t => empList.Contains(t.UserId)).ToList();
                                    if (permissions.Count > 0)
                                        permissions = permissions.Where(x => x.UserId == paramUserId).ToList();
                                }
                                else
                                {
                                    if (OnlyReportedToMe)
                                    {
                                        permissions = qry.Where(x => x.ReportingToId == LeadId).ToList();
                                    }
                                    else
                                    {
                                        permissions = qry.Where(t => empList.Contains(t.UserId)).ToList();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (permissions.Count > 0)
            {
                TimeSpan timeFrom;
                DateTime permissionDateFromTime;
                TimeSpan timeTo;
                DateTime permissionDateToTime;
                for (int i = 0; i < permissions.Count; i++)
                {
                    EmployeeProfile EmployeeProfileObj = new EmployeeDac().GetEmployeeProfile(permissions[i].UserId);
                    string reportingManager = string.Empty;
                    if (EmployeeProfileObj != null)
                    {
                        reportingManager = EmployeeProfileObj.ReportedToName;
                    }
                    permissions[i].Month = ReturnMonthName(permissions[i].PermissionMonth);
                    permissionDateFromTime = Convert.ToDateTime(permissions[i].TimeFrom);
                    permissionDateToTime = Convert.ToDateTime(permissions[i].TimeTo);
                    timeFrom = Convert.ToDateTime(permissions[i].TimeFrom).TimeOfDay;
                    timeTo = Convert.ToDateTime(permissions[i].TimeTo).TimeOfDay;
                    if ((timeFrom.Hours >= 0 && timeFrom.Hours < 12) && timeTo.Hours < 12)
                        permissionDateFromTime = permissionDateFromTime.AddDays(1);
                    if (timeTo.Hours >= 0 && timeTo.Hours < 12)
                        permissionDateToTime = permissionDateToTime.AddDays(1);
                    permissions[i].Duration = permissionDateToTime.Subtract(permissionDateFromTime).ToString(@"hh\:mm");
                    permissions[i].Status = ReturnStatus(permissions[i].Status);
                    permissions[i].ReportingManager = (reportingManager == null ? "" : reportingManager.Trim());
                }
            }
            return permissions;
        }

        public string ReturnMonthName(int month)
        {
            switch (month)
            {
                case 1:
                    return "January";

                case 2:
                    return "February";

                case 3:
                    return "March";

                case 4:
                    return "April";

                case 5:
                    return "May";

                case 6:
                    return "June";

                case 7:
                    return "July";

                case 8:
                    return "August";

                case 9:
                    return "September";

                case 10:
                    return "October";

                case 11:
                    return "November";

                case 12:
                    return "December";

                default:
                    return null;
            }
        }

        public decimal ReturnDuration(DateTime LeaveFrom, DateTime LeaveUpto, string LeaveFromTime, string LeaveUptoTime, Int64 UserId)
        {
            IList<LeaveDtl> leaveDetail = new List<LeaveDtl>();
            Int32 leaveDuration = (LeaveUpto - LeaveFrom).Days;
            DayOfWeek dw;
            LeaveDtl dtl;
            int addDays = 0;
            decimal leaveQty = 0;
            decimal leaveCount = 0;
            IList<HolidayModel> holidayList = GetHolidays(UserId, LeaveFrom.Year);
            IList<WeekOffDayModel> lstOffDays = ReturnWeekOffDays(UserId);
            do
            {
                if (LeaveFrom.Date.AddDays(addDays) == LeaveFrom.Date)
                {
                    if (LeaveFromTime == "A")
                        leaveQty = 1;
                    else
                        leaveQty = 0.5M;
                }
                else if (LeaveFrom.Date.AddDays(addDays) == LeaveUpto.Date)
                {
                    if (LeaveUptoTime == "A")
                        leaveQty = 1;
                    else
                        leaveQty = 0.5M;
                }
                else
                {
                    leaveQty = 1;
                }
                dw = LeaveFrom.Date.AddDays(addDays).DayOfWeek;
                var holDay = holidayList.Where(x => x.HolidayDate.Date == LeaveFrom.Date.AddDays(addDays)).FirstOrDefault();
                var offDay = lstOffDays.Where(x => x.Day.ToUpper() == dw.ToString().ToUpper()).FirstOrDefault();
                if (offDay != null)
                {
                    dtl = new LeaveDtl
                    {
                        LeaveDayItem = LeaveFrom.AddDays(addDays),
                        IsDayOff = true,
                        LeaveDayItemQty = 0,
                        Remarks = "Week Off - " + dw.ToString()
                    };
                    leaveDetail.Add(dtl);
                }
                else if (holDay != null)
                {
                    dtl = new LeaveDtl
                    {
                        LeaveDayItem = LeaveFrom.AddDays(addDays),
                        IsDayOff = true,
                        LeaveDayItemQty = 0,
                        Remarks = "Holiday - " + holDay.HolidayText
                    };
                    leaveDetail.Add(dtl);
                }
                else
                {
                    dtl = new LeaveDtl
                    {
                        LeaveDayItem = LeaveFrom.AddDays(addDays),
                        IsDayOff = false,
                        LeaveDayItemQty = leaveQty,
                        Remarks = "Leave"
                };
                    leaveCount = leaveCount + leaveQty;
                    leaveDetail.Add(dtl);
                }
                addDays++;
            }
            while (addDays <= leaveDuration);
            return leaveCount;
        }

        public DashBoardModel GetDashboardData(Int64 UserId, Int64 OfficeId)
        {
            bool previousYear = false, nextYear = false;

            DashBoardModel dshMdl = new DashBoardModel
            {
                lstLeaveSummary = GetLeaveSumary(UserId, DateTime.Now.Year),
                lstHolidayModel = GetHolidaysDetails(UserId, DateTime.Now.Year, ref previousYear, ref nextYear),
                PreviousYear = previousYear,
                NextYear = nextYear,
                lstWeekOffs = GetWeekOffs(UserId),
                PendingApprovalCount = GetPendingApprovalCount(UserId),
                EmployeeCount = GetEmployeeCount(OfficeId),
                EmployeeInOfficeCount = GetEmployeeInOfficeCount(OfficeId)
            };
            return dshMdl;
        }

        public List<TimeSheetModel> GetMyTeamTimeSheet(Int64 UserID)
        {
            List<TimeSheetModel> timeSheetModelList = new List<TimeSheetModel>();
            // To Get all the employee profile under the manager or lead
            EmployeeDac employeeDac = new EmployeeDac();
            string leadRole = employeeDac.GetEmployeeRole(UserID);

            List<EmployeeProfile> employeeProfileListUnderManager = new EmployeeDac().GetReportingEmployeeProfile(UserID, leadRole, false).OrderBy(m => m.FirstName).ToList();

            for (int i = 0; i < employeeProfileListUnderManager.Count; i++)
            {
                List<TimeSheetModel> timeSheetModelListTemp = new TimeSheetDac().GetMyTimeSheet(employeeProfileListUnderManager[i].UserId, DateTime.Now.Date, DateTime.Now.Date);
                timeSheetModelList.AddRange(timeSheetModelListTemp);
            }
            return timeSheetModelList;
        }

        public int GetEmployeeInOfficeCount(Int64 OfficeId)
        {
            int count = 0;
            DateTime dateTime = DateTime.Now;
            using (var context = new NLTDDbContext())
            {
                var qry = (from e in context.EmployeeAttendance
                           join emp in context.Employee on e.UserID equals emp.UserId
                           join s in context.ShiftMapping on e.UserID equals s.UserID
                           join sm in context.ShiftMaster on s.ShiftID equals sm.ShiftID
                           where emp.OfficeId == OfficeId && DbFunctions.TruncateTime(e.InOutDate) == DbFunctions.TruncateTime(dateTime)
                                 && DbFunctions.TruncateTime(s.ShiftDate) == DbFunctions.TruncateTime(dateTime)
                                 && (DbFunctions.CreateTime(e.InOutDate.Hour > (23 - BSB) ? 23 : e.InOutDate.Hour + BSB, e.InOutDate.Minute, e.InOutDate.Second) >= sm.FromTime
                                 && DbFunctions.CreateTime(e.InOutDate.Hour, e.InOutDate.Minute, e.InOutDate.Second) <= (sm.ToTime.Hours > 9 ? sm.ToTime : new TimeSpan(23, 59, 59)))
                           select new { userID = e.UserID }
                );

                count = qry.ToList().Distinct().Count();
            }
            return count;
        }

        public int GetEmployeeCount(Int64 OfficeId)
        {
            int count = 0;
            using (var context = new NLTDDbContext())
            {
                count = context.Employee.Where(x => x.OfficeId == OfficeId && x.IsActive == true).Count();
            }
            return count;
        }

        public int GetPendingApprovalCount(Int64 userId)
        {
            int count = 0;

            using (var context = new NLTDDbContext())
            {
                var qry = (from l in context.Leave
                           join emp in context.Employee on l.UserId equals emp.UserId
                           where l.Status == "P" && emp.ReportingToId == userId
                           select new { l.LeaveId }
                         ).AsQueryable();

                count = qry.Count();
            }
            return count;
        }

        public LeaveRequestModel ApplyLeaveCommonData(Int64 OfficeId, Int64 UserId)
        {
            LeaveRequestModel lrm = new LeaveRequestModel
            {
                lstLeaveTypes = GetLeaveTypes(OfficeId, UserId),
                lstSummary = GetLeaveSumary(UserId, DateTime.Now.Year),
                WeekOffs = ReturnWeekOff(UserId),
                holidayDates = GetHolidayDates(UserId, DateTime.Now.Year),
                TimebasedLeaveTypeIds = GetTimeBasedLeaveTypesString(OfficeId, UserId)
            };
            return lrm;
        }

        public IList<LeaveDetailModel> ShowLeaveDetail(Int64 LeaveId)
        {
            IList<LeaveDetailModel> lstDtl;
            using (var context = new NLTDDbContext())
            {
                var qry = (from ld in context.LeaveDetail
                           where ld.LeaveId == LeaveId
                           select new LeaveDetailModel
                           {
                               LeaveId = ld.LeaveId,
                               LeaveDayItemQty = ld.LeaveDayQty,
                               LeaveDayItem = ld.LeaveDate,
                               Remarks = ld.Remarks
                           }).AsQueryable();
                lstDtl = qry.ToList();
            }

            return lstDtl;
        }

        public IList<MonthwiseLeavesCountModel> GetMonthwiseLeavesCount(Int32 year, Int64 LeadId, bool OnlyReportedToMe, Int64? paramUserId, string reqUsr)
        {
            IList<Int64> empList = GetEmployeesReporting(LeadId);
            IList<MonthwiseLeavesCountModel> lstMdl = new List<MonthwiseLeavesCountModel>();
            IList<DaywiseLeaveDtlModel> retList;
            using (var context = new NLTDDbContext())
            {
                var dtlQry = (from e in context.Employee
                              join lv in context.Leave on e.UserId equals lv.UserId
                              join lvd in context.LeaveDetail on lv.LeaveId equals lvd.LeaveId
                              join lt in context.LeaveType on lv.LeaveTypeId equals lt.LeaveTypeId
                              where lvd.IsDayOff == false && lvd.LeaveDate.Year == year && lt.IsTimeBased == false && lt.IsLeave == true && (lv.Status == "A" || lv.Status == "P")
                              orderby e.FirstName
                              select new DaywiseLeaveDtlModel
                              {
                                  UserId = e.UserId,
                                  EmpId = e.EmployeeId,
                                  Name = e.FirstName + " " + e.LastName,
                                  ReportingToId = e.ReportingToId,
                                  LeaveType = lt.Type,
                                  LeaveDate = lvd.LeaveDate,
                                  Duration = lvd.LeaveDayQty
                              }
                            ).AsQueryable();

                retList = dtlQry.ToList();
                if (reqUsr == "My")
                {
                    retList = retList.Where(x => x.UserId == LeadId).ToList();
                }
                else
                {
                    EmployeeDac employeeDac = new EmployeeDac();
                    string leadRole = employeeDac.GetEmployeeRole(LeadId);

                    if (leadRole != "")
                    {
                        if (reqUsr == "Team")
                        {
                            if (leadRole == "ADMIN" || leadRole == "HR")
                            {
                                if (paramUserId > 0)
                                {
                                    retList = retList.Where(x => x.UserId == paramUserId).ToList();
                                }
                                else
                                {
                                    if (OnlyReportedToMe)
                                    {
                                        retList = retList.Where(x => x.ReportingToId == LeadId).ToList();
                                    }
                                    else
                                    {
                                        retList = retList.ToList();
                                    }
                                }
                            }
                            else
                            {
                                if (paramUserId > 0)
                                {
                                    retList = retList.Where(t => empList.Contains(t.UserId)).OrderBy(t => t.Name).ToList();
                                    if (retList.Count > 0)
                                        retList = retList.Where(x => x.UserId == paramUserId).ToList();
                                }
                                else
                                {
                                    if (OnlyReportedToMe)
                                    {
                                        retList = retList.Where(x => x.ReportingToId == LeadId).ToList();
                                    }
                                    else
                                    {
                                        retList = retList.Where(t => empList.Contains(t.UserId)).OrderBy(t => t.Name).ToList();
                                    }
                                }
                            }
                        }
                    }
                }

                IList<MonthwiseCountEmp> newList = new List<MonthwiseCountEmp>();

                if (paramUserId != null && paramUserId != 0)
                {
                    if (retList.Count > 0)
                    {
                        retList = retList.Where(x => x.UserId == paramUserId).ToList();

                        newList = retList.GroupBy(x => new { x.EmpId, x.Name, x.LeaveType, x.LeaveDate.Month })
                             .Select(y => new MonthwiseCountEmp
                             {
                                 EmpId = y.Key.EmpId,
                                 Name = y.Key.Name,
                                 LeaveType = y.Key.LeaveType,
                                 Month = y.Key.Month,
                                 Duration = y.Sum(x => x.Duration)
                             }
                             ).ToList();
                    }
                }
                else
                {
                    newList = retList.GroupBy(x => new { x.EmpId, x.Name, x.LeaveType, x.LeaveDate.Month })
                 .Select(y => new MonthwiseCountEmp
                 {
                     EmpId = y.Key.EmpId,
                     Name = y.Key.Name,
                     LeaveType = y.Key.LeaveType,
                     Month = y.Key.Month,
                     Duration = y.Sum(x => x.Duration)
                 }
                 ).ToList();

                    MonthwiseCountEmp emp;
                    EmployeeDac employeeDac = new EmployeeDac();
                    string leadRole = employeeDac.GetEmployeeRole(LeadId);

                    if (leadRole != "")
                    {
                        if (reqUsr == "Team")
                        {
                            if (leadRole == "ADMIN" || leadRole == "HR")
                            {
                                if (OnlyReportedToMe)
                                {
                                    var allEmp = context.Employee.Where(x => x.ReportingToId == LeadId).ToList();
                                    foreach (var item in allEmp)
                                    {
                                        if (retList.Where(x => x.EmpId == item.EmployeeId).FirstOrDefault() == null)//This employee didn't take leaves, we have to show zero.
                                        {
                                            emp = new MonthwiseCountEmp
                                            {
                                                EmpId = item.EmployeeId,
                                                Name = item.FirstName + " " + item.LastName
                                            };
                                            newList.Add(emp);
                                        }
                                    }
                                }
                                else
                                {
                                    var allEmp = context.Employee.Where(x => x.IsActive == true).ToList();
                                    foreach (var item in allEmp)
                                    {
                                        if (retList.Where(x => x.EmpId == item.EmployeeId).FirstOrDefault() == null)//This employee didn't take leaves, we have to show zero.
                                        {
                                            emp = new MonthwiseCountEmp
                                            {
                                                EmpId = item.EmployeeId,
                                                Name = item.FirstName + " " + item.LastName
                                            };
                                            newList.Add(emp);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (OnlyReportedToMe)
                                {
                                    var allEmp = context.Employee.Where(x => x.ReportingToId == LeadId).ToList();
                                    foreach (var item in allEmp)
                                    {
                                        if (retList.Where(x => x.EmpId == item.EmployeeId).FirstOrDefault() == null)//This employee didn't take leaves, we have to show zero.
                                        {
                                            emp = new MonthwiseCountEmp
                                            {
                                                EmpId = item.EmployeeId,
                                                Name = item.FirstName + " " + item.LastName
                                            };
                                            newList.Add(emp);
                                        }
                                    }
                                }
                                else
                                {
                                    var allEmp = context.Employee.Where(t => empList.Contains(t.UserId)).ToList();
                                    foreach (var item in allEmp)
                                    {
                                        if (retList.Where(x => x.EmpId == item.EmployeeId).FirstOrDefault() == null)//This employee didn't take leaves, we have to show zero.
                                        {
                                            emp = new MonthwiseCountEmp
                                            {
                                                EmpId = item.EmployeeId,
                                                Name = item.FirstName + " " + item.LastName
                                            };
                                            newList.Add(emp);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (newList.Count > 0)
                {
                    var empIds = newList.GroupBy(x => new { x.EmpId }).Select(y => y.First()).ToList();
                    MonthwiseLeavesCountModel mdl;

                    foreach (var item in empIds)
                    {
                        mdl = new MonthwiseLeavesCountModel
                        {
                            EmpId = item.EmpId,
                            Name = item.Name,
                            CL1 = newList.Where(x => x.Month == 1 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 1 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL1 = newList.Where(x => x.Month == 1 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 1 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL1 = newList.Where(x => x.Month == 1 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 1 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP1 = newList.Where(x => x.Month == 1 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 1 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO1 = newList.Where(x => x.Month == 1 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 1 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL2 = newList.Where(x => x.Month == 2 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 2 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL2 = newList.Where(x => x.Month == 2 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 2 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL2 = newList.Where(x => x.Month == 2 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 2 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP2 = newList.Where(x => x.Month == 2 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 2 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO2 = newList.Where(x => x.Month == 2 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 2 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL3 = newList.Where(x => x.Month == 3 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 3 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL3 = newList.Where(x => x.Month == 3 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 3 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL3 = newList.Where(x => x.Month == 3 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 3 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP3 = newList.Where(x => x.Month == 3 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 3 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO3 = newList.Where(x => x.Month == 3 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 3 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL4 = newList.Where(x => x.Month == 4 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 4 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL4 = newList.Where(x => x.Month == 4 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 4 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL4 = newList.Where(x => x.Month == 4 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 4 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP4 = newList.Where(x => x.Month == 4 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 4 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO4 = newList.Where(x => x.Month == 4 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 4 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL5 = newList.Where(x => x.Month == 5 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 5 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL5 = newList.Where(x => x.Month == 5 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 5 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL5 = newList.Where(x => x.Month == 5 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 5 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP5 = newList.Where(x => x.Month == 5 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 5 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO5 = newList.Where(x => x.Month == 5 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 5 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL6 = newList.Where(x => x.Month == 6 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 6 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL6 = newList.Where(x => x.Month == 6 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 6 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL6 = newList.Where(x => x.Month == 6 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 6 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP6 = newList.Where(x => x.Month == 6 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 6 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO6 = newList.Where(x => x.Month == 6 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 6 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL7 = newList.Where(x => x.Month == 7 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 7 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL7 = newList.Where(x => x.Month == 7 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 7 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL7 = newList.Where(x => x.Month == 7 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 7 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP7 = newList.Where(x => x.Month == 7 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 7 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO7 = newList.Where(x => x.Month == 7 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 7 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL8 = newList.Where(x => x.Month == 8 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 8 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL8 = newList.Where(x => x.Month == 8 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 8 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL8 = newList.Where(x => x.Month == 8 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 8 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP8 = newList.Where(x => x.Month == 8 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 8 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO8 = newList.Where(x => x.Month == 8 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 8 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL9 = newList.Where(x => x.Month == 9 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 9 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL9 = newList.Where(x => x.Month == 9 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 9 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL9 = newList.Where(x => x.Month == 9 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 9 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP9 = newList.Where(x => x.Month == 9 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 9 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO9 = newList.Where(x => x.Month == 9 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 9 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL10 = newList.Where(x => x.Month == 10 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 10 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL10 = newList.Where(x => x.Month == 10 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 10 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL10 = newList.Where(x => x.Month == 10 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 10 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP10 = newList.Where(x => x.Month == 10 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 10 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO10 = newList.Where(x => x.Month == 10 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 10 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL11 = newList.Where(x => x.Month == 11 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 11 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL11 = newList.Where(x => x.Month == 11 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 11 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL11 = newList.Where(x => x.Month == 11 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 11 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP11 = newList.Where(x => x.Month == 11 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 11 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO11 = newList.Where(x => x.Month == 11 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 11 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration,

                            CL12 = newList.Where(x => x.Month == 12 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 12 && x.LeaveType == "Casual/Sick Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            PL12 = newList.Where(x => x.Month == 12 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 12 && x.LeaveType == "Earned Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            DL12 = newList.Where(x => x.Month == 12 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 12 && x.LeaveType == "Debit Leave" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            LWP12 = newList.Where(x => x.Month == 12 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 12 && x.LeaveType == "Leave Without Pay" && x.EmpId == item.EmpId).FirstOrDefault().Duration,
                            CO12 = newList.Where(x => x.Month == 12 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault() == null ? 0 : newList.Where(x => x.Month == 12 && x.LeaveType == "Compensatory Off" && x.EmpId == item.EmpId).FirstOrDefault().Duration
                        };

                        lstMdl.Add(mdl);
                    }
                }
            }
            if (lstMdl.Count > 0)
                lstMdl = lstMdl.OrderBy(x => x.EmpId).ToList();
            return lstMdl;
        }

        public string CheckIfAuthLeaveId(Int64 leaveId, Int64 userId)
        {
            string retString = string.Empty;
            using (var context = new NLTDDbContext())
            {
                var leaveQry = (from leave in context.Leave
                                join types in context.LeaveType on leave.LeaveTypeId equals types.LeaveTypeId
                                join user in context.Employee on leave.UserId equals user.UserId
                                where leave.LeaveId == leaveId
                                select new
                                {
                                    leaveStatus = leave.Status,
                                    ReportingTo = user.ReportingToId
                                }).FirstOrDefault();

                if (leaveQry == null)
                {
                    retString = "The request Id is invalid.";
                }
                else
                {
                    if (leaveQry.ReportingTo != userId)
                        retString = "You are not authorized to access this request.";
                }
            }
            if (retString == "")
                retString = "Valid";
            return retString;
        }

        public IList<string> FilterOptoutEmail(IList<string> lstCCEmail, long? reportingToUserId)
        {
            try
            {
                if (lstCCEmail.Count > 0 && reportingToUserId != null)
                {
                    List<string> lstOptoutEmailAddress = GetOnlyDirectAlertsEmailIds();
                    using (var context = new NLTDDbContext())
                    {
                        var qryReportingTo = context.Employee.Where(x => x.UserId == reportingToUserId).FirstOrDefault();

                        foreach (var item in lstOptoutEmailAddress)
                        {
                            if (qryReportingTo.EmailAddress.ToUpper() != item.ToUpper())//If employee is not directly reporting to person in OptOut list, remove email from cc list.
                            {
                                lstCCEmail.Remove(item);
                            }
                        }
                    }
                }
            }
            catch { throw; }

            return lstCCEmail;
        }

        private List<string> GetOnlyDirectAlertsEmailIds()
        {
            List<string> onlyDirectAlertsEmailIdsList = new List<string>();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    onlyDirectAlertsEmailIdsList = context.Employee.Where(x => x.OnlyDirectAlerts == true).Select(x => x.EmailAddress).ToList();
                }
            }
            catch { throw; }
            return onlyDirectAlertsEmailIdsList;
        }

        public EmailDataModel GetEmailData(Int64 leaveId, string actionName)
        {
            EmailDataModel retMdl = new EmailDataModel();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    var qry = (from e in context.Employee
                               join l in context.Leave on e.UserId equals l.UserId
                               join lt in context.LeaveType on l.LeaveTypeId equals lt.LeaveTypeId
                               where l.LeaveId == leaveId
                               select new EmailDataModel
                               {
                                   RequestFor = e.FirstName + " " + e.LastName,
                                   EmpId = e.EmployeeId,
                                   LeaveTypeText = lt.Type,
                                   DateFrom = l.StartDate,
                                   DateTo = l.EndDate,
                                   ReportingToId = e.ReportingToId,
                                   NoOfDays = l.Duration,
                                   IsTimeBased = lt.IsTimeBased,
                                   RequestorEmailId = e.EmailAddress,
                                   FromType = l.StartDateType,
                                   ToType = l.EndDateType,
                                   Reason = l.Remarks,
                                   ApproverComments = l.Comments,
                                   LeaveId = leaveId
                               }
                             ).FirstOrDefault();

                    qry.CcEmailIds = GetHigherApproversEmailIds(qry.ReportingToId);
                    var hrEmail = (from e in context.Employee
                                   join er in context.EmployeeRole on e.EmployeeRoleId equals er.RoleId
                                   where (er.Role == "HR" || er.Role == "Admin") && e.IsActive == true
                                   select new { EmailId = e.EmailAddress }
                                 ).ToList();
                    foreach (var item in hrEmail)
                    {
                        qry.CcEmailIds.Add(item.EmailId);
                    }

                    var qryReportingTo = context.Employee.Where(x => x.UserId == qry.ReportingToId).FirstOrDefault();
                    if (qryReportingTo == null)
                    {
                        qry.ToEmailId = "AutoApproved";
                    }
                    else
                    {
                        qry.ReportingToName = qryReportingTo.FirstName + " " + qryReportingTo.LastName;
                        qry.ToEmailId = qryReportingTo.EmailAddress;
                    }
                    if (actionName == "Pending")
                    {
                        qry.CcEmailIds.Remove(qry.ToEmailId);
                        qry.CcEmailIds.Add(qry.RequestorEmailId);
                    }
                    else
                    {
                        qry.ToEmailId = qry.RequestorEmailId;
                    }

                    //optout
                    qry.CcEmailIds = FilterOptoutEmail(qry.CcEmailIds, qry.ReportingToId);

                    if (qry.IsTimeBased)
                    {
                        var TimeDuration = context.PermissionDetail.Where(x => x.LeaveId == leaveId).FirstOrDefault();
                        qry.Date = TimeDuration.PermissionDate.Date.ToString("dd-MM-yyyy");
                        qry.Duration = TimeDuration.TimeFrom + " to " + TimeDuration.TimeTo;
                    }
                    else
                    {
                        if (qry.FromType == "A")
                            qry.FromType = "Full Day";
                        else if (qry.FromType == "F")
                            qry.FromType = "First Half";
                        else if (qry.FromType == "S")
                            qry.FromType = "Second Half";

                        if (qry.ToType == "A")
                            qry.ToType = "Full Day";
                        else if (qry.ToType == "F")
                            qry.ToType = "First Half";
                        else if (qry.ToType == "S")
                            qry.ToType = "Second Half";

                        if (qry.DateFrom.Date == qry.DateTo.Date)
                        {
                            if (qry.FromType == "First Half" || qry.FromType == "Second Half")
                                qry.ToType = "";
                        }
                        if (qry.ToType == "")
                        {
                            qry.Date = qry.DateFrom.Date.ToString("dd-MM-yyyy") + "(" + qry.FromType + ")" + " to " + qry.DateTo.Date.ToString("dd-MM-yyyy");
                        }
                        else
                        {
                            qry.Date = qry.DateFrom.Date.ToString("dd-MM-yyyy") + "(" + qry.FromType + ")" + " to " + qry.DateTo.Date.ToString("dd-MM-yyyy") + "(" + qry.ToType + ")";
                        }
                        qry.Duration = Convert.ToString(qry.NoOfDays) + " " + "Day(s).";
                    }
                    retMdl = qry;
                }

                return retMdl;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public EmailDataModel GetEmailDataAddLeave(Int64 userID, Int64? leaveTypeID)
        {
            EmailDataModel retMdl = new EmailDataModel();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    var qry = (from e in context.Employee
                               join lt in context.LeaveType on leaveTypeID equals lt.LeaveTypeId
                               where e.UserId == userID
                               select new EmailDataModel
                               {
                                   RequestFor = e.FirstName + " " + e.LastName,
                                   EmpId = e.EmployeeId,
                                   ReportingToId = e.ReportingToId,
                                   RequestorEmailId = e.EmailAddress,
                                   LeaveTypeText = lt.Type
                               }
                            ).FirstOrDefault();
                    qry.CcEmailIds = GetHigherApproversEmailIds(qry.ReportingToId);
                    var hrEmail = (from e in context.Employee
                                   join er in context.EmployeeRole on e.EmployeeRoleId equals er.RoleId
                                   where (er.Role == "HR" || er.Role == "Admin") && e.IsActive == true
                                   select new { EmailId = e.EmailAddress }
                                 ).ToList();
                    foreach (var item in hrEmail)
                    {
                        qry.CcEmailIds.Add(item.EmailId);
                    }
                    //optout
                    qry.CcEmailIds = FilterOptoutEmail(qry.CcEmailIds, qry.ReportingToId);

                    retMdl = qry;
                }
                return retMdl;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public EmailDataModel ViewLeaveFromEmail(Int64 leaveId, Int64 userId)
        {
            EmailDataModel retMdl = new EmailDataModel();
            string checkAuth = CheckIfAuthLeaveId(leaveId, userId);
            try
            {
                if (checkAuth == "Valid")
                {
                    using (var context = new NLTDDbContext())
                    {
                        var qry = (from e in context.Employee
                                   join l in context.Leave on e.UserId equals l.UserId
                                   join lt in context.LeaveType on l.LeaveTypeId equals lt.LeaveTypeId
                                   where l.LeaveId == leaveId
                                   select new EmailDataModel
                                   {
                                       RequestFor = e.FirstName + " " + e.LastName,
                                       EmpId = e.EmployeeId,
                                       LeaveTypeText = lt.Type,
                                       DateFrom = l.StartDate,
                                       DateTo = l.EndDate,
                                       ReportingToId = e.ReportingToId,
                                       NoOfDays = l.Duration,
                                       IsTimeBased = lt.IsTimeBased,
                                       RequestorEmailId = e.EmailAddress,
                                       FromType = l.StartDateType,
                                       ToType = l.EndDateType,
                                       Reason = l.Remarks,
                                       ApproverComments = l.Comments,
                                       LeaveId = leaveId
                                   }
                                 ).FirstOrDefault();
                        if (qry.ReportingToId != null)
                        {
                            var qryReportingTo = context.Employee.Where(x => x.UserId == qry.ReportingToId).FirstOrDefault();
                            qry.ReportingToName = qryReportingTo.FirstName + " " + qryReportingTo.LastName;
                        }
                        if (qry.IsTimeBased)
                        {
                            var TimeDuration = context.PermissionDetail.Where(x => x.LeaveId == leaveId).FirstOrDefault();
                            qry.Date = TimeDuration.PermissionDate.Date.ToString("dd-MM-yyyy");
                            qry.Duration = TimeDuration.TimeFrom + " to " + TimeDuration.TimeTo;
                        }
                        else
                        {
                            if (qry.FromType == "A")
                                qry.FromType = "Full Day";
                            else if (qry.FromType == "F")
                                qry.FromType = "First Half";
                            else if (qry.FromType == "S")
                                qry.FromType = "Second Half";

                            if (qry.ToType == "A")
                                qry.ToType = "Full Day";
                            else if (qry.ToType == "F")
                                qry.ToType = "First Half";
                            else if (qry.ToType == "S")
                                qry.ToType = "Second Half";

                            if (qry.ToType == "")
                            {
                                qry.Date = qry.DateFrom.Date.ToString("dd-MM-yyyy") + "(" + qry.FromType + ")" + " to " + qry.DateTo.Date.ToString("dd-MM-yyyy");
                            }
                            else
                            {
                                qry.Date = qry.DateFrom.Date.ToString("dd-MM-yyyy") + "(" + qry.FromType + ")" + " to " + qry.DateTo.Date.ToString("dd-MM-yyyy") + "(" + qry.ToType + ")";
                            }

                            qry.Date = qry.DateFrom.Date.ToString("dd-MM-yyyy") + "(" + qry.FromType + ")" + " to " + qry.DateTo.Date.ToString("dd-MM-yyyy") + "(" + qry.ToType + ")";
                            qry.Duration = Convert.ToString(qry.NoOfDays);
                        }
                        retMdl = qry;
                    }
                }
                else
                {
                    retMdl.AuthError = checkAuth;
                }
                return retMdl;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Dispose()
        {
            //Nothing to dispose...
        }
    }
}