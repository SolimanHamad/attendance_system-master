using System;

namespace AttendanceSystem.Utilities
{
    public class VacationUtilities
    {
        public static double GetWorkingDays(DateTime from, DateTime to)
        {
            // half day vacations
            if ((to - from).TotalDays == 0.5)
            {
                return !IsOnWeekend(to) ? 0.5 : 0;
            }

            var totalDays = 0;
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (!IsOnWeekend(date))
                    totalDays++;
            }

            return totalDays;
        }

        private static bool IsOnWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday;
        }
    }
}
