using Hangfire;
using Hangfire.Storage;
using NLTD.EmployeePortal.LMS.Ux.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Timers;

namespace NLTD.EmployeePortal.LMS.Ux
{
    public class HangfireRecurringJobs
    {
        public HangfireRecurringJobs()
        {
        }

        public HangfireRecurringJobs(bool runFlag)
        {
            string timesheetWeeklyEmailServiceConfig = ConfigurationManager.AppSettings["TimesheetWeeklyEmailServiceConfig"];
            List<RecurringJobDto> recurringJobList;
            using (var connection = JobStorage.Current.GetConnection())
            {
                recurringJobList = connection.GetRecurringJobs();
            }

            //Remove Service if exists
            if (recurringJobList != null && recurringJobList.Any())
            {
                var curJob = recurringJobList.FirstOrDefault(x => x.Id == "HangfireRecurringJobs.TimesheetWeeklyEmailService");
                if (curJob != null)
                {
                    RecurringJob.RemoveIfExists(curJob.Id);

                    if (!String.IsNullOrWhiteSpace(curJob.LastJobId))
                    {
                        RecurringJob.RemoveIfExists(curJob.LastJobId);
                    }
                }
            }            

            //Add TimesheetWeeklyEmailService if config exists
            if (!string.IsNullOrWhiteSpace(timesheetWeeklyEmailServiceConfig))
            {
                RecurringJob.AddOrUpdate(() => TimesheetWeeklyEmailService(), timesheetWeeklyEmailServiceConfig, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            }
        }

        public void TimesheetWeeklyEmailService()
        {
            TimesheetEmailReportService srv = new TimesheetEmailReportService();
            srv.ProcessWeeklyReport();
        }
    }
}