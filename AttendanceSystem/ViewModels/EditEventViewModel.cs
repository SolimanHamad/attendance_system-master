using AttendanceSystem.Models;
using System;
using System.ComponentModel.DataAnnotations;
namespace AttendanceSystem.ViewModels
{
    public class EditEventViewModel
    {
        [Required]
        public string EventID { get; set; }
        [Required]
        public string WorkingDayID { get; set; }
        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime Time { get; set; }
        [Required]
        public Event Event { get; set; }
        [MaxLength(250, ErrorMessage = "Comment is exceeded the maximum allowed length: {1} charecters")]
        public string Comment { get; set; }
    }
}
