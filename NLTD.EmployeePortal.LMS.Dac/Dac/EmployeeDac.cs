using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.DbModel;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public class EmployeeDac : IEmployeeHelper
    {
        public EmployeeProfile GetEmployeeProfile(Int64 userId)
        {
            EmployeeProfile profile = null;
            try
            {
                using (var context = new NLTDDbContext())
                {
                    profile = (from employee in context.Employee
                               join rt in context.EmployeeRole on employee.EmployeeRoleId equals rt.RoleId
                               where employee.UserId == userId
                               select new EmployeeProfile
                               {
                                   EmailAddress = employee.EmailAddress,
                                   EmployeeId = employee.EmployeeId,
                                   FirstName = employee.FirstName,
                                   LastName = employee.LastName,
                                   Gender = employee.Gender,
                                   OfficeHolidayId = employee.OfficeHolidayId,
                                   EmploymentTypeId = employee.EmploymentTypeId,
                                   OfficeId = employee.OfficeId,
                                   LocationText = "",
                                   MobileNumber = employee.MobileNumber,
                                   ReportedToId = employee.ReportingToId,
                                   RoleId = employee.EmployeeRoleId,
                                   RoleText = rt.Role,
                                   UserId = employee.UserId,
                                   Avatar = employee.AvatarUrl,
                                   CardId = employee.Cardid,
                                   ShiftId = employee.ShiftId,
                                   ConfirmationDate = employee.ConfirmationDate,
                                   DOJ = employee.DOJ,
                                   RelievingDate = employee.RelievingDate,
                                   LogonId = employee.LoginId,
                                   IsActive = employee.IsActive
                               }).FirstOrDefault();
                    if (profile != null)
                    {
                        if (profile.ReportedToId != null)
                        {
                            var repName = context.Employee.Where(x => x.UserId == profile.ReportedToId).FirstOrDefault();
                            if (repName != null)
                            {
                                profile.ReportedToName = repName.FirstName + " " + repName.LastName;
                            }
                        }
                        var weekOffs = (from e in context.Employee
                                        join wo in context.EmployeeWeekOff on e.UserId equals wo.UserId
                                        join w in context.DayOfWeek on wo.DaysOfWeekId equals w.DaysOfWeekId
                                        where e.UserId == profile.UserId
                                        select new { w.Day }
                                      ).ToList();

                        if (weekOffs.Count > 0)
                        {
                            foreach (var item in weekOffs)
                            {
                                switch (item.Day.ToUpper())
                                {
                                    case "SUNDAY":
                                        profile.Sunday = true;
                                        break;

                                    case "MONDAY":
                                        profile.Monday = true;
                                        break;

                                    case "TUESDAY":
                                        profile.Tuesday = true;
                                        break;

                                    case "WEDNESDAY":
                                        profile.Wednesday = true;
                                        break;

                                    case "THURSDAY":
                                        profile.Thursday = true;
                                        break;

                                    case "FRIDAY":
                                        profile.Friday = true;
                                        break;

                                    case "SATURDAY":
                                        profile.Saturday = true;
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return profile;
        }

        public string GetEmployeeRole(Int64 userId)
        {
            string userRole;
            using (var context = new NLTDDbContext())
            {
                userRole = (from emp in context.Employee
                            join role in context.EmployeeRole on emp.EmployeeRoleId equals role.RoleId
                            where emp.UserId == userId
                            select role.Role).FirstOrDefault();
            }

            if (userRole == null)
            {
                userRole = "";
            }

            return userRole.ToUpper();
        }

        public List<EmployeeProfile> GetReportingEmployeeProfile(Int64 userId, string role, bool myDirectEmployees)
        {
            List<EmployeeProfile> employeeProfileList = new List<EmployeeProfile>();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    if ((role.ToUpper() == "ADMIN" || role.ToUpper() == "HR") && !myDirectEmployees)
                    {
                        // If the user role is admin we retrieve all the employees in the company
                        employeeProfileList = (from employee in context.Employee
                                               join rt in context.EmployeeRole on employee.EmployeeRoleId equals rt.RoleId
                                               where employee.IsActive == true
                                               select new EmployeeProfile
                                               {
                                                   EmailAddress = employee.EmailAddress,
                                                   EmployeeId = employee.EmployeeId,
                                                   FirstName = employee.FirstName,
                                                   LastName = employee.LastName,
                                                   Gender = employee.Gender,
                                                   OfficeHolidayId = employee.OfficeHolidayId,
                                                   OfficeId = employee.OfficeId,
                                                   LocationText = "",
                                                   MobileNumber = employee.MobileNumber,
                                                   ReportedToId = employee.ReportingToId,
                                                   RoleId = employee.EmployeeRoleId,
                                                   RoleText = rt.Role,
                                                   UserId = employee.UserId,
                                                   Avatar = employee.AvatarUrl,
                                                   LogonId = employee.LoginId,
                                                   IsActive = employee.IsActive
                                               }).ToList();
                    }
                    else//for Team Lead
                    {
                        if (myDirectEmployees)
                        {
                            employeeProfileList = (from employee in context.Employee
                                                   join rt in context.EmployeeRole on employee.EmployeeRoleId equals rt.RoleId
                                                   where employee.IsActive == true && employee.ReportingToId == userId
                                                   select new EmployeeProfile
                                                   {
                                                       EmailAddress = employee.EmailAddress,
                                                       EmployeeId = employee.EmployeeId,
                                                       FirstName = employee.FirstName,
                                                       LastName = employee.LastName,
                                                       Gender = employee.Gender,
                                                       OfficeHolidayId = employee.OfficeHolidayId,
                                                       OfficeId = employee.OfficeId,
                                                       LocationText = "",
                                                       MobileNumber = employee.MobileNumber,
                                                       ReportedToId = employee.ReportingToId,
                                                       RoleId = employee.EmployeeRoleId,
                                                       RoleText = rt.Role,
                                                       UserId = employee.UserId,
                                                       Avatar = employee.AvatarUrl,
                                                       LogonId = employee.LoginId,
                                                       IsActive = employee.IsActive
                                                   }).ToList();
                        }
                        else
                        {
                            IList<Int64> reportingEmployeeList = GetEmployeesReporting(userId);
                            employeeProfileList = (from employee in context.Employee
                                                   join rt in context.EmployeeRole on employee.EmployeeRoleId equals rt.RoleId
                                                   where employee.IsActive == true && reportingEmployeeList.Contains(employee.UserId)
                                                   select new EmployeeProfile
                                                   {
                                                       EmailAddress = employee.EmailAddress,
                                                       EmployeeId = employee.EmployeeId,
                                                       FirstName = employee.FirstName,
                                                       LastName = employee.LastName,
                                                       Gender = employee.Gender,
                                                       OfficeHolidayId = employee.OfficeHolidayId,
                                                       OfficeId = employee.OfficeId,
                                                       LocationText = "",
                                                       MobileNumber = employee.MobileNumber,
                                                       ReportedToId = employee.ReportingToId,
                                                       RoleId = employee.EmployeeRoleId,
                                                       RoleText = rt.Role,
                                                       UserId = employee.UserId,
                                                       Avatar = employee.AvatarUrl,
                                                       LogonId = employee.LoginId,
                                                       IsActive = employee.IsActive
                                                   }).ToList();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return employeeProfileList;
        }

        public IList<Int64> GetEmployeesReporting(long leadId)
        {
            var result = new List<Int64>();
            try
            {
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
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public IList<LeaveCreditModel> GetEmployeeProfilesforEL(DateTime lastCreditRun)
        {
            IList<LeaveCreditModel> empProfileModel = new List<LeaveCreditModel>();
            DateTime toDate = DateTime.Now.AddMonths(-1);
            lastCreditRun = new DateTime(lastCreditRun.Year, lastCreditRun.Month, 1);
            try
            {
                Int32 year = lastCreditRun.Year;
                long leaveTypeId = 2;

                using (var context = new NLTDDbContext())
                {
                    var empProfile = (from e in context.Employee
                                      join elb in context.EmployeeLeaveBalance on
                                      new { p1 = e.UserId, p2 = year, p3 = leaveTypeId }
                                      equals
                                      new { p1 = elb.UserId, p2 = elb.Year, p3 = elb.LeaveTypeId }
                                      into leaveBal
                                      from lb in leaveBal.DefaultIfEmpty()
                                      where e.IsActive == true && e.EmploymentTypeId == 1
                                      orderby e.FirstName
                                      select new LeaveCreditModel
                                      {
                                          UserId = e.UserId,
                                          EmployeeId = e.EmployeeId,
                                          Name = e.FirstName + " " + e.LastName,
                                          DOJ = e.DOJ,
                                          ConfirmationDate = e.ConfirmationDate,
                                          CurrentLeave = lb.BalanceDays == null ? 0 : (long)lb.BalanceDays,
                                          LeaveBalanceId = lb.LeaveBalanceId,
                                          TotalDays = lb.TotalDays == null ? 0 : lb.TotalDays
                                      }
                                      ).ToList();

                    foreach (var profile in empProfile)
                    {
                        if (profile.DOJ != null && profile.ConfirmationDate != null)
                        {
                            profile.LeaveCredit = GetELCredit(lastCreditRun, toDate, Convert.ToDateTime(profile.ConfirmationDate));
                            profile.NewLeaveBalance = profile.CurrentLeave + profile.LeaveCredit;
                        }
                        empProfileModel.Add(profile);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return empProfileModel;
        }

        public IList<LeaveCreditModel> GetEmployeeProfilesforCLSL(long leaveTypeId)
        {
            IList<LeaveCreditModel> leaveCreditModel = new List<LeaveCreditModel>();
            DateTime toDate = DateTime.Now.AddMonths(-1);

            try
            {
                Int32 year = System.DateTime.Now.Year;

                using (var context = new NLTDDbContext())
                {
                    leaveCreditModel = (from e in context.Employee
                                        join elb in context.EmployeeLeaveBalance on
                                        new { p1 = e.UserId, p2 = System.DateTime.Now.Year, p3 = leaveTypeId }
                                        equals
                                        new { p1 = elb.UserId, p2 = elb.Year, p3 = elb.LeaveTypeId }
                                        into leaveBal
                                        from lb in leaveBal.DefaultIfEmpty()
                                        where e.IsActive == true && e.ConfirmationDate == null && e.EmploymentTypeId == 1
                                        orderby e.FirstName
                                        select new LeaveCreditModel
                                        {
                                            UserId = e.UserId,
                                            EmployeeId = e.EmployeeId,
                                            Name = e.FirstName + " " + e.LastName,
                                            DOJ = e.DOJ,
                                            ConfirmationDate = e.ConfirmationDate,
                                            CurrentLeave = lb.BalanceDays == null ? 0 : (long)lb.BalanceDays,
                                            LeaveBalanceId = lb.LeaveBalanceId,
                                            TotalDays = lb.TotalDays == null ? 0 : lb.TotalDays
                                        }
                                      ).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return leaveCreditModel;
        }

        public static int GetELCredit(DateTime fromDate, DateTime toDate, DateTime confirmationDate)
        {
            int elCredit = 0;
            if (GetMonthDifference(toDate, fromDate) >= 1)
            {
                if (fromDate > confirmationDate)
                {
                    elCredit = GetMonthDifference(toDate, fromDate);
                }
                else if (toDate > confirmationDate)
                {
                    if (Convert.ToInt64(confirmationDate.Day) > 15)
                        elCredit = GetMonthDifference(toDate, confirmationDate) - 1;
                    else
                        elCredit = GetMonthDifference(toDate, confirmationDate);
                }
            }
            return elCredit;
        }

        public static int GetMonthDifference(DateTime toDate, DateTime fromDate)
        {
            int monthsApart = (toDate.Year * 12 + toDate.Month) - (fromDate.Year * 12 + fromDate.Month) + 1;
            return monthsApart;
        }

        public IList<ViewEmployeeProfileModel> GetTeamProfiles(Int64 userId, bool onlyReportedToMe, Int64? paramUserId, string requestMenuUser, bool hideInactiveEmp)
        {
            IList<ViewEmployeeProfileModel> retModel = new List<ViewEmployeeProfileModel>();
            IList<Int64> empList = GetEmployeesReporting(userId);
            ViewEmployeeProfileModel profile = null;
            try
            {
                using (var context = new NLTDDbContext())
                {
                    var ids = (from e in context.Employee
                               join s in context.ShiftMaster on e.ShiftId equals s.ShiftID
                               orderby e.FirstName
                               select new { userId = e.UserId, iactive = e.IsActive, reportingToId = e.ReportingToId, name = e.FirstName + " " + e.LastName, Shift = s.FromTime + "-" + s.ToTime }
                             ).ToList();

                    if (requestMenuUser == "My")
                    {
                        ids = ids.Where(x => x.userId == userId).ToList();
                    }
                    else
                    {
                        string userRole = GetEmployeeRole(userId);
                        if (userRole != "")
                        {
                            if (requestMenuUser == "Admin")
                            {
                                if (userRole == "ADMIN" || userRole == "HR")
                                {
                                    if (hideInactiveEmp == true)
                                    {
                                        ids = ids.Where(x => x.iactive == true).ToList();
                                    }
                                    else
                                    {
                                        ids = ids.ToList();
                                    }
                                }
                            }
                            else if (requestMenuUser == "Team")
                            {
                                if (userRole == "ADMIN" || userRole == "HR")
                                {
                                    if (paramUserId > 0)
                                    {
                                        ids = ids.Where(x => x.userId == paramUserId).ToList();
                                    }
                                    else
                                    {
                                        if (onlyReportedToMe)
                                        {
                                            ids = ids.Where(t => t.reportingToId == userId && t.iactive == true).ToList();
                                        }
                                        else
                                        {
                                            ids = ids.ToList();
                                        }

                                        if (hideInactiveEmp == true)
                                        {
                                            if (ids.Count > 0)
                                                ids = ids.Where(x => x.iactive == true).ToList();
                                        }
                                    }
                                }
                                else
                                {
                                    if (paramUserId > 0)
                                    {
                                        ids = ids.Where(t => empList.Contains(t.userId) && t.iactive == true).ToList();
                                        if (ids.Count > 0)
                                            ids = ids.Where(x => x.userId == paramUserId).ToList();
                                    }
                                    else
                                    {
                                        if (onlyReportedToMe)
                                        {
                                            ids = ids.Where(t => t.reportingToId == userId && t.iactive == true).ToList();
                                        }
                                        else
                                        {
                                            ids = ids.Where(t => empList.Contains(t.userId) && t.iactive == true).ToList();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (var memId in ids)
                    {
                        profile = (from employee in context.Employee.AsEnumerable()
                                   join rt in context.EmployeeRole on employee.EmployeeRoleId equals rt.RoleId
                                   join et in context.EmploymentType on employee.EmploymentTypeId equals et.EmploymentTypeId
                                   join o in context.OfficeLocation on employee.OfficeId equals o.OfficeId
                                   join h in context.OfficeHoliday on employee.OfficeHolidayId equals h.OfficeHolidayId
                                   join s in context.ShiftMaster on employee.ShiftId equals s.ShiftID
                                   where employee.UserId == memId.userId
                                   select new ViewEmployeeProfileModel
                                   {
                                       EmailAddress = employee.EmailAddress,
                                       EmployeeId = employee.EmployeeId,
                                       FirstName = employee.FirstName,
                                       LastName = employee.LastName,
                                       Name = employee.FirstName + " " + employee.LastName,
                                       Gender = employee.Gender == "M" ? "Male" : "Female",
                                       HolidayOfficeId = employee.OfficeHolidayId,
                                       OfficeName = o.OfficeName,
                                       EmploymentTypeCode = et.Code,
                                       MobileNumber = employee.MobileNumber,
                                       ReportedToId = employee.ReportingToId,
                                       RoleText = rt.Role,
                                       UserId = employee.UserId,
                                       LogonId = employee.LoginId,
                                       IsActive = employee.IsActive,
                                       CardId = employee.Cardid,
                                       DOJ = String.Format("{0:dd-MM-yyyy}", employee.DOJ),
                                       RelievingDate = String.Format("{0:dd-MM-yyyy}", employee.RelievingDate),
                                       ConfirmationDate = String.Format("{0:dd-MM-yyyy}", employee.ConfirmationDate),
                                       Shift = string.Format("{0:hh\\:mm}", s.FromTime) + " - " + string.Format("{0:hh\\:mm}", s.ToTime)
                                   }).FirstOrDefault();
                        if (profile != null)
                        {
                            var repName = context.Employee.Where(x => x.UserId == profile.ReportedToId).FirstOrDefault();
                            if (repName != null)
                            {
                                profile.ReportedToName = repName.FirstName + " " + repName.LastName;
                            }
                            profile.HolidayOfficeName = context.OfficeLocation.Where(x => x.OfficeId == profile.HolidayOfficeId).FirstOrDefault().OfficeName;
                            var weekOffs = (from e in context.Employee
                                            join wo in context.EmployeeWeekOff on e.UserId equals wo.UserId
                                            join w in context.DayOfWeek on wo.DaysOfWeekId equals w.DaysOfWeekId
                                            where e.UserId == profile.UserId
                                            select new { w.Day }
                                          ).ToList();

                            if (weekOffs.Count > 0)
                            {
                                foreach (var item in weekOffs)
                                {
                                    switch (item.Day.ToUpper())
                                    {
                                        case "SUNDAY":
                                            profile.Sunday = true;
                                            break;

                                        case "MONDAY":
                                            profile.Monday = true;
                                            break;

                                        case "TUESDAY":
                                            profile.Tuesday = true;
                                            break;

                                        case "WEDNESDAY":
                                            profile.Wednesday = true;
                                            break;

                                        case "THURSDAY":
                                            profile.Thursday = true;
                                            break;

                                        case "FRIDAY":
                                            profile.Friday = true;
                                            break;

                                        case "SATURDAY":
                                            profile.Saturday = true;
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }
                        }

                        retModel.Add(profile);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return retModel;
        }

        public ViewEmployeeProfileModel ViewEmployeeProfile(Int64 userId)
        {
            ViewEmployeeProfileModel profile = null;
            try
            {
                using (var context = new NLTDDbContext())
                {
                    profile = (from employee in context.Employee
                               join rt in context.EmployeeRole on employee.EmployeeRoleId equals rt.RoleId
                               join o in context.OfficeLocation on employee.OfficeId equals o.OfficeId
                               join h in context.OfficeHoliday on employee.OfficeHolidayId equals h.OfficeHolidayId
                               where employee.UserId == userId
                               select new ViewEmployeeProfileModel
                               {
                                   EmailAddress = employee.EmailAddress,
                                   EmployeeId = employee.EmployeeId,
                                   FirstName = employee.FirstName,
                                   LastName = employee.LastName,
                                   Gender = employee.Gender == "M" ? "Male" : "Female",
                                   HolidayOfficeId = employee.OfficeHolidayId,
                                   OfficeName = o.OfficeName,
                                   MobileNumber = employee.MobileNumber,
                                   ReportedToId = employee.ReportingToId,
                                   RoleText = rt.Role,
                                   UserId = employee.UserId,
                                   LogonId = employee.LoginId,
                                   IsActive = employee.IsActive
                               }).FirstOrDefault();
                    if (profile != null)
                    {
                        var repName = context.Employee.Where(x => x.UserId == profile.ReportedToId).FirstOrDefault();
                        if (repName != null)
                        {
                            profile.ReportedToName = repName.FirstName + " " + repName.LastName;
                        }
                        profile.HolidayOfficeName = context.OfficeLocation.Where(x => x.OfficeId == profile.HolidayOfficeId).FirstOrDefault().OfficeName;
                        var weekOffs = (from e in context.Employee
                                        join wo in context.EmployeeWeekOff on e.UserId equals wo.UserId
                                        join w in context.DayOfWeek on wo.DaysOfWeekId equals w.DaysOfWeekId
                                        where e.UserId == profile.UserId
                                        select new { w.Day }
                                      ).ToList();

                        if (weekOffs.Count > 0)
                        {
                            foreach (var item in weekOffs)
                            {
                                switch (item.Day.ToUpper())
                                {
                                    case "SUNDAY":
                                        profile.Sunday = true;
                                        break;

                                    case "MONDAY":
                                        profile.Monday = true;
                                        break;

                                    case "TUESDAY":
                                        profile.Tuesday = true;
                                        break;

                                    case "WEDNESDAY":
                                        profile.Wednesday = true;
                                        break;

                                    case "THURSDAY":
                                        profile.Thursday = true;
                                        break;

                                    case "FRIDAY":
                                        profile.Friday = true;
                                        break;

                                    case "SATURDAY":
                                        profile.Saturday = true;
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return profile;
        }

        public EmployeeProfile GetEmployeeLoginProfile(string LogonId)
        {
            EmployeeProfile profile = null;
            try
            {
                using (var context = new NLTDDbContext())
                {
                    profile = (from employee in context.Employee
                               join rt in context.EmployeeRole on employee.EmployeeRoleId equals rt.RoleId
                               where employee.LoginId == LogonId && employee.IsActive == true
                               select new EmployeeProfile
                               {
                                   EmailAddress = employee.EmailAddress,
                                   EmployeeId = employee.EmployeeId,
                                   FirstName = employee.FirstName,
                                   LastName = employee.LastName,
                                   OfficeId = employee.OfficeId,
                                   ReportedToId = employee.ReportingToId,
                                   RoleId = employee.EmployeeRoleId,
                                   RoleText = rt.Role,
                                   UserId = employee.UserId,
                                   LogonId = employee.LoginId,
                                   IsActive = employee.IsActive
                               }).FirstOrDefault();
                    if (profile != null)
                    {
                        if (profile.ReportedToId != null)
                        {
                            var repName = context.Employee.Where(x => x.UserId == profile.ReportedToId).FirstOrDefault();
                            if (repName != null)
                            {
                                profile.ReportedToName = repName.FirstName + " " + repName.LastName;
                            }
                        }
                        var teamReporting = context.Employee.Where(x => x.ReportingToId == profile.UserId && x.IsActive == true).FirstOrDefault();
                        if (teamReporting != null)
                        {
                            profile.IsHandleMembers = true;
                        }
                        else
                        {
                            profile.IsHandleMembers = false;
                        }
                    }
                }
            }
            catch { throw; }
            return profile;
        }

        public List<DropDownItem> GetReportToList(Int64 OfficeId)
        {
            using (var context = new NLTDDbContext())
            {
                List<DropDownItem> ReportToPersons = (from employee in context.Employee
                                                      join lead in context.Employee on employee.ReportingToId equals lead.UserId
                                                      where employee.OfficeId == OfficeId
                                                      select new DropDownItem
                                                      {
                                                          Key = lead.UserId.ToString(),
                                                          Value = lead.FirstName + " " + lead.LastName
                                                      }).ToList();

                ReportToPersons = ReportToPersons.GroupBy(x => x.Key)
                   .Select(grp => grp.First())
                   .ToList();

                return ReportToPersons;
            }
        }

        public List<DropDownItem> GetActiveEmpList(Int64 OfficeId, Int64? exceptUserId)
        {
            List<DropDownItem> AllEmp = new List<DropDownItem>();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    if (exceptUserId == null || exceptUserId == 0)
                    {
                        var emp = (from employee in context.Employee
                                   where employee.OfficeId == OfficeId && employee.IsActive == true
                                   select new DropDownItem
                                   {
                                       Key = employee.UserId.ToString(),
                                       Value = employee.FirstName + " " + employee.LastName
                                   }).ToList();

                        AllEmp = emp.OrderBy(x => x.Value).ToList();
                    }
                    else
                    {
                        var emp = (from employee in context.Employee
                                   where employee.OfficeId == OfficeId && employee.IsActive == true && employee.UserId != exceptUserId
                                   select new DropDownItem
                                   {
                                       Key = employee.UserId.ToString(),
                                       Value = employee.FirstName + " " + employee.LastName
                                   }).ToList();

                        AllEmp = emp.OrderBy(x => x.Value).ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return AllEmp;
        }

        public static int AddOrUpdateEmployeeDefaultShift(string mode, Int64 eUserId, int? newShiftId, Int64 currentUserId, int? oldShiftId)
        {
            using (var context = new NLTDDbContext())
            {
                var isSaved = 0;
                try
                {
                    if (mode == "Add")
                    {
                        DateTime fromdate = new DateTime(DateTime.Now.Year, 1, 1);
                        DateTime todate = new DateTime(DateTime.Now.Year, 12, 31);
                        var listofDays = Enumerable.Range(0, (todate - fromdate).Days + 1).Select(d => fromdate.AddDays(d));

                        var shiftMapping = from date in listofDays
                                           select new ShiftMapping
                                           {
                                               UserID = eUserId,
                                               ShiftID = newShiftId,
                                               ShiftDate = date,
                                               Createddate = DateTime.Now,
                                               CreatedBy = currentUserId,
                                               ModifiedDate = DateTime.Now,
                                               ModifiedBy = currentUserId,
                                           };
                        context.ShiftMapping.AddRange(shiftMapping);
                        isSaved = context.SaveChanges();
                    }
                    else
                    {
                        var shiftMapping = context.ShiftMapping.Where(c => c.UserID == eUserId && c.ShiftDate > DateTime.Now && c.ShiftID == oldShiftId).ToList();
                        shiftMapping.ForEach(u =>
                        {
                            u.ShiftID = newShiftId;
                            u.ModifiedBy = currentUserId;
                            u.ModifiedDate = DateTime.Now;
                        });
                        isSaved = context.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                return isSaved;
            }
        }

        public void Dispose()
        {
            //Nothing to dispose...
        }

        public string UpdateEmployeeProfile(EmployeeProfile profile, Int64 ModifiedBy)
        {
            //check if ModifiedBy user is HR

            Employee employee = null;
            int isSaved = 0;
            string retMsg = string.Empty;
            bool noChanges = false;
            string remarks = string.Empty;
            try
            {
                EmployeeDac employeeDac = new EmployeeDac();
                string userRole = employeeDac.GetEmployeeRole(ModifiedBy);
                if (userRole == "HR")
                {
                    using (var context = new NLTDDbContext())
                    {
                        employee = context.Employee.Where(e => e.EmployeeId.ToUpper() == profile.EmployeeId).FirstOrDefault();
                        if (profile.Mode == "Add")
                        {
                            if (employee != null)
                            {
                                return "Duplicate";
                            }
                        }

                        if (profile.Mode == "Add")
                        {
                            var isCorpIdExits = context.Employee.Where(e => e.LoginId == profile.LogonId.ToUpper()).FirstOrDefault();
                            if (isCorpIdExits != null)
                            {
                                if (isCorpIdExits.LoginId.Trim().Length > 5)
                                {
                                    return "DupCorp";
                                }
                            }
                            var isCardIdExits = context.Employee.Where(e => e.Cardid == profile.CardId && e.Cardid != null && e.IsActive).FirstOrDefault();
                            if (isCardIdExits != null)
                            {
                                return "DupCard";
                            }
                        }
                        else
                        {
                            var isCorpIdExits = context.Employee.Where(e => e.LoginId == profile.LogonId.ToUpper() && e.EmployeeId != employee.EmployeeId).FirstOrDefault();
                            if (isCorpIdExits != null)
                            {
                                if (isCorpIdExits.LoginId.Trim().Length > 5)
                                {
                                    return "DupCorp";
                                }
                            }
                            var isCardIdExits = context.Employee.Where(e => e.Cardid == profile.CardId && e.Cardid != null && e.EmployeeId != employee.EmployeeId && e.IsActive).FirstOrDefault();
                            if (isCardIdExits != null)
                            {
                                return "DupCard";
                            }
                        }

                        if (employee == null)
                        {
                            remarks = "#LogonId" + "^" + profile.LogonId;
                            remarks = remarks + "#EmployeeId" + "^" + profile.EmployeeId;
                            remarks = remarks + "#OfficeId" + "^" + profile.OfficeId;
                            remarks = remarks + "#IsActive" + "^" + profile.IsActive;
                            remarks = remarks + "#FirstName" + "^" + profile.FirstName;
                            remarks = remarks + "#LastName" + "^" + profile.LastName;
                            remarks = remarks + "#Gender" + "^" + profile.Gender;
                            remarks = remarks + "#EmailAddress" + "^" + profile.EmailAddress;
                            remarks = remarks + "#MobileNumber" + "^" + profile.MobileNumber;
                            remarks = remarks + "#RoleId" + "^" + profile.RoleId;
                            remarks = remarks + "#ReportedToId" + "^" + profile.ReportedToId;
                            remarks = remarks + "#OfficeHolidayId" + "^" + profile.OfficeHolidayId;
                            remarks = remarks + "#ShiftId" + "^" + 1;
                            remarks = remarks + "#CardId" + "^" + profile.CardId;
                            remarks = remarks + "#DOJ" + "^" + profile.DOJ;
                            remarks = remarks + "#ConfirmationDate" + "^" + profile.ConfirmationDate;
                            remarks = remarks + "#RelievingDate" + "^" + profile.RelievingDate;

                            employee = new Employee
                            {
                                LoginId = profile.LogonId.Trim().ToUpper(),
                                EmployeeId = profile.EmployeeId,
                                OfficeId = profile.OfficeId,
                                EmploymentTypeId = profile.EmploymentTypeId,
                                IsActive = profile.IsActive,
                                FirstName = profile.FirstName,
                                LastName = profile.LastName,
                                Gender = profile.Gender,
                                MobileNumber = profile.MobileNumber,
                                EmailAddress = profile.EmailAddress,
                                EmployeeRoleId = profile.RoleId,
                                ReportingToId = profile.ReportedToId,
                                OfficeHolidayId = profile.OfficeHolidayId,
                                ShiftId = 1,//Hard coded as default shift
                                Cardid = profile.CardId,
                                DOJ = profile.DOJ,
                                ConfirmationDate = profile.ConfirmationDate,
                                RelievingDate = profile.RelievingDate,
                                OnlyDirectAlerts = false,//When new employee added, setting this default flag
                                ModifiedBy = -1,
                                CreatedBy = ModifiedBy,
                                CreatedOn = System.DateTime.Now,
                                ModifiedOn = System.DateTime.Now
                            };

                            context.Employee.Add(employee);
                            isSaved = context.SaveChanges();
                            if (isSaved > 0)
                            {
                                isSaved = AddOrUpdateEmployeeDefaultShift(profile.Mode, employee.UserId, 1, ModifiedBy,
                                    1);//Hard coded employee shift
                            }

                            if (isSaved > 0)
                            {
                                List<String> lstNewWeekOffs = new List<string>();
                                if (profile.Sunday)
                                    lstNewWeekOffs.Add("Sunday");
                                if (profile.Monday)
                                    lstNewWeekOffs.Add("Monday");
                                if (profile.Tuesday)
                                    lstNewWeekOffs.Add("Tuesday");
                                if (profile.Wednesday)
                                    lstNewWeekOffs.Add("Wednesday");
                                if (profile.Thursday)
                                    lstNewWeekOffs.Add("Thursday");
                                if (profile.Friday)
                                    lstNewWeekOffs.Add("Friday");
                                if (profile.Saturday)
                                    lstNewWeekOffs.Add("Saturday");
                                Int64 dayOfWeek = 0;
                                EmployeeWeekOff ew;
                                foreach (var item in lstNewWeekOffs)
                                {
                                    dayOfWeek = context.DayOfWeek.Where(x => x.Day.ToUpper() == item.ToUpper()).FirstOrDefault().DaysOfWeekId;
                                    ew = new EmployeeWeekOff
                                    {
                                        UserId = employee.UserId,
                                        DaysOfWeekId = dayOfWeek,
                                        ModifiedBy = -1,
                                        ModifiedOn = System.DateTime.Now,
                                        CreatedBy = ModifiedBy,
                                        CreatedOn = System.DateTime.Now
                                    };
                                    context.EmployeeWeekOff.Add(ew);
                                    isSaved = context.SaveChanges();
                                }
                            }
                            if (isSaved > 0)
                            {
                                EmployeeTransactionHistory hist = new EmployeeTransactionHistory
                                {
                                    UserId = employee.UserId,
                                    TransactionDate = System.DateTime.Now,
                                    TransactionType = "Insert",
                                    TransactionBy = ModifiedBy,
                                    Remarks = remarks
                                };
                                context.EmployeeTransactionHistory.Add(hist);
                                isSaved = context.SaveChanges();
                            }
                            if (isSaved > 0)
                                retMsg = "Saved";
                            else
                            {
                                if (retMsg == "")
                                    retMsg = "Failed";
                            }
                            return retMsg;
                        }
                        else
                        {
                            Employee oldEmpData = new Employee();
                            oldEmpData = context.Employee.Where(x => x.UserId == profile.UserId).FirstOrDefault();

                            if (profile.LogonId != oldEmpData.LoginId)
                                remarks = "#LogonId" + "^" + profile.LogonId;

                            if (profile.EmployeeId != oldEmpData.EmployeeId)
                                remarks = remarks + "#EmployeeId" + "^" + profile.EmployeeId;

                            if (profile.IsActive != oldEmpData.IsActive)
                                remarks = remarks + "#IsActive" + "^" + profile.IsActive;

                            if (profile.ReportedToId != oldEmpData.ReportingToId)
                                remarks = remarks + "#ReportedToId" + "^" + profile.ReportedToId;

                            if (profile.FirstName != oldEmpData.FirstName)
                                remarks = remarks + "#FirstName" + "^" + profile.FirstName;

                            if (profile.LastName != oldEmpData.LastName)
                                remarks = remarks + "#LastName" + "^" + profile.LastName;

                            if (profile.Gender != oldEmpData.Gender)
                                remarks = remarks + "#Gender" + "^" + profile.Gender;

                            if (profile.MobileNumber != oldEmpData.MobileNumber)
                                remarks = remarks + "#MobileNumber" + "^" + profile.MobileNumber;

                            if (profile.EmailAddress != oldEmpData.EmailAddress)
                                remarks = remarks + "#EmailAddress" + "^" + profile.EmailAddress;

                            if (profile.OfficeHolidayId != oldEmpData.OfficeHolidayId)
                                remarks = remarks + "#OfficeHolidayId" + "^" + profile.OfficeHolidayId;

                            if (profile.RoleId != oldEmpData.EmployeeRoleId)
                                remarks = remarks + "#RoleId" + "^" + profile.RoleId;

                            if (profile.CardId != oldEmpData.Cardid)
                                remarks = remarks + "#CardId" + "^" + profile.CardId;

                            if (profile.DOJ != oldEmpData.DOJ)
                                remarks = remarks + "#DOJ" + "^" + profile.DOJ;

                            if (profile.ConfirmationDate != oldEmpData.ConfirmationDate)
                                remarks = remarks + "#ConfirmationDate" + "^" + profile.ConfirmationDate;

                            if (profile.RelievingDate != oldEmpData.RelievingDate)
                                remarks = remarks + "#RelievingDate" + "^" + profile.RelievingDate;

                            int? oldShiftid = oldEmpData.ShiftId;
                            employee.OfficeId = profile.OfficeId;
                            employee.LoginId = profile.LogonId.Trim().ToUpper();
                            employee.EmployeeId = profile.EmployeeId;
                            employee.IsActive = profile.IsActive;
                            employee.ReportingToId = profile.ReportedToId;
                            employee.FirstName = profile.FirstName;
                            employee.LastName = profile.LastName;
                            employee.Gender = profile.Gender;
                            employee.MobileNumber = profile.MobileNumber;
                            employee.EmailAddress = profile.EmailAddress;
                            employee.Cardid = profile.CardId;
                            employee.OfficeHolidayId = profile.OfficeHolidayId;
                            employee.EmployeeRoleId = profile.RoleId;
                            employee.DOJ = profile.DOJ;
                            employee.ConfirmationDate = profile.ConfirmationDate;
                            employee.RelievingDate = profile.RelievingDate;

                            if (remarks == "") { noChanges = true; }
                            else
                            {
                                noChanges = false;
                                employee.ModifiedOn = System.DateTime.Now;
                                employee.ModifiedBy = ModifiedBy;
                                isSaved = context.SaveChanges();
                            }

                            string isSameWeekoff = "";

                            var existingWeekOffs = (from wo in context.EmployeeWeekOff
                                                    join w in context.DayOfWeek on wo.DaysOfWeekId equals w.DaysOfWeekId
                                                    where wo.UserId == employee.UserId
                                                    select new { w.Day, EmpWeekOffId = wo.EmployeeWeekOffId }
                                                  ).ToList();

                            List<String> lstNewWeekOffs = new List<string>();
                            if (profile.Sunday)
                                lstNewWeekOffs.Add("Sunday");
                            if (profile.Monday)
                                lstNewWeekOffs.Add("Monday");
                            if (profile.Tuesday)
                                lstNewWeekOffs.Add("Tuesday");
                            if (profile.Wednesday)
                                lstNewWeekOffs.Add("Wednesday");
                            if (profile.Thursday)
                                lstNewWeekOffs.Add("Thursday");
                            if (profile.Friday)
                                lstNewWeekOffs.Add("Friday");
                            if (profile.Saturday)
                                lstNewWeekOffs.Add("Saturday");
                            Int64 dayOfWeek = 0;
                            Dictionary<Int64, string> dicttUpdateExisting = new Dictionary<long, string>();

                            bool deleteAndAdd = false;
                            var prepareList = lstNewWeekOffs;
                            if (existingWeekOffs.Count == lstNewWeekOffs.Count)
                            {
                                if (existingWeekOffs.Count == 0 && lstNewWeekOffs.Count == 0) { }
                                else
                                {
                                    //verify if they are same
                                    foreach (var item in existingWeekOffs)
                                    {
                                        if (lstNewWeekOffs.Where(x => x == item.Day).Any())
                                        {
                                            if (isSameWeekoff == "")
                                            {
                                                isSameWeekoff = "Yes";
                                            }
                                        }
                                        else
                                        {
                                            isSameWeekoff = "No";
                                            dicttUpdateExisting.Add(item.EmpWeekOffId, prepareList.Where(x => x != item.Day).FirstOrDefault());
                                            prepareList.Remove(dicttUpdateExisting[item.EmpWeekOffId]);
                                        }
                                    }
                                }
                            }
                            else if (lstNewWeekOffs.Count > existingWeekOffs.Count)
                            {
                                deleteAndAdd = true;
                            }
                            else if (lstNewWeekOffs.Count < existingWeekOffs.Count)
                            {
                                deleteAndAdd = true;
                            }
                            if (isSameWeekoff != "Yes")
                            {
                                noChanges = false;
                                remarks = remarks + "#WeeklyOff" + "^" + "Changed";
                                if (deleteAndAdd)
                                {
                                    var deleteOldOffs = context.EmployeeWeekOff.Where(x => x.UserId == employee.UserId).ToList();
                                    if (deleteOldOffs.Any())
                                    {
                                        context.EmployeeWeekOff.RemoveRange(deleteOldOffs);
                                        context.SaveChanges();
                                    }
                                    EmployeeWeekOff ew;
                                    foreach (var item in lstNewWeekOffs)
                                    {
                                        dayOfWeek = context.DayOfWeek.Where(x => x.Day.ToUpper() == item.ToUpper()).FirstOrDefault().DaysOfWeekId;
                                        ew = new EmployeeWeekOff
                                        {
                                            UserId = employee.UserId,
                                            DaysOfWeekId = dayOfWeek,
                                            ModifiedBy = -1,
                                            ModifiedOn = System.DateTime.Now,
                                            CreatedBy = ModifiedBy,
                                            CreatedOn = System.DateTime.Now
                                        };
                                        context.EmployeeWeekOff.Add(ew);
                                        isSaved = context.SaveChanges();
                                    }
                                }
                                else
                                {
                                    foreach (var item in dicttUpdateExisting)
                                    {
                                        var updtExist = context.EmployeeWeekOff.Where(x => x.EmployeeWeekOffId == item.Key).FirstOrDefault();
                                        dayOfWeek = context.DayOfWeek.Where(x => x.Day.ToUpper() == item.Value.ToUpper()).FirstOrDefault().DaysOfWeekId;
                                        if (updtExist != null)
                                        {
                                            updtExist.DaysOfWeekId = dayOfWeek;
                                            updtExist.ModifiedBy = ModifiedBy;
                                            updtExist.ModifiedOn = System.DateTime.Now;
                                            isSaved = context.SaveChanges();
                                        }
                                    }
                                }
                            }

                            if (isSaved > 0)
                            {
                                EmployeeTransactionHistory hist = new EmployeeTransactionHistory
                                {
                                    UserId = profile.UserId,
                                    TransactionDate = System.DateTime.Now,
                                    TransactionType = "Update",
                                    TransactionBy = ModifiedBy,
                                    Remarks = remarks
                                };
                                context.EmployeeTransactionHistory.Add(hist);
                                isSaved = context.SaveChanges();
                            }
                            if (isSaved > 0)
                                retMsg = "Saved";
                            else if (noChanges)
                            {
                                retMsg = "noChanges";
                            }
                            else
                            {
                                if (retMsg == "")
                                    retMsg = "Failed";
                            }

                            return retMsg;
                        }
                    }
                }
                else
                {
                    return "NeedRole";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public long GetEmployeeId(string LogonId)
        {
            using (var context = new NLTDDbContext())
            {
                try
                {
                    var user = context.Employee.Where(e => e.LoginId.ToUpper() == LogonId.ToUpper()).FirstOrDefault();
                    if (user == null)
                        return 0;
                    else
                        return user.UserId;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public long GetUserId(string name)
        {
            using (var context = new NLTDDbContext())
            {
                try
                {
                    var empPrf = context.Employee.Where(x => string.Concat(x.FirstName, " ", x.LastName).ToUpper() == name.ToUpper()).FirstOrDefault();
                    if (empPrf == null)
                        return 0;
                    else
                        return empPrf.UserId;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public string GetNewEmpId(Int64 OfficeId)
        {
            Int32 newEmpId = 0;
            using (var context = new NLTDDbContext())
            {
                try
                {
                    var empPrf = context.Employee.Where(x => x.OfficeId == OfficeId).OrderByDescending(x => x.EmployeeId).Select(x => x.EmployeeId).ToList();

                    if (empPrf.Count() != 0)
                        newEmpId = empPrf.Select(int.Parse).ToList().Max();

                    if (empPrf == null)
                        return "0";
                    else
                    {
                        newEmpId = newEmpId + 1;
                        return Convert.ToString(newEmpId);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public string GetNewEmpId(Int64 OfficeId, Int64 employmentTypeId)
        {
            Int32 newEmpId = 0;

            try
            {
                using (var context = new NLTDDbContext())
                {
                    var employeeIdPrefix = context.EmploymentType.Where(x => x.EmploymentTypeId == employmentTypeId).FirstOrDefault().EmployeeIdPrefix;
                    var empPrf = context.Employee.Where(x => x.OfficeId == OfficeId && x.EmploymentTypeId == employmentTypeId).Select(x => x.EmployeeId.Replace(employeeIdPrefix, "")).ToList();

                    if (empPrf.Count() != 0)
                        newEmpId = empPrf.Select(int.Parse).ToList().Max();

                    if (empPrf == null)
                        return employeeIdPrefix + "001";
                    else
                    {
                        newEmpId = newEmpId + 1;
                        return employeeIdPrefix + newEmpId.ToString("000");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string ReportingToName(Int64 userId)
        {
            using (var context = new NLTDDbContext())
            {
                try
                {
                    var user = (from e in context.Employee
                                join r in context.Employee on e.ReportingToId equals r.UserId
                                where e.UserId == userId
                                select new { r.FirstName, r.LastName }
                                       ).FirstOrDefault();
                    if (user == null)
                    {
                        return "";
                    }
                    else
                    {
                        return user.FirstName + " " + user.LastName;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public IList<Int64> GetDirectEmployees(Int64 userID)
        {
            IList<Int64> userIDList = new List<Int64>();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    userIDList = (from e in context.Employee where e.ReportingToId == userID && e.IsActive select e.UserId).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return userIDList;
        }

        public IList<DropDownItem> GetEmploymentTypes()
        {
            IList<DropDownItem> employmentTypeList = new List<DropDownItem>();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    employmentTypeList = (from et in context.EmploymentType
                                          orderby et.EmploymentTypeId
                                          select new DropDownItem
                                          {
                                              Key = et.EmploymentTypeId.ToString(),
                                              Value = et.Code
                                          }
                                          ).ToList();
                }
            }
            catch
            {
                throw;
            }
            return employmentTypeList;
        }
    }
}