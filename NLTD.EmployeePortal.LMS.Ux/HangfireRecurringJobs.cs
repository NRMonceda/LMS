using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using NLTD.EmployeePortal.LMS.Ux.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

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
            //string timesheetMonthlyEmailServiceConfig = ConfigurationManager.AppSettings["TimesheetMonthlyEmailServiceConfig"];
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
            var monitor = JobStorage.Current.GetMonitoringApi();
            var toDelete = new List<string>();
            foreach (QueueWithTopEnqueuedJobsDto queue in monitor.Queues())
            {
                for (var i = 0; i < Math.Ceiling(queue.Length / 1000d); i++)
                {
                    monitor.EnqueuedJobs(queue.Name, 1000 * i, 1000)
                        .ForEach(x => toDelete.Add(x.Key));
                    monitor.ScheduledJobs(1000 * i, 1000).ForEach(x => toDelete.Add(x.Key));
                }
            }
            foreach (var job in toDelete)
            {
                BackgroundJob.Delete(job);
            }

            //Add TimesheetWeeklyEmailService if config exists
            if (!string.IsNullOrWhiteSpace(timesheetWeeklyEmailServiceConfig))
            {
                RecurringJob.AddOrUpdate(() => TimesheetWeeklyEmailService(), timesheetWeeklyEmailServiceConfig, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            }
            if (!string.IsNullOrWhiteSpace(timesheetWeeklyEmailServiceConfig))
            {
                RecurringJob.AddOrUpdate(() => CreditMonthlyCLSL(), "0 5 29 2 1");
            }
            //if (string.IsNullOrWhiteSpace(timesheetMonthlyEmailServiceConfig))
            //{
            //    if (recurringJobList != null && recurringJobList.Any())
            //    {
            //        var curJob = recurringJobList.FirstOrDefault(x => x.Id == "HangfireRecurringJobs.TimesheetMonthlyEmailService");
            //        if (curJob != null)
            //        {
            //            RecurringJob.RemoveIfExists(curJob.Id);

            //            if (!String.IsNullOrWhiteSpace(curJob.LastJobId))
            //            {
            //                RecurringJob.RemoveIfExists(curJob.LastJobId);
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    RecurringJob.AddOrUpdate(() => TimesheetMonthlyEmailService(), timesheetMonthlyEmailServiceConfig, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            //}
        }

        public void TimesheetWeeklyEmailService()
        {
            TimesheetEmailReportService srv = new TimesheetEmailReportService();
            srv.ProcessWeeklyReport();
        }

        //public void TimesheetMonthlyEmailService()
        //{
        //    TimesheetEmailReportService srv = new TimesheetEmailReportService();
        //    srv.ProcessMonthlyReport();
        //}
        public void CreditMonthlyCLSL()
        {
            ProfileController cs = new ProfileController();
            cs.UpdateCLSL();
        }
    }
}