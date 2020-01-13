using System;
using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.Models
{
    public class Offense
    {
        [Key]
        public string ID { get; set; }
        [Required]
        public DateTime IssueTime { get; set; } = DateTime.Now; // Offense issue time
        [Required]
        public int PenaltyPercent { get; set; }
        [Required]
        public OffenseDegree Degree { get; set; }
        [Required]
        public bool NeedApproval { get; set; }
        [Required]
        public bool HasAllowance { get; set; }
        [Required]
        public string WorkingDayID { get; set; }
        public virtual WorkingDay WorkingDay { get; set; }
        
        public Offense() {}

        public Offense(string workingDayID, int penaltyPercent, OffenseDegree degree)
        {
            WorkingDayID = workingDayID;
            PenaltyPercent = penaltyPercent;
            Degree = degree;
        }

    }
}
