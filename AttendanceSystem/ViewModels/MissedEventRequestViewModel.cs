using AttendanceSystem.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.ViewModels
{
    public class MissedEventRequestViewModel
    {        
        [Required]
        public string UserID { get; set; }
        
        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddT16:00}", ApplyFormatInEditMode = true)]
        public DateTime Time { get; set; } = GetLastWorkingDayDate(); // show me last working day by default 

        [Required]
        public Event Event { get; set; }
        
        [MaxLength(250, ErrorMessage = "Comment is exceeded the maximum allowed length: {1} characters")]
        public string Comment { get; set; }

        private static DateTime GetLastWorkingDayDate()
        {
            DateTime time = DateTime.Now;
            switch (time.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    time = DateTime.Now.AddDays(-3); // show me Thursday if I was on Sunday 
                    break;
                case DayOfWeek.Saturday:
                    time = DateTime.Now.AddDays(-2); // show me Thursday if I was on Saturday 
                    break;
                default:
                    time = DateTime.Now.AddDays(-1); // show me yesterday. At 4 pm 
                    break;
            }
            return time;
        }
    }
}
