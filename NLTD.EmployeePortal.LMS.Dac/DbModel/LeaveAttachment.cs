using System;

namespace NLTD.EmployeePortal.LMS.Dac.DbModel
{
    public class LeaveAttachment
    {
        public Int64 AttachmentId { get; set; }
        public Int64 LeaveId { get; set; }
        public String AttachmentDocumentTitle { get; set; }
        public String AttachmentPath { get; set; }
    }
}