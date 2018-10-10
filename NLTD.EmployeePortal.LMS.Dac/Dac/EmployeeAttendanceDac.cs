using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public class EmployeeAttendanceDac : IEmployeeAttendanceHelper
    {
        public void Dispose()
        {
        }

        public List<EmployeeAttendanceModel> GetAttendance(Int64 UserID)
        {
            List<EmployeeAttendanceModel> employeeAttendanceModelList = new List<EmployeeAttendanceModel>();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    employeeAttendanceModelList = (from ea in context.EmployeeAttendance
                                                   where ea.UserID == UserID
                                                   select new EmployeeAttendanceModel
                                                   {
                                                       UserID = ea.UserID,
                                                       InOutDate = ea.InOutDate,
                                                       InOut = (ea.InOut ? "OUT" : "IN")
                                                   }).OrderByDescending(e => e.InOutDate).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return employeeAttendanceModelList;
        }

        public List<EmployeeAttendanceModel> GetAttendanceForRange(Int64 UserID, DateTime FromDateTime, DateTime ToDateTime, string requestLevelPerson, bool isDirectEmployees)
        {
            List<EmployeeAttendanceModel> employeeAttendanceModelList = new List<EmployeeAttendanceModel>();
            EmployeeDac employeeDac = new EmployeeDac();
            IList<Int64> employeeIDs = new List<Int64>();

            if (requestLevelPerson.ToUpper() == "MY")
            {
                employeeIDs.Add(UserID);
            }
            else
            {
                if (isDirectEmployees)
                {
                    employeeIDs = employeeDac.GetDirectEmployees(UserID);
                }
                else
                {
                    employeeIDs = employeeDac.GetEmployeesReporting(UserID);
                }
            }

            using (var context = new NLTDDbContext())
            {
                employeeAttendanceModelList = (from ea in context.EmployeeAttendance
                                               join e in context.Employee on ea.UserID equals e.UserId
                                               where employeeIDs.Contains(ea.UserID ?? 0) && ea.InOutDate >= FromDateTime && ea.InOutDate <= ToDateTime
                                               select new EmployeeAttendanceModel
                                               {
                                                   UserID = ea.UserID,
                                                   InOutDate = ea.InOutDate,
                                                   InOut = (ea.InOut ? "Out" : "In"),
                                                   Name = (e.FirstName + " " + e.LastName)
                                               }).OrderBy(e => e.UserID).ThenBy(e => e.InOutDate).ToList();
            }

            TimeSpan punchTime = new TimeSpan();
            TimeSpan breakTime = new TimeSpan();
            TimeSpan workTime = new TimeSpan();

            for (int i = 0; i <= employeeAttendanceModelList.Count - 1; i++)
            {
                if (i == employeeAttendanceModelList.Count - 1)
                {
                    employeeAttendanceModelList[i].BreakDuration = "End Of Day. Total Work : " + workTime.ToString() + ", Total Break : " + breakTime.ToString();
                }
                else if (employeeAttendanceModelList[i].UserID == employeeAttendanceModelList[i + 1].UserID)
                {
                    var currentPunchShift = employeeAttendanceModelList[i].InOutDate.Hour < 9 ? employeeAttendanceModelList[i].InOutDate.AddDays(-1) : employeeAttendanceModelList[i].InOutDate;
                    var nextPunchShift = employeeAttendanceModelList[i + 1].InOutDate.Hour < 9 ? employeeAttendanceModelList[i + 1].InOutDate.AddDays(-1) : employeeAttendanceModelList[i + 1].InOutDate;

                    punchTime = (employeeAttendanceModelList[i + 1].InOutDate - employeeAttendanceModelList[i].InOutDate);
                    if (currentPunchShift.Date.CompareTo(nextPunchShift.Date) == 0)
                    {
                        if (employeeAttendanceModelList[i].InOut == "In")
                        {
                            if (employeeAttendanceModelList[i + 1].InOut == "Out")
                            {
                                employeeAttendanceModelList[i].BreakDuration = "Work : " + punchTime.ToString();
                                workTime += punchTime;
                            }
                            else
                            {
                                employeeAttendanceModelList[i].BreakDuration = "Missing Out Punch";
                            }
                        }
                        else
                        {
                            if (employeeAttendanceModelList[i + 1].InOut == "In")
                            {
                                employeeAttendanceModelList[i].BreakDuration = "Break : " + punchTime.ToString();
                                breakTime += punchTime;
                            }
                            else
                            {
                                employeeAttendanceModelList[i].BreakDuration = "Missing In Punch";
                            }
                        }
                    }
                    else
                    {
                        employeeAttendanceModelList[i].BreakDuration = "End Of Day. Total Work : " + workTime.ToString() + ", Total Break : " + breakTime.ToString();
                        workTime = TimeSpan.Zero;
                        breakTime = TimeSpan.Zero;
                    }
                }
                else
                {
                    employeeAttendanceModelList[i].BreakDuration = "End Of Day. Total Work : " + workTime.ToString() + ", Total Break : " + breakTime.ToString();
                    workTime = TimeSpan.Zero;
                    breakTime = TimeSpan.Zero;
                }
            }

            return employeeAttendanceModelList.OrderBy(e => e.Name).ThenByDescending(e => e.InOutDate).ToList();
        }

        public List<EmployeeAttendanceModel> GetAccessCardAttendanceForRange(Int64 UserID, DateTime FromDateTime, DateTime ToDateTime, string requestLevelPerson)
        {
            List<EmployeeAttendanceModel> employeeAttendanceModelList = new List<EmployeeAttendanceModel>();
            try
            {
                using (var context = new NLTDDbContext())
                {
                    employeeAttendanceModelList = (from ea in context.EmployeeAttendance
                                                   join e in context.Employee on ea.UserID equals e.UserId
                                                   into ej
                                                   from d in ej.DefaultIfEmpty()
                                                   where ea.InOutDate >= FromDateTime && ea.InOutDate <= ToDateTime
                                                   select new EmployeeAttendanceModel
                                                   {
                                                       UserID = ea.UserID,
                                                       InOutDate = ea.InOutDate,
                                                       CardID = ea.CardID,
                                                       InOut = (ea.InOut ? "Out" : "In"),
                                                       Name = (d.UserId == 0 ? "Visitor" : (d.FirstName + " " + d.LastName))
                                                   }).OrderBy(e => e.CardID).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return employeeAttendanceModelList;
        }
    }
}