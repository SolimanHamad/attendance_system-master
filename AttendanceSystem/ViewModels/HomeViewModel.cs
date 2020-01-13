using AttendanceSystem.Models;
using System;
using System.Collections.Generic;

namespace AttendanceSystem.ViewModels
{
    public class HomeViewModel
    {
        public string Countdown { get; set; } 

        public string CurrentUTCTime { get; set; } = DateTime.UtcNow.ToString("HH:mm:ss");

        public string NowDate { get; set; } = DateTime.Now.Date.ToString("yyyy-MM-ddT");

        public string LastEventTime { get; set; } 

        public bool OnVacation { get; set; } 

        public bool NotToday { get; set; } 

        public bool MissedEventAdded { get; set; }

        public bool CheckedIn { get; set; } 

        public bool UserIsLate { get; set; } 

        public IEnumerable<TimeLineDayViewModel> TimeLine { get; set; }

    }
}
