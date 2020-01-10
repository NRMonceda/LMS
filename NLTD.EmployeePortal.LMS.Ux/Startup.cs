using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(NLTD.EmployeePortal.LMS.Ux.Startup))]

namespace NLTD.EmployeePortal.LMS.Ux
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(ConfigurationManager.ConnectionStrings["LmsdbConn"].ConnectionString);

            //Fire and Forget

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new LMSSiteAdminAuthFilter() }
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                HeartbeatInterval = TimeSpan.FromMilliseconds(60000),
                ServerCheckInterval = TimeSpan.FromMilliseconds(60000),
                SchedulePollingInterval = TimeSpan.FromMilliseconds(60000)
            });
            HangfireRecurringJobs jobs = new HangfireRecurringJobs(true);
        }
    }

    public class LMSSiteAdminAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            string hangfireDashboardUsers = ConfigurationManager.AppSettings["SiteAdminUsers"].ToString();
            List<string> lstUsers = hangfireDashboardUsers.Split(',').ToList();

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                if (HttpContext.Current.User.Identity.Name.IndexOf("CORP\\", StringComparison.Ordinal) != -1)
                {
                    if (lstUsers.Any(x => x.ToUpper() == HttpContext.Current.User.Identity.Name.Substring(5).ToUpper()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}