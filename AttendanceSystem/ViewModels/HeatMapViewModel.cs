using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels
{
    public class HeatMapViewModel
    {
        public ApplicationUser User { get; set; }

        [Display(Name = "Sundays")]
        public double SundayCount { get; set; }

        [Display(Name = "Mondays")]
        public double MondayCount { get; set; }

        [Display(Name = "Tuesdays")]
        public double TuesdayCount { get; set; }

        [Display(Name = "Wednesdays")]
        public double WednesdayCount { get; set; }

        [Display(Name = "Thursdays")]
        public double ThursdayCount { get; set; }

        [Display(Name = "Total Count")]
        public double TotalCount { get; set; }
    }
}