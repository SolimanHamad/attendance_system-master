using AttendanceSystem.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class MissedEventRequest
    {
        [Key]
        public string ID { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime Time { get; set; }

        [Required]
        public Event Event { get; set; }

        [Required]
        public Approval Approval { get; set; } = Approval.Pending;

        [MaxLength(250, ErrorMessage = "Comment has exceeded the maximum allowed length: {1} characters")]
        public string Comment { get; set; }

        [Required]
        public string UserID { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
