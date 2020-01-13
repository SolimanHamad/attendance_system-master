using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Attributes;
using AttendanceSystem.Data.JsonParsers;
using AttendanceSystem.Models;
using Newtonsoft.Json;

namespace AttendanceSystem.ViewModels
{
    public class LoanViewModel
    {
        [NotDisplayed]
        public string Id { get; set; }

        [JsonConverter(typeof(HumanDateConverter))]
        [Display(Name = "Issue Date")]
        public DateTime CreatedAt { get; set; }

        [DisplayName("For")]
        public string UserFullName { get; set; }

        [Required]
        [Range(0, double.PositiveInfinity, ErrorMessage = "This field can't be negative.")]
        [Display(Name = "Loan Amount")]
        public int OriginalAmount { get; set; }

        [Required]
        [Range(0, double.PositiveInfinity, ErrorMessage = "This field can't be negative.")]
        [Display(Name = "Remaining Amount")]
        public int RemainingAmount { get; set; }

        [Required]
        [Range(0, double.PositiveInfinity, ErrorMessage = "This field can't be negative.")]
        [Display(Name = "Monthly Payment")]
        public int MonthlyPayment { get; set; }


        [Display(Name = "No. Upcoming Months")]
        public int RemainingTime => (int)Math.Ceiling((double)RemainingAmount / MonthlyPayment);

        [NotDisplayed]
        public string UserId { get; set; }

        public LoanViewModel(Loan loan)
        {
            Id = loan.Id;
            CreatedAt = loan.CreatedAt;
            OriginalAmount = loan.OriginalAmount;
            RemainingAmount = loan.RemainingAmount;
            MonthlyPayment = loan.MonthlyPayment;
            UserFullName = loan.User.FullName;
            UserId = loan.UserID;
        }
        public LoanViewModel() { }
    }
}
