using System;
using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.Models
{
    public class EditEventRequest
    {
        [Key]
        public string ID { get; set; }

        [Required]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime NewTime { get; set; }

        [Required]
        public Approval Approval { get; set; } = Approval.Pending;

        [MaxLength(250, ErrorMessage = "Comment has exceeded the maximum allowed length: {1} charecters")]
        public string Comment { get; set; }

        [Required]  
        public string UserEventID { get; set; }

        public virtual UserEvent UserEvent { get; set; }
    }
}
