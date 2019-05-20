using Hangfire;
using NLTD.EmployeePortal.LMS.Client;
using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Common.QueryModel;
using NLTD.EmployeePortal.LMS.Repository;
using NLTD.EmployeePortal.LMS.Ux.AppHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace NLTD.EmployeePortal.LMS.Ux
{
    public class TimesheetEmailReportService
    {
        public void ProcessWeeklyReport()
        {            
            DateTime monthReportDate = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 23);
           
            if(DateTime.Now.Date!= monthReportDate.Date)//Donot run weekly report as monthly report will be run
            {
                //GetTimesheetReportData(DateTime.Now.Date.AddDays(-7), DateTime.Now.Date.AddDays(-1));
                GetTimesheetReportData(DateTime.Parse("18/02/2019", new CultureInfo("en-GB", true)), DateTime.Parse("24/02/2019", new CultureInfo("en-GB", true)));
            }
            
        }
        //public void ProcessMonthlyReport()
        //{
        //    DateTime lastMonth = DateTime.Now.Date.AddMonths(-1).AddDays(1);
        //    GetTimesheetReportData(lastMonth.Date, DateTime.Now.Date);
        //}

        public void GetTimesheetReportData(DateTime FromDate, DateTime ToDate)
        {
            TimeSheetQueryModel TimeSheetQueryModelObj = new TimeSheetQueryModel();
            ITimesheetHelper EmployeeAttendanceHelperObj = new TimesheetClient();
            IList<DailyTimeNotMaintainedModel> lstTimeSheetDailyNotMaintained = new List<DailyTimeNotMaintainedModel>();
            IList<WeeklyTimeNotMaintainedModel> lstTimeSheetWeeklyNotMaintained = new List<WeeklyTimeNotMaintainedModel>();
            string errorMessage = string.Empty;
           
            IList<long> lstUsers;
            try
            {
                TimeSheetQueryModelObj.FromDate = FromDate; 
                TimeSheetQueryModelObj.ToDate = ToDate; 
                TimeSheetQueryModelObj.UserID = EmployeeAttendanceHelperObj.GetHrUserId();
                TimeSheetQueryModelObj.MyDirectEmployees = false;
                TimeSpan expectedDayWorkTime = TimeSpan.Zero;
                List<TimeSheetModel> timeSheetModelList = EmployeeAttendanceHelperObj.GetMyTeamTimeSheet(TimeSheetQueryModelObj.UserID, TimeSheetQueryModelObj.FromDate, TimeSheetQueryModelObj.ToDate, TimeSheetQueryModelObj.MyDirectEmployees);                
                timeSheetModelList = timeSheetModelList.OrderBy(x => x.userID).ToList();
                IList<WeeklyDateBlocksModel> lstWeeklyDates = BuildWeeklyDateBlocks(TimeSheetQueryModelObj.FromDate, TimeSheetQueryModelObj.ToDate);
                lstUsers = timeSheetModelList.Select(x => x.userID).Distinct().ToList();
                DailyTimeNotMaintainedModel dayMdl;
                foreach (var item in timeSheetModelList)
                {
                    if (!IsOneDayTimeMaintained(item, out expectedDayWorkTime))
                    {
                        dayMdl = new DailyTimeNotMaintainedModel();
                        dayMdl.WorkingDay = item.WorkingDate;
                        dayMdl.WorkingDayText = item.WorkingDate.DayOfWeek.ToString();
                        dayMdl.Shift = item.Shift;
                        dayMdl.InTime = item.InTime.ToShortTimeString();
                        dayMdl.OutTime = item.OutTime.ToShortTimeString();
                        dayMdl.TotalDayWorkingHoursFormatted = item.WorkingHours.ToString("hh") + ":" + item.WorkingHours.ToString("mm") + ":" + item.WorkingHours.ToString("ss");
                        //dayMdl.ExpectedDayWorkingHoursFormatted = expectedDayWorkTime.ToString("hh") + ":" + expectedDayWorkTime.ToString("mm") + ":" + expectedDayWorkTime.ToString("ss");
                        dayMdl.Status = item.Status;
                        dayMdl.Request = item.Requests;
                        dayMdl.UserId = item.userID;                      
                        lstTimeSheetDailyNotMaintained.Add(dayMdl);
                    }
                }

                WeeklyTimeNotMaintainedModel weekMdl;
                TimeSpan totalWeekTime = TimeSpan.Zero;
                TimeSpan calculatedWeekTime = TimeSpan.Zero;
                bool isWeeklyMet = true;
                foreach (var item in lstUsers)
                {
                    foreach (var week in lstWeeklyDates)
                    {
                        IsWeekTimeMaintained(timeSheetModelList.Where(x => x.WorkingDate.Date >= week.WeekDayStartDate.Date && x.WorkingDate.Date <= week.WeekDayEndDate && x.userID == item).ToList(), out totalWeekTime, out calculatedWeekTime);

                        TimeSpan minWeekTime = new TimeSpan(45, 0, 0);
                        if (calculatedWeekTime.Ticks < minWeekTime.Ticks)
                        {
                            isWeeklyMet = false;
                        }                       
                        weekMdl = new WeeklyTimeNotMaintainedModel();
                        weekMdl.DateRange = week.WeekDayStartDate.ToShortDateString() +" - " +week.WeekDayEndDate.ToShortDateString();
                        weekMdl.WorkFromHomeQty = 0;
                        weekMdl.Permissions = "Permission(O: " + 0 + "hrs" + "P: " + 0 + "hrs)";
                        weekMdl.TotalWeekWorkingHoursFormatted = totalWeekTime.TotalHours.ToString().Substring(0, (totalWeekTime.TotalHours.ToString().IndexOf(".", StringComparison.Ordinal))) + ":" + totalWeekTime.ToString("mm") + ":" + totalWeekTime.ToString("ss");
                        weekMdl.Requests = 0;
                        weekMdl.IsWeeklyTimeMet = isWeeklyMet;
                        weekMdl.UserId = item;
                        lstTimeSheetWeeklyNotMaintained.Add(weekMdl);
                        
                    }
                }

                //Get unique UserIds from both above lists
                string body = string.Empty;
                ITimesheetHelper timesheetClient = new TimesheetClient();
                IList<long> lstlstFinalUsers = lstTimeSheetDailyNotMaintained.Select(x => x.UserId).Distinct().ToList();
                IList<long> lstlstFinalWeekwiseUsers = lstTimeSheetWeeklyNotMaintained.Select(x => x.UserId).Distinct().ToList();
                foreach (var item in lstlstFinalWeekwiseUsers)
                {
                    if (lstlstFinalUsers.Any(x => x == item) == false)
                    {
                        lstlstFinalUsers.Add(item);
                    }
                }
                IList<UserEmailListModel> lstUserEmail = timesheetClient.GetUserEmailData();
                List<string> ccEmailaddresses = new List<string>();
                foreach (var userId in lstlstFinalUsers)
                {
                    ccEmailaddresses = new List<string>();
                    body = PrepareEmployeeEmailBody(lstTimeSheetDailyNotMaintained.Count > 0 ? lstTimeSheetDailyNotMaintained.Where(x => x.UserId == userId).ToList() : null, lstTimeSheetWeeklyNotMaintained.Count > 0 ? lstTimeSheetWeeklyNotMaintained.Where(x => x.UserId == userId).ToList() : null, lstUserEmail.Where(x => x.UserId == userId).FirstOrDefault().FirstName + " " + lstUserEmail.Where(x => x.UserId == userId).FirstOrDefault().LastName);
                    if (body != "NoEmail")
                    {
                        ccEmailaddresses.Add(lstUserEmail.Where(x => x.UserId == userId).FirstOrDefault().ReportingToEmailAddress);
                        SendEmail(body, lstUserEmail.Where(x => x.UserId == userId).FirstOrDefault().EmployeeEmailAddress, ccEmailaddresses, "LMS Timesheet Weekly Descripency Alert");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string PrepareEmployeeEmailBody(IList<DailyTimeNotMaintainedModel> lstDaywiseEmp, IList<WeeklyTimeNotMaintainedModel> lstEmpWeekwise, string empName)
        {
            if ((lstDaywiseEmp == null ? false : lstDaywiseEmp.Count == 0) && (lstEmpWeekwise == null ? false : lstEmpWeekwise.Count == 0))
            {
                return "NoEmail";
            }
            string body = string.Empty;

                       
            foreach (var mdl in lstEmpWeekwise)
            {
                if ((lstDaywiseEmp == null ? false : lstDaywiseEmp.Count > 0) || mdl.IsWeeklyTimeMet == false)
                {
                    body += "<tr class='weeklytotal'><th>Summary</th><th>" + mdl.DateRange + "</th><th>Work From Home : " + mdl.WorkFromHomeQty + "</th><th colspan='2'>" + mdl.Permissions + "</th><th>" + mdl.TotalWeekWorkingHoursFormatted + "</th><th></th><th>" + mdl.Requests + "</th></tr>";
                }
                if (lstDaywiseEmp == null ? false : lstDaywiseEmp.Count > 0)
                {
                    foreach (var dayMdl in lstDaywiseEmp)
                    {
                        body += "<tr><th>" + dayMdl.WorkingDay + "</th><th>" + dayMdl.WorkingDayText + "</th><th>" + dayMdl.Shift + "</th><th>" + dayMdl.InTime + "</th><th>" + dayMdl.OutTime + "</th><th>" + dayMdl.TotalDayWorkingHoursFormatted + "</th><th>" + dayMdl.Status + "</th><th>" + dayMdl.Request + "</th></tr>";
                    }
                }
            }           
            
            return body;
        }
        public void SendEmail(string body, string toEmailAddress, IList<string> lstCCEmailAddresses,string subject)
        {
            string emailBody = "<!DOCTYPE html><html><head><style>table{font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; width: 100%; width: 100%; margin: 0 auto; clear: both; border-collapse: separate; border-spacing: 0; font-weight: normal; font-size: 12px; border-top: 1px solid #111; border-bottom: 1px solid #111;}tr.weeklytotal{background: #DFD5FD !important; font-weight: bold;}td, th{border: 1px solid #dddddd; text-align: left; padding: 8px;}tbody th, tbody td{padding: 8px 10px;}tbody > tr > td{vertical-align: top; border-top: 1px solid #ddd;}tr:nth-child(odd){background-color: #ffffffff;}tr:nth-child(even){background-color: #f9f9f9;}td.red{color: red;}td.blue{color: dodgerblue;}</style></head><body><table> <tbody> <tr> <th>Date</th> <th>Day</th> <th>Shift</th> <th>In Time</th> <th>Out Time</th> <th>Working Hours</th> <th>Status</th> <th>Requests</th> </tr>";            
            emailBody += body;
            emailBody += "</tbody></table></body></html>";
            EmailHelper email = new EmailHelper();

#if DEBUG
            email.SendHtmlFormattedEmail(toEmailAddress, lstCCEmailAddresses, subject, emailBody);
#else
            BackgroundJob.Enqueue(() => email.SendHtmlFormattedEmail(toEmailAddress, lstCCEmailAddresses, subject, emailBody));
#endif
        }
        public string PrepareLeadEmailBody()
        {
            return null;
        }
        public IList<WeeklyDateBlocksModel> BuildWeeklyDateBlocks(DateTime fromDate, DateTime toDate)
        {
            //Monday to Sunday is 1 week block
            IList<WeeklyDateBlocksModel> lstWeeklyDates = new List<WeeklyDateBlocksModel>();
            DateTime currDate = fromDate;

            WeeklyDateBlocksModel mdl;
            while (currDate.Date <= toDate.Date)
            {
                if (currDate.DayOfWeek.ToString() == "Monday")
                {
                    mdl = new WeeklyDateBlocksModel();
                    mdl.WeekDayStartDate = currDate.Date;

                    if (currDate.Date.AddDays(6) <= toDate.Date)
                    {
                        mdl.WeekDayEndDate = currDate.AddDays(6);
                        lstWeeklyDates.Add(mdl);
                        currDate = currDate.AddDays(6);
                    }
                    else
                    {
                        break;
                    }

                }
                currDate = currDate.AddDays(1);
            }


            return lstWeeklyDates;
        }
        public bool IsOneDayTimeMaintained(TimeSheetModel timesheet, out TimeSpan expectedDayTime)
        {
            TimeSpan minOneDayTime = new TimeSpan(9, 00, 00);
            Tuple<TimeSpan, TimeSpan> dayTimes = CalculateTimeForADay(timesheet);
            expectedDayTime = dayTimes.Item2;
            if (timesheet.Status == "Week Off")
            {
                return true;
            }
            if (dayTimes.Item1.Ticks >= minOneDayTime.Ticks)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public Tuple<TimeSpan, TimeSpan> CalculateTimeForADay(TimeSheetModel timesheet)
        {
            TimeSpan expectedWorkTime = TimeSpan.Zero;
            TimeSpan calculatedWorkTime = TimeSpan.Zero;

            calculatedWorkTime = timesheet.WorkingHours;
            expectedWorkTime = new TimeSpan(9, 00, 00);

            if (timesheet.Status == "Holiday")
            {
                calculatedWorkTime += new TimeSpan(9, 00, 00);
                expectedWorkTime -= new TimeSpan(9, 00, 00);
            }
            if (timesheet.LeaveDayQty == 1)
            {
                calculatedWorkTime += new TimeSpan(9, 00, 00);
                expectedWorkTime -= new TimeSpan(9, 00, 00);
            }
            if ((double)timesheet.LeaveDayQty == 0.5)
            {
                calculatedWorkTime += new TimeSpan(4, 30, 00);
                expectedWorkTime -= new TimeSpan(4, 30, 00);
            }
            //Permission Hours
            if (timesheet.permissionCountPersonal > 0)
            {
                int minutes = (int)((timesheet.permissionCountPersonal - Math.Truncate(timesheet.permissionCountPersonal)) * 60);
                int hours = (int)(timesheet.permissionCountPersonal - (timesheet.permissionCountPersonal - Math.Truncate(timesheet.permissionCountPersonal)));
                calculatedWorkTime += new TimeSpan(hours, minutes, 00);
                expectedWorkTime -= new TimeSpan(hours, minutes, 00);
            }
            if (timesheet.permissionCountOfficial > 0)
            {
                int minutes = (int)((timesheet.permissionCountOfficial - Math.Truncate(timesheet.permissionCountOfficial)) * 60);
                int hours = (int)(timesheet.permissionCountOfficial - (timesheet.permissionCountOfficial - Math.Truncate(timesheet.permissionCountOfficial)));
                calculatedWorkTime += new TimeSpan(hours, minutes, 00);
                expectedWorkTime -= new TimeSpan(hours, minutes, 00);
            }

            Tuple<TimeSpan, TimeSpan> times = new Tuple<TimeSpan, TimeSpan>(calculatedWorkTime, expectedWorkTime);
            return times;            
        }
        public void IsWeekTimeMaintained(IList<TimeSheetModel> lstweekTimeSheet, out TimeSpan totalWeekHours, out TimeSpan calculatedWeekHours)
        {
            TimeSpan totalWeekTime = TimeSpan.Zero;
            TimeSpan totalCalculatedWeekTime = TimeSpan.Zero;
            TimeSpan totalExpectedWeekTime = TimeSpan.Zero;
            TimeSpan minWeekTime = new TimeSpan(45, 0, 0);
            Tuple<TimeSpan, TimeSpan> dayTimes;
            foreach (var day in lstweekTimeSheet)
            {
                if (day.Status != "Week Off")
                {
                    dayTimes = CalculateTimeForADay(day);
                    totalCalculatedWeekTime += dayTimes.Item1;
                    totalExpectedWeekTime += dayTimes.Item2;
                    totalWeekTime = totalWeekTime.Add(new TimeSpan(0, day.WorkingHours.Hours, day.WorkingHours.Minutes, day.WorkingHours.Seconds, 0));
                }
            }
            totalWeekHours = totalWeekTime;
            calculatedWeekHours = totalCalculatedWeekTime;
           
        }

    }
}