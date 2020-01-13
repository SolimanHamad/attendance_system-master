using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystem.Models
{
    public class SalarySheet
    {
        [Key]
        public string ID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int Salary { get; set; }
        [Required]
        public int AttendancePenalties { get; set; }
        [Required]
        public int UnpaidVacationDeduction { get; set; }
        [Required]
        public int InsuranceDeduction { get; set; }
        [Required]
        public int LoanDeduction { get; set; }
        [Required]
        public int OtherDeductions { get; set; }
        [Required]
        public int HousingAllowance { get; set; }
        [Required]
        public int TransportationAllowance { get; set; }
        [Required]
        public int OtherAdditions { get; set; }
        [Required]
        public int FinalSalary { get; set; }
        public string Comment { get; set; }
        [Required]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        public void SetFinalSalary()
        {
            FinalSalary = (Salary + HousingAllowance + TransportationAllowance + OtherAdditions) - (AttendancePenalties + UnpaidVacationDeduction + InsuranceDeduction + LoanDeduction + OtherDeductions);
        }
    }
}
