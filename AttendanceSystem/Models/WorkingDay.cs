using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AttendanceSystem.Models
{
    public class WorkingDay
    {
        [Key]
        public string ID { get; set; }
        [Required]
        public DateTime Date { get; set; } = DateTime.Today;
        public DateTime? CheckIn { get; set; } // First Check in
        public DateTime? CheckOut { get; set; } // Last Check out
        public TimeSpan? WorkingTime { get; set; } // Total working time in the day
        public string Comment { get; set; } = "";


        [Required]
        public string UserID { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<UserEvent> Events { get; set; }
        public virtual ICollection<Offense> Offenses { get; set; }

        public WorkingDay() {}

        public WorkingDay(string userID, DateTime date)
        {
            UserID = userID;
            Date = date;
        }

        /** Calculate the total working time from the first check in to the last check out.
         * Assuming that: no two duplicate consecutive events, otherwise: return null.
         * Note: any check in after the last check out will be ignored, and any check out before the first check in will also be ignored. **/
        private TimeSpan? CalculateWorkingTime()
        {
            TimeSpan workingTime = new TimeSpan();
            UserEvent[] eventsArray = Events?
                .OrderBy(record => record.Time)
                .SkipWhile(record => record.Event == Event.CheckedOut)
                .ToArray();

            if (eventsArray != null && eventsArray.Length > 1)
            {
                int remainingElementsInArray = eventsArray.Length;
                for (int i = 0; remainingElementsInArray > 1; i += 2)
                {
                    UserEvent checkInEvent = eventsArray[i];
                    UserEvent checkOutEvent = eventsArray[i + 1];
                    if (checkInEvent.Event != Event.CheckedIn || checkOutEvent.Event != Event.CheckedOut)
                    {
                        return null;
                    }
                    workingTime += checkOutEvent.Time.Subtract(checkInEvent.Time);
                    remainingElementsInArray -= 2;
                }
                return workingTime;
            }
            return null;
        }

        public void SetWorkingTime()
        {
            WorkingTime = CalculateWorkingTime();
        }

        public bool IsLate()
        {
            return CheckIn.HasValue && CheckIn.Value.TimeOfDay > User.WorkStartTime.Add(User.GracePeriod);
        }

        public bool IsLateMoreThanHour()
        {
            return CheckIn.HasValue && CheckIn.Value.TimeOfDay > User.WorkStartTime.Add(TimeSpan.FromHours(1));
        }

        public bool IsStayedLessThanRequired()
        {
            if (WorkingTime.HasValue && CheckIn.HasValue)
            {
                TimeSpan requiredWorkTime = User.WorkEndTime - User.WorkStartTime;
                // Account for half days
                if (User.IsOnHalfDayVacation(Date))
                    requiredWorkTime = requiredWorkTime.Divide(2.0);
                // Account for coming within grace period
                if (CheckIn.Value.TimeOfDay > User.WorkStartTime && CheckIn.Value.TimeOfDay <= User.WorkStartTime.Add(User.GracePeriod))
                    requiredWorkTime = User.WorkEndTime - CheckIn.Value.TimeOfDay;
                return WorkingTime.Value < requiredWorkTime;
            }
            return false;
        }

        public bool IsAbsent()
        {
            if (Date.DayOfWeek == DayOfWeek.Friday || Date.DayOfWeek == DayOfWeek.Saturday)
                return false;

            return !Events?.Any() ?? true;
        }
    }
}
