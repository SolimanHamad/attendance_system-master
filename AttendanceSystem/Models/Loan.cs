using System;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class Loan
    {
        public string Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdateAt { get; set; }

        public int OriginalAmount { get; set; }

        public int RemainingAmount { get; set; }

        public int MonthlyPayment { get; set; }

        [Required]
        public string UserID { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
