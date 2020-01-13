using AttendanceSystem.Attributes;
using AttendanceSystem.Data.JsonParsers;
using AttendanceSystem.Models;
using AttendanceSystem.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.ViewModels
{
    public class VacationViewModel : IValidatableObject
    {
        [NotDisplayed]
        public string Id { get; set; }

        [JsonConverter(typeof(HumanDateConverter))]
        [Display(Name = "Issued at")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddT16:00}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }

        [DisplayName("By User")]
        public string UserFullName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(OnlyDateConverter))]
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddT16:00}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [JsonConverter(typeof(OnlyDateConverter))]
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddT16:00}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Total Days")]
        public double TotalDays => VacationUtilities.GetWorkingDays(StartDate, EndDate);

        [NotDisplayed]
        public RequestStatus Status { get; set; }

        [DisplayName("Status")]
        public string FormattedStatus => Status.ToString();

        [Required]
        [NotDisplayed]
        public VacationType VacationType { get; set; }

        [DisplayName("Vacation Type")]
        public string FormattedVacationType => VacationType.ToString();

        public string Comment { get; set; }

        [NotDisplayed]
        public double VacationBalance { get; set; }

        [NotDisplayed]
        public string UserId { get; set; }

        public enum DisplayedColumns
        {

            CreatedAt = 0,
            UserFullName = 1,
            TotalDays = 2,
            Status = 3,
            VacationType = 4,
            Comment = 5

        }

        public VacationViewModel(Vacation vacation)
        {
            UserFullName = vacation.User?.FullName ?? "";
            Id = vacation.ID;
            CreatedAt = vacation.CreatedAt;
            StartDate = vacation.StartDate;
            EndDate = vacation.EndDate;
            Status = vacation.Status;
            VacationType = vacation.VacationType;
            Comment = vacation.Comment;
            VacationBalance = vacation.User?.VacationBalance ?? 0;
            UserId = vacation.UserID ?? "";
        }

        public VacationViewModel() { }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TotalDays <= 0 && (EndDate - StartDate).TotalDays == 0.5)
            {
                yield return new ValidationResult("Make sure the selected day is not on a weekend!");
            }

            else if (TotalDays <= 0)
            {
                yield return new ValidationResult("Make sure End Date is after Start Date, and not all selected vacation days are on a weekend!");
            }

            else if (!string.IsNullOrEmpty(UserId) && VacationType == VacationType.Personal)
            {
                if (TotalDays > VacationBalance)
                {
                    yield return new ValidationResult($"Asking for {TotalDays} vacation days while only {VacationBalance} days vacation balance are available!");
                }
            }
        }
    }
}
