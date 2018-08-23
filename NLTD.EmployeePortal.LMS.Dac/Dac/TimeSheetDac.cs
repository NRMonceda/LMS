using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Dac.DbModel;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public class TimeSheetDac : ITimesheetHelper
    {
        private int BeforeShiftBuffer = Convert.ToInt32(ConfigurationManager.AppSettings["BeforeShiftBuffer"]);
        private int AfterShiftBuffer = Convert.ToInt32(ConfigurationManager.AppSettings["AfterShiftBuffer"]);

        public List<ShiftQueryModel> GetShiftDetails(Int64 UserID, DateTime FromDate, DateTime ToDate)
        {
            List<ShiftQueryModel> shiftQueryModelList = new List<ShiftQueryModel>();
            try
            {
                using (var entity = new NLTDDbContext())
                {
                    shiftQueryModelList = (from sm in entity.ShiftMaster
                                           join smp in entity.ShiftMapping on sm.ShiftID equals smp.ShiftID
                                           join e in entity.Employee on smp.UserID equals e.UserId
                                           where smp.UserID == UserID && smp.ShiftDate >= FromDate && smp.ShiftDate <= ToDate
                                           select new ShiftQueryModel
                                           {
                                               UserID = smp.UserID,
                                               Employeename = e.FirstName + " " + e.LastName,
                                               ShiftFromtime = sm.FromTime,
                                               ShiftTotime = sm.ToTime,
                                               ShiftDate = smp.ShiftDate,
                                           }).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return shiftQueryModelList.OrderBy(m => m.ShiftDate).ToList();
        }

        private string GetAbsentStatus(DateTime CurrentDate, List<string> officeWeekOffDayList,
            List<OfficeHoliday> officeHolidayList)
        {
            string status = string.Empty;
            bool found = false;

            // to check whether the day is week off
            try
            {
                if (officeWeekOffDayList.Count() > 0)
                {
                    int count = (from wod in officeWeekOffDayList
                                 where wod == CurrentDate.DayOfWeek.ToString()
                                 select wod).Count();
                    if (count > 0)
                    {
                        status = TimeSheetStatus.WeekOff;
                        found = true;
                    }
                }
                // To check whether it is public holiday
                if (!found)
                {
                    if (officeHolidayList.Count > 0)
                    {
                        int count = (from hl in officeHolidayList where hl.Holiday == CurrentDate select hl).Count();

                        if (count > 0)
                        {
                            status = TimeSheetStatus.Holiday;
                            found = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return status;
        }

        public List<TimeSheetModel> GetMyTimeSheet(Int64 UserID, DateTime FromDate, DateTime ToDate)
        {
            List<TimeSheetModel> timeSheetModelList = new List<TimeSheetModel>();
            List<ShiftQueryModel> ShiftQueryModelList = GetShiftDetails(UserID, FromDate, ToDate);

            string PersonalPermisionLabel = ConfigurationManager.AppSettings["PersonalPermission"].ToString();
            string officialPermisionLabel = ConfigurationManager.AppSettings["OfficialPermission"].ToString();

            var toDateShift = (from m in ShiftQueryModelList where m.ShiftDate == ToDate select new { fromTime = m.ShiftFromtime, toTime = m.ShiftTotime }).FirstOrDefault();

            if (toDateShift != null)
            {
                TimeSpan fromTime = toDateShift.fromTime;
                TimeSpan toTime = toDateShift.toTime;
                if (fromTime > toTime)
                {
                    ToDate = ToDate.AddDays(1).Add(toTime.Add(new TimeSpan(AfterShiftBuffer, 0, 0)));
                }
                else
                {
                    ToDate = ToDate.Add(toTime.Add(new TimeSpan(AfterShiftBuffer, 0, 0)));
                }
            }

            IEmployeeAttendanceHelper EmployeeAttendanceDacObj = new EmployeeAttendanceDac();

            //To Retrieve the Employee Attendance for the given date.
            List<EmployeeAttendanceModel> EmployeeAttendanceList = EmployeeAttendanceDacObj.GetAttendanceForRange(UserID, FromDate, ToDate, "My", true);

            // To Get the Employee name
            EmployeeProfile EmployeeProfileObj = new EmployeeDac().GetEmployeeProfile(UserID);
            string name = string.Empty;
            string reportingManager = string.Empty;
            if (EmployeeProfileObj != null)
            {
                name = EmployeeProfileObj.FirstName + ' ' + EmployeeProfileObj.LastName;
                reportingManager = EmployeeProfileObj.ReportedToName;
            }

            // To get the employee week off Days
            OfficeWeekOffDac officeWeekOffDacObj = new OfficeWeekOffDac();
            List<string> officeWeekOffDayList = officeWeekOffDacObj.GetEmployeeWeekOffDay(UserID);

            // To get the employee Holiday List
            OfficeHolidayDac officeHolidayDacObj = new OfficeHolidayDac();
            List<OfficeHoliday> officeHolidayList = officeHolidayDacObj.GetOfficeHoliday(UserID);

            // To get the employee Leave Details
            LeaveTransactionHistoryDac leaveTransactionHistoryDacObj = new LeaveTransactionHistoryDac();
            List<EmployeeLeave> employeeLeaveList = leaveTransactionHistoryDacObj.GetLeaveForEmployee(UserID, FromDate, ToDate);

            for (int i = 0; i < ShiftQueryModelList.Count(); i++)
            {
                decimal permissionCountOfficial = 0, permissionCountPersonal = 0, LeaveDayQty = 0, WorkFromHomeDayQty = 0; 
                TimeSheetModel TimeSheetModelObj = new TimeSheetModel();
                DateTime shiftFromDateTime = ShiftQueryModelList[i].ShiftDate.Add(ShiftQueryModelList[i].ShiftFromtime.Add(new TimeSpan(-BeforeShiftBuffer, 0, 0)));
                DateTime shiftEndDateTime = ShiftQueryModelList[i].ShiftDate.Add(ShiftQueryModelList[i].ShiftTotime.Add(new TimeSpan(AfterShiftBuffer, 0, 0)));

                if (shiftEndDateTime < shiftFromDateTime)
                {
                    shiftEndDateTime = shiftEndDateTime.AddDays(1);
                }
                // To add the employee basic details
                TimeSheetModelObj.Shift = ShiftQueryModelList[i].ShiftFromtime.ToString(@"hh\:mm") + '-' + ShiftQueryModelList[i].ShiftTotime.ToString(@"hh\:mm");
                TimeSheetModelObj.userID = UserID;
                TimeSheetModelObj.Name = name;
                TimeSheetModelObj.ReportingManager = reportingManager;
                TimeSheetModelObj.WorkingDate = ShiftQueryModelList[i].ShiftDate;
                // Linq query to find the min and max for the given date
                var maxmin = from s in EmployeeAttendanceList
                             where s.InOutDate >= shiftFromDateTime && s.InOutDate <= shiftEndDateTime
                             group s by true into r
                             select new
                             {
                                 min = r.Min(z => z.InOutDate),
                                 max = r.Max(z => z.InOutDate)
                             };
                if (maxmin != null && maxmin.Count() > 0)
                {
                    TimeSheetModelObj.InTime = maxmin.ToList()[0].min;
                    TimeSheetModelObj.OutTime = maxmin.ToList()[0].max;
                    if (employeeLeaveList.Select(e => e.LeaveType == officialPermisionLabel).Count() > 0)
                    {
                        foreach (var permissionTime in employeeLeaveList)
                        {
                            if (permissionTime.StartDate == TimeSheetModelObj.WorkingDate && permissionTime.LeaveType == officialPermisionLabel)
                            {
                                permissionCountOfficial += permissionTime.PermissionCount;
                            }
                        }
                    }


                    if (employeeLeaveList.Select(e => e.LeaveType == PersonalPermisionLabel).Count() > 0)
                    {
                        foreach (var permissionTime in employeeLeaveList)
                        {
                            if (permissionTime.StartDate == TimeSheetModelObj.WorkingDate && permissionTime.LeaveType == PersonalPermisionLabel)
                            {
                                permissionCountPersonal += permissionTime.PermissionCount;
                            }

                        }
                    }

                    if (employeeLeaveList.Select(e => e.LeaveTypeId = 0).Count() > 0)
                    {
                        foreach (var permissionTime in employeeLeaveList)
                        {
                            if (permissionTime.StartDate == TimeSheetModelObj.WorkingDate && permissionTime.LeaveDayQty != 0 && permissionTime.IsLeave == true)
                            {
                                LeaveDayQty += permissionTime.LeaveDayQty;
                            }
                        }
                    }

                    if (employeeLeaveList.Select(e => e.LeaveTypeId = 0).Count() > 0)
                    {
                        foreach (var permissionTime in employeeLeaveList)
                        {
                            if (permissionTime.StartDate == TimeSheetModelObj.WorkingDate && permissionTime.WorkFromHomeDayQty != 0 && permissionTime.IsLeave == false)
                            {
                                WorkFromHomeDayQty += permissionTime.WorkFromHomeDayQty;
                            }
                        }
                    }

                    TimeSheetModelObj.WorkingHours = TimeSheetModelObj.OutTime - TimeSheetModelObj.InTime;

                    TimeSheetModelObj.Status = "Present";

                    if (TimeSheetModelObj.InTime.TimeOfDay > ShiftQueryModelList[i].ShiftFromtime)
                    {
                        TimeSheetModelObj.LateIn = TimeSheetModelObj.InTime.TimeOfDay - ShiftQueryModelList[i].ShiftFromtime;
                    }

                    DateTime shiftToTime = ShiftQueryModelList[i].ShiftDate.Add(ShiftQueryModelList[i].ShiftTotime);
                    DateTime shiftFromTime = ShiftQueryModelList[i].ShiftDate.Add(ShiftQueryModelList[i].ShiftFromtime);
                    if (shiftToTime < shiftFromTime)
                    {
                        shiftToTime = shiftToTime.AddDays(1);
                    }
                    if (shiftToTime > TimeSheetModelObj.OutTime)
                    {
                        TimeSheetModelObj.EarlyOut = ShiftQueryModelList[i].ShiftTotime - TimeSheetModelObj.OutTime.TimeOfDay;
                    }
                }
                else// If no record found in the employee for the given date
                {
                    // Get Absent Details
                    TimeSheetModelObj.Status = GetAbsentStatus(ShiftQueryModelList[i].ShiftDate, officeWeekOffDayList, officeHolidayList);
                    if (employeeLeaveList.Select(e => e.LeaveType == officialPermisionLabel).Count() > 0)
                    {
                        foreach (var permissionTime in employeeLeaveList)
                        {
                            if (permissionTime.StartDate == TimeSheetModelObj.WorkingDate && permissionTime.LeaveType == officialPermisionLabel)
                            {
                                permissionCountOfficial += permissionTime.PermissionCount;
                            }
                        }
                    }

                    if (employeeLeaveList.Select(e => e.LeaveType == PersonalPermisionLabel).Count() > 0)
                    {
                        foreach (var permissionTime in employeeLeaveList)
                        {
                            if (permissionTime.StartDate == TimeSheetModelObj.WorkingDate && permissionTime.LeaveType == PersonalPermisionLabel)
                            {
                                permissionCountPersonal += permissionTime.PermissionCount;
                            }
                        }
                    }

                    if (employeeLeaveList.Select(e => e.LeaveTypeId = 0).Count() > 0)
                    {
                        foreach (var permissionTime in employeeLeaveList)
                        {
                            if (permissionTime.StartDate == TimeSheetModelObj.WorkingDate && permissionTime.LeaveDayQty != 0 && permissionTime.IsLeave == true)
                            {
                                LeaveDayQty += permissionTime.LeaveDayQty;
                            }
                        }
                    }
                    if (employeeLeaveList.Select(e => e.LeaveTypeId = 0).Count() > 0)
                    {
                        foreach (var permissionTime in employeeLeaveList)
                        {
                            if (permissionTime.StartDate == TimeSheetModelObj.WorkingDate && permissionTime.WorkFromHomeDayQty != 0 && permissionTime.IsLeave == false)
                            {
                                WorkFromHomeDayQty += permissionTime.WorkFromHomeDayQty;
                            }
                        }
                    }
                }

                string StartDateType = "";
                string EndDateType = "";

                // To get the employee Leave Details

                TimeSheetModelObj.Requests = GetLMSStatus(employeeLeaveList, ShiftQueryModelList[i].ShiftDate);
                TimeSheetModelObj.StartDateType = GetHalfDayLMSType(employeeLeaveList, ShiftQueryModelList[i].ShiftDate, out StartDateType);
                TimeSheetModelObj.EndDateType = GetHalfDayLMSType(employeeLeaveList, ShiftQueryModelList[i].ShiftDate, out EndDateType);

                TimeSheetModelObj.LeaveDayQty = LeaveDayQty;
                TimeSheetModelObj.WorkFromHomeDayQty = WorkFromHomeDayQty;
                TimeSheetModelObj.PermissionCount = permissionCountPersonal;


                TimeSheetModelObj.permissionCountOfficial = permissionCountOfficial;
                TimeSheetModelObj.permissionCountPersonal = permissionCountPersonal;

                timeSheetModelList.Add(TimeSheetModelObj);
            }

            return timeSheetModelList.OrderByDescending(m => m.WorkingDate).ToList();
        }

        public List<TimeSheetModel> GetMyTeamTimeSheet(Int64 UserID, DateTime FromDate, DateTime ToDate, bool myDirectEmployees)
        {
            List<TimeSheetModel> timeSheetModelList = new List<TimeSheetModel>();
            // To Get all the employee profile under the manager or lead
            EmployeeDac EmployeeDacObj = new EmployeeDac();
            string userRole = string.Empty;
            // To get the employee role, whether he is the Team lead or HR Or admin
            try
            {
                using (NLTDDbContext context = new NLTDDbContext())
                {
                    userRole = (from emp in context.Employee
                                join role in context.EmployeeRole on emp.EmployeeRoleId equals role.RoleId
                                where emp.UserId == UserID
                                select role.Role).FirstOrDefault();
                }
                List<EmployeeProfile> employeeProfileListUnderManager = EmployeeDacObj.GetReportingEmployeeProfile(UserID, userRole, myDirectEmployees).OrderBy(m => m.FirstName).ToList();
                for (int i = 0; i < employeeProfileListUnderManager.Count; i++)
                {
                    List<TimeSheetModel> timeSheetModelListTemp = GetMyTimeSheet(employeeProfileListUnderManager[i].UserId, FromDate, ToDate);
                    timeSheetModelList.AddRange(timeSheetModelListTemp);
                }
                return timeSheetModelList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetLMSStatus(List<EmployeeLeave> employeeLeaveList, DateTime statusDate)
        {
            string personalPermisionLabel = ConfigurationManager.AppSettings["PersonalPermission"].ToString();
            string officialPermisionLabel = ConfigurationManager.AppSettings["OfficialPermission"].ToString();
            string overTimeLabel = ConfigurationManager.AppSettings["OverTime"].ToString();

            string LMSPermissionStatus = string.Empty;
            string LMSLeaveStatus = string.Empty;
            string getLMSStatus = string.Empty;           

            try
            {
                if (employeeLeaveList.Count > 0)
                {
                    var StatusList = (from e in employeeLeaveList where statusDate >= e.StartDate && statusDate <= e.EndDate select new { e.LeaveType, e.LeaveDayQty, e.PermissionCount, e.StartDateType, e.LeaveTypeId });
                    if (StatusList != null && StatusList.Count() > 0)
                    {
                        foreach (var Status in StatusList.OrderBy(e => e.LeaveTypeId))
                        {
                            if (Status.LeaveType == personalPermisionLabel || Status.LeaveType == officialPermisionLabel || Status.LeaveType == overTimeLabel)
                            {
                                if (string.IsNullOrEmpty(LMSPermissionStatus))
                                {
                                    if (Status.LeaveType == officialPermisionLabel)
                                    {
                                        LMSPermissionStatus = "Permission (O: " + Status.PermissionCount + "hrs)";
                                    }
                                    else if (Status.LeaveType == personalPermisionLabel)
                                    {
                                        LMSPermissionStatus = "Permission (P: " + Status.PermissionCount + "hrs)";
                                    }
                                    else
                                    {
                                        LMSPermissionStatus = "OT: " + Status.PermissionCount + "hrs";
                                    }
                                }
                                else
                                {
                                    if (Status.LeaveType == officialPermisionLabel)
                                    {
                                        LMSPermissionStatus = LMSPermissionStatus.Remove(LMSPermissionStatus.Length - 1) + ", O: " + Status.PermissionCount + "hrs)";
                                    }
                                    else if (Status.LeaveType == personalPermisionLabel)
                                    {
                                        LMSPermissionStatus = LMSPermissionStatus.Remove(LMSPermissionStatus.Length - 1) + ", P: " + Status.PermissionCount + "hrs)";
                                    }
                                    else
                                    {
                                        LMSPermissionStatus = LMSPermissionStatus + ", OT: " + Status.PermissionCount + "hrs";
                                    }

                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(LMSLeaveStatus))
                                {
                                    LMSLeaveStatus = Status.LeaveType;
                                }
                                else
                                {
                                    LMSLeaveStatus = LMSLeaveStatus + ", " + Status.LeaveType;
                                }
                                if (Status.StartDateType == "F" || Status.StartDateType == "S")
                                {
                                    if (Status.StartDateType == "F")
                                    {
                                        LMSLeaveStatus = LMSLeaveStatus + " (First Half)";
                                    }
                                    else
                                    {
                                        LMSLeaveStatus = LMSLeaveStatus + " (Second Half)";
                                    }
                                }
                            }
                        }
                    }
                }
                getLMSStatus = LMSLeaveStatus + (LMSLeaveStatus.Length > 0 && LMSPermissionStatus.Length > 0 ? ", " : "") + LMSPermissionStatus;                
            }
            catch (Exception)
            {
                throw;
            }
            return getLMSStatus;
        }

        public string GetHalfDayLMSType(List<EmployeeLeave> employeeLeaveList, DateTime statusDate, out string StartDateType)
        {
            StartDateType = string.Empty;

            try
            {
                if (employeeLeaveList.Count > 0)
                {
                    var Status = (from e in employeeLeaveList where statusDate >= e.StartDate && statusDate <= e.EndDate select new { e.StartDateType });
                    if (Status != null && Status.Count() > 0)
                    {
                        StartDateType = Status.Select(p => p.StartDateType).FirstOrDefault();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return StartDateType;
        }
    }

    public class EmployeeLeave
    {
        public Int64 LeaveId { get; set; }
        public Int64 UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public String StartDateType { get; set; }
        public String EndDateType { get; set; }
        public Int64 LeaveTypeId { get; set; }
        public Decimal Duration { get; set; }
        public String Remarks { get; set; }
        public String Comments { get; set; }
        public String Status { get; set; }
        public Int64 AppliedBy { get; set; }
        public DateTime AppliedAt { get; set; }
        public Int64? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public string LeaveType { get; set; }

        public decimal LeaveDayQty { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }

        public decimal PermissionCount { get; set; }

        public bool IsLeave { get; set; }

        public decimal WorkFromHomeDayQty { get; set; }
    }
}