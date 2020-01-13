using AttendanceSystem.Attributes;
using AttendanceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.ViewModels
{
    public class UserManagementModel
    {
        [Required]
        [NotDisplayed]
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [Display(Name = "Password")]
        [NotDisplayed]
        public string Password { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Joined")]
        [NotDisplayed]
        public DateTime DateJoined { get; set; } = DateTime.Now;

        [Required]
        public double Salary { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [Remote(action: "IsEmailInUse", controller: "UserManagement")] //remote validation via the provided action
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^(\d{10})$", ErrorMessage = "Wrong phone number!")]
        public string PhoneNumber { get; set; }

        [Required]
        [Range(0, double.PositiveInfinity, ErrorMessage = "This field can't be negative.")]
        [Display(Name = "Vacation Balance")]
        public double VacationBalance { get; set; }

        [Required]
        [Range(0, double.PositiveInfinity, ErrorMessage = "This field can't be negative.")]
        [Display(Name = "Yearly Increment")]
        public int YearlyIncrement { get; set; }

        [Required]
        [Display(Name = "Grace Period (min.)")]
        [NotDisplayed]
        public double GracePeriod { get; set; } = 15;

        [Required]
        [NotDisplayed]
        [Range(0, 100)]
        [Display(Name = "Insurance Deduction (%)")]
        public int InsuranceDeduction { get; set; }

        [Required]
        [NotDisplayed]
        [Display(Name = "Housing Allowance")]
        public int HousingAllowance { get; set; }

        [Required]
        [Display(Name = "Transportation Allowance")]
        [NotDisplayed]
        public int TransportationAllowance { get; set; }

        [Required]
        [Display(Name = "Work Start Time")]
        [DataType(DataType.Time)]
        [NotDisplayed]
        public TimeSpan WorkStartTime { get; set; } = new TimeSpan(9, 0, 0);

        [Required]
        [Display(Name = "Work End Time")]
        [DataType(DataType.Time)]
        [NotDisplayed]
        public TimeSpan WorkEndTime { get; set; } = new TimeSpan(16, 0, 0);

        [Required]
        [DefaultValue(true)]
        [ Display(Name =" Check attendance")]
        [NotDisplayed]
        public bool CheckAttendance { get; set; } = true;

        [Required]
        [Display(Name = "Is Admin?")]
        [NotDisplayed]
        public bool IsAdmin { get; set; }

        public UserManagementModel(ApplicationUser user, bool isAdmin = false)
        {
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            DateJoined = user.DateJoined;
            VacationBalance = user.VacationBalance;
            YearlyIncrement = user.YearlyIncrement;
            WorkStartTime = user.WorkStartTime;
            WorkEndTime = user.WorkEndTime;
            GracePeriod = user.GracePeriod.TotalMinutes;
            Salary = user.Salary;
            PhoneNumber = user.PhoneNumber;
            InsuranceDeduction = user.InsuranceDeduction;
            HousingAllowance = user.HousingAllowance;
            TransportationAllowance = user.TransportationAllowance;
            CheckAttendance = user.CheckAttendance;
            IsAdmin = isAdmin;
        }

        public UserManagementModel() { }
    }
}
