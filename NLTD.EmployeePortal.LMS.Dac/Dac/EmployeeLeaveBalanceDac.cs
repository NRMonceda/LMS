using NLTD.EmployeePortal.LMS.Common.DisplayModel;
using NLTD.EmployeePortal.LMS.Dac.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public class EmployeeLeaveBalanceDac : IDisposable
    {
        public void Dispose()
        {
            //Nothing to implement...
        }

        public IList<LeaveBalanceEmpProfile> GetLeaveBalanceEmpProfile(Int64 userId)
        {
            IList<LeaveBalanceEmpProfile> retModel = new List<LeaveBalanceEmpProfile>();

            using (var context = new NLTDDbContext())
            {
                if (userId > 0)
                {
                    var employeeLeaveBalanceProfile = (from employee in context.Employee
                                                       where employee.UserId == userId
                                                       select new EmployeeLeaveBalanceProfile
                                                       {
                                                           LogonId = employee.LoginId,
                                                           EmployeeId = employee.EmployeeId,
                                                           UserId = employee.UserId,
                                                           FirstName = employee.FirstName,
                                                           LastName = employee.LastName,
                                                           ReportedToId = employee.ReportingToId
                                                       }).FirstOrDefault();

                    var repName = context.Employee.Where(x => x.UserId == employeeLeaveBalanceProfile.ReportedToId).FirstOrDefault();
                    if (repName != null)
                    {
                        employeeLeaveBalanceProfile.ReportedToName = repName.FirstName + " " + repName.LastName;
                    }

                    var employeeLeaveBalance = (from l in context.LeaveType
                                                from elb in context.EmployeeLeaveBalance.Where(x => x.LeaveTypeId == l.LeaveTypeId && x.UserId == userId && x.Year == DateTime.Now.Year).DefaultIfEmpty()
                                                where l.AdjustLeaveBalance == true
                                                orderby l.LeaveTypeId
                                                select new EmployeeLeaveBalanceDetails
                                                {
                                                    LeaveTypeId = l.LeaveTypeId,
                                                    OfficeId = l.OfficeId,
                                                    Type = l.Type,
                                                    AdjustLeaveBalance = l.AdjustLeaveBalance,
                                                    LeaveBalanceId = elb.LeaveBalanceId,
                                                    ExistingTotalDays = elb.TotalDays ?? 0,
                                                    BalanceDays = elb.BalanceDays,
                                                    UserId = elb.UserId,
                                                    Year = elb.Year
                                                }).ToList();

                    retModel.Add(new LeaveBalanceEmpProfile
                    {
                        employeeLeaveBalanceProfile = employeeLeaveBalanceProfile,
                        lstEmployeeLeaveBalance = employeeLeaveBalance,
                        Name = employeeLeaveBalanceProfile.FirstName + " " + employeeLeaveBalanceProfile.LastName
                    });
                }
            }

            return retModel;
        }

        public string UpdateLeaveBalance(List<EmployeeLeaveBalanceDetails> empLeaveBalanceDetails, Int64 LoginUserId, bool isElCredit = false)
        {
            try
            {
                int isSaved = 0;

                using (var context = new NLTDDbContext())
                {
                    EmployeeDac employeeDac = new EmployeeDac();
                    string userRole = string.Empty;
                    if (LoginUserId == 0)
                    {
                        userRole=employeeDac.GetEmployeeRole(LoginUserId);
                    }
                    else
                    {
                        userRole = "System";
                    }
                        
                    if (userRole == "HR" || userRole=="System")
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            int creditYear = Convert.ToDateTime(context.LeaveType.Where(x => x.Type == "Earned Leave").FirstOrDefault().lastCreditRun).Year;

                            foreach (var item in empLeaveBalanceDetails)
                            {
                                if (item.NoOfDays > 0)
                                {
                                    EmployeeLeaveBalance leaveBalance;
                                    if (isElCredit == false)
                                    {
                                        leaveBalance = context.EmployeeLeaveBalance.Where(x => x.UserId == item.UserId && x.LeaveTypeId == item.LeaveTypeId
                                        && x.LeaveBalanceId == item.LeaveBalanceId && x.Year == DateTime.Now.Year).FirstOrDefault();
                                    }
                                    else
                                    {
                                        leaveBalance = context.EmployeeLeaveBalance.Where(x => x.UserId == item.UserId && x.LeaveTypeId == item.LeaveTypeId
                                        && x.LeaveBalanceId == item.LeaveBalanceId && x.Year == creditYear).FirstOrDefault();
                                    }

                                    if (leaveBalance != null)
                                    {
                                        leaveBalance.TotalDays = item.CreditOrDebit == "C" ? (leaveBalance.TotalDays + item.NoOfDays) : (leaveBalance.TotalDays - item.NoOfDays);
                                        leaveBalance.ModifiedBy = LoginUserId;
                                        leaveBalance.ModifiedOn = DateTime.Now;
                                        leaveBalance.BalanceDays = item.TotalDays;
                                        isSaved = context.SaveChanges();
                                    }
                                    else
                                    {
                                        leaveBalance = new EmployeeLeaveBalance();
                                        leaveBalance.UserId = Convert.ToInt64(item.UserId);
                                        if (isElCredit == false)
                                        {
                                            leaveBalance.Year = DateTime.Now.Year;
                                        }
                                        else
                                        {
                                            leaveBalance.Year = creditYear;
                                        }
                                        leaveBalance.LeaveTypeId = Convert.ToInt64(item.LeaveTypeId);
                                        leaveBalance.TotalDays = item.TotalDays;
                                        leaveBalance.LeaveTakenDays = 0;
                                        leaveBalance.PendingApprovalDays = 0;
                                        leaveBalance.BalanceDays = item.TotalDays;
                                        leaveBalance.CreatedBy = LoginUserId;
                                        leaveBalance.CreatedOn = DateTime.Now;
                                        leaveBalance.ModifiedBy = LoginUserId;
                                        leaveBalance.ModifiedOn = DateTime.Now;
                                        context.EmployeeLeaveBalance.Add(leaveBalance);
                                        isSaved = context.SaveChanges();
                                    }

                                    LeaveTransactionHistory leaveTransactionHistory = new LeaveTransactionHistory();
                                    leaveTransactionHistory.UserId = Convert.ToInt64(item.UserId);
                                    leaveTransactionHistory.LeaveTypeId = Convert.ToInt64(item.LeaveTypeId);
                                    leaveTransactionHistory.LeaveId = -1;
                                    leaveTransactionHistory.TransactionDate = DateTime.Now;
                                    leaveTransactionHistory.TransactionType = item.CreditOrDebit;
                                    leaveTransactionHistory.NumberOfDays = item.NoOfDays;
                                    leaveTransactionHistory.TransactionBy = LoginUserId;
                                    leaveTransactionHistory.Remarks = item.Remarks;
                                    context.LeaveTransactionHistory.Add(leaveTransactionHistory);
                                    isSaved = context.SaveChanges();
                                    if (isSaved > 0)
                                        continue;
                                    else
                                        break;
                                }
                            }

                            if (isSaved > 0 && isElCredit == true)
                            {
                                var leaveType = context.LeaveType.Where(x => x.Type.ToUpper() == "EARNED LEAVE").FirstOrDefault();

                                if (leaveType != null)
                                {
                                    leaveType.lastCreditRun = System.DateTime.Now;
                                    leaveType.ModifiedBy = LoginUserId;
                                    leaveType.Modifiedon = System.DateTime.Now;
                                    isSaved = context.SaveChanges();
                                }
                            }
                            if (isSaved > 0)
                            {
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                    }
                    else
                    {
                        return "Need Role";
                    }
                }
                if (isSaved > 0)
                    return "Saved";
                else
                    return "Failed";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}