using AttendanceSystem.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.ViewModels
{
    public class SalarySheetRecord
    {
        [Required]
        public string UserID { get; set; }
        
        public string UserName { get; set; }        
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
        
        [Required]
        public bool Editable { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public string Comment { get; set; }
        
        public SalarySheetRecord() { }
        
        public SalarySheetRecord(SalarySheet salarySheetRecord, bool editable)
        {
            UserID = salarySheetRecord.UserID;
            UserName = salarySheetRecord.User.FullName;
            Salary = salarySheetRecord.Salary;
            AttendancePenalties = salarySheetRecord.AttendancePenalties;
            UnpaidVacationDeduction = salarySheetRecord.UnpaidVacationDeduction;
            InsuranceDeduction = salarySheetRecord.InsuranceDeduction;
            LoanDeduction = salarySheetRecord.LoanDeduction;
            HousingAllowance = salarySheetRecord.HousingAllowance;
            TransportationAllowance = salarySheetRecord.TransportationAllowance;
            FinalSalary = salarySheetRecord.FinalSalary;
            OtherDeductions = salarySheetRecord.OtherDeductions;
            OtherAdditions = salarySheetRecord.OtherAdditions;
            Comment = salarySheetRecord.Comment;
            EndDate = salarySheetRecord.Date;
            Editable = editable;
        }

        public SalarySheetRecord(string userID, string userName, int salary, int attendancePenalties, int unpaidVacationDeduction,
            int insuranceDeduction, int loanDeduction, int housingAllowance, int transportationAllowance, int finalSalary,
            DateTime endDate ,bool editable)
        {
            UserID = userID;
            UserName = userName;
            Salary = salary;
            AttendancePenalties = attendancePenalties;
            UnpaidVacationDeduction = unpaidVacationDeduction;
            InsuranceDeduction = insuranceDeduction;
            LoanDeduction = loanDeduction;
            HousingAllowance = housingAllowance;
            TransportationAllowance = transportationAllowance;
            FinalSalary = finalSalary;
            EndDate = endDate;
            Editable = editable;
        }
    }
}
