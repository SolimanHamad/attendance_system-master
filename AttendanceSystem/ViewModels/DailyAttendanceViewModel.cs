using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.ViewModels
{
    public class DailyAttendanceViewModel
    {
        public string UserName { get; set; }
        public string LastEvent { get; set; }
        public AttendanceStatus Status { get; set; }
        public VacationType Type { get; set; }
    }

    public enum AttendanceStatus
    {
        Missing,
        Present,
        Left,
        Vacation
    }
}

