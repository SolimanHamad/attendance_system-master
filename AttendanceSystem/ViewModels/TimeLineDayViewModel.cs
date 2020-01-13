using AttendanceSystem.Models;
using AttendanceSystem.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AttendanceSystem.ViewModels
{
    public class TimeLineDayViewModel
    {
        public string UserID { get; set; }
        public DateTime Date { get; set; }
        public DateTime? CheckedIn { get; set; }
        public DateTime? CheckedOut { get; set; }
        public TimeSpan? WorkedTime { get; set; }
        public bool IsOnVacation { get; set; }
        public virtual ICollection<UserEvent> Events { get; set; }
        public bool IsAbsent { get; set; }
        public string VacationType { get; set; }


    }
}
