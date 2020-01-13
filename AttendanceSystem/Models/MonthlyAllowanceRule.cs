using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.Models
{
    public class MonthlyAllowanceRule
    {
        [Key]
        public OffenseDegree Degree { get; set; }
        [Required]
        public int UserMonthlyAllowance { get; set; }
        
        public MonthlyAllowanceRule() {}
        public MonthlyAllowanceRule(OffenseDegree degree, int userMonthlyAllowance)
        {
            Degree = degree;
            UserMonthlyAllowance = userMonthlyAllowance;
        }
    }
}
