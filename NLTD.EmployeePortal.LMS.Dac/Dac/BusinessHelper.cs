namespace NLTD.EmployeePortal.LMS.Dac.Dac
{
    public static class BusinessHelper
    {
        public static string ReturnLeaveStatusText(string leaveStatus)
        {
            switch (leaveStatus)
            {
                case ("P"):
                    return "Pending";

                case ("A"):
                    return "Approved";

                case ("R"):
                    return "Rejected";

                default:
                    return leaveStatus;
            }
        }
    }
}