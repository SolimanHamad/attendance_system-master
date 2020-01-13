using AttendanceSystem.Data.JsonParsers;
using AttendanceSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public const string AdminRole = "Admin";

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(OnlyDateConverter))]
        [Display(Name = "Date Joined")]
        public DateTime DateJoined { get; set; }

        [Required]
        public double Salary { get; set; }

        [Required]
        [Display(Name = "Vacation Balance")]
        [Range(0, double.PositiveInfinity)]
        public double VacationBalance { get; set; }

        [Required]
        [Display(Name = "Yearly Increment")]
        [Range(0, double.PositiveInfinity)]
        public int YearlyIncrement { get; set; }

        [Required]
        [Display(Name = "Grace Period")]
        [Range(0, double.PositiveInfinity)]
        public TimeSpan GracePeriod { get; set; }

        [Required]
        [Display(Name = "Work Start Time")]
        public TimeSpan WorkStartTime { get; set; }

        [Required]
        [Display(Name = "Work End Time")]
        public TimeSpan WorkEndTime { get; set; }

        [Required]
        [Display(Name = "Insurance Deduction")]
        [Range(0, 100)]
        public int InsuranceDeduction { get; set; }

        [Required]
        [Display(Name = "Housing Allowance")]
        public int HousingAllowance { get; set; }

        [Required]
        [Display(Name = "Transportation Allowance")]
        public int TransportationAllowance { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool CheckAttendance { get; set; } = true;

        [Required]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; } = true;

        public virtual ICollection<MissedEventRequest> MissedEventRequests { get; set; } 
        public virtual ICollection<Vacation> Vacations { get; set; }
        public virtual ICollection<WorkingDay> WorkingDays { get; set; }

        public bool WasOnVacationYesterday()
        {
            if (Vacations?.Count > 0)
            {
                DateTime date = DateTime.Today.AddDays(-1).Date;
                return Vacations.Any(v => v.UserID == this.Id && v.Status == RequestStatus.Approved && 
                                          v.StartDate <= date && v.EndDate >= date && 
                                          new VacationViewModel(v).TotalDays >= 1.0);
            }
            return false;
        }

        public VacationType? GetCurrentVacationType(DateTime date)
        {
            if (Vacations?.Count > 0)
            {
                return Vacations.FirstOrDefault(v => v.UserID == this.Id && v.Status == RequestStatus.Approved && 
                                          v.StartDate <= date.Date && v.EndDate >= date.Date &&
                                          new VacationViewModel(v).TotalDays >= 1.0)?.VacationType;
            }
            return null;
        }

        public bool IsOnHalfDayVacation(DateTime date)
        {
            if (Vacations?.Count > 0)
            {
                return Vacations.Any(v => v.UserID == Id && v.Status == RequestStatus.Approved &&
                                          v.StartDate.Date == date.Date && v.EndDate.Date == date.Date &&
                                          new VacationViewModel(v).TotalDays == 0.5);
            }
            return false;
        }

        public bool IsOnVacation(DateTime date)
        {
            if (Vacations?.Count > 0)
            {
                return Vacations.Any(v => v.UserID == Id && v.Status == RequestStatus.Approved &&
                                          v.StartDate.Date <= date.Date && v.EndDate.Date >= date.Date &&
                                          new VacationViewModel(v).TotalDays > 0.5);
            }
            return false;
        }
    }
}
