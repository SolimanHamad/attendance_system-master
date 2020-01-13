using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models.Enums
{
    /** The set of offenses an user may conduct
    * is determined by this OffenseDegree enum **/
    public enum OffenseDegree
    {
        [Display(Name = "Leave early")]
        LeaveEarly,
        [Display(Name = "Late less than 1 hr, with make up")]
        LateLessThanHourWithMakeUp,
        [Display(Name = "Late less than 1 hr, without make up")]
        LateLessThanHourNoMakeUp,
        [Display(Name = "Late more than 1 hr, with make up")]
        LateMoreThanHourWithMakeUp,
        [Display(Name = "Late more than 1 hr, without make up")]
        LateMoreThanHourNoMakeUp,
        [Display(Name = "Absence")]
        Absence
    }
}
