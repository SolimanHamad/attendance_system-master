using AttendanceSystem.Models.Enums;
using AttendanceSystem.Models;

namespace AttendanceSystem.ViewModels
{
    public class DailyAttendanceViewModel
    {
        public string UserName { get; set; }
        public string LastEvent { get; set; }
        public AttendanceStatus Status { get; set; }
        public VacationType Type { get; set; }
        public bool CheckAttendance { get; set; }
    }

    public enum AttendanceStatus
    {
        Missing,
        Present,
        Left,
        Vacation
    }

    
}

