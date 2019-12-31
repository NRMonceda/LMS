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
#if !DEBUG
            KeepAwakeIIS();
#endif
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
            if (recurringJobList != null && recurringJobList.Any())
            {
                var curJob = recurringJobList.FirstOrDefault(x => x.Id == "HangfireRecurringJobs.CreditMonthlyCLSL");
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

            RecurringJob.AddOrUpdate(() => CreditMonthlyCLSL(), "0 5 29 2 1");
        }

        public void TimesheetWeeklyEmailService()
        {
            TimesheetEmailReportService srv = new TimesheetEmailReportService();
            srv.ProcessWeeklyReport();
        }

        public void CreditMonthlyCLSL()
        {
            ProfileController cs = new ProfileController();
            cs.UpdateCLSL(1);
            cs.UpdateCLSL(14);
        }

        public void KeepAwakeIIS()
        {
            System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromMinutes(10).TotalMilliseconds)
            {
                AutoReset = true
            };
            timer.Elapsed += new System.Timers.ElapsedEventHandler(CallWebMethod);
            timer.Start();
        }

        public static void CallWebMethod(object sender, ElapsedEventArgs e)
        {
            string heartbeatResponse = string.Empty;
            using (WebClient wc = new WebClient())
            {
                heartbeatResponse = wc.DownloadString(ConfigurationManager.AppSettings["HeartbeatUrl"]);
            }
        }
    }
}