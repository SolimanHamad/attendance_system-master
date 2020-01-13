using System;

namespace AttendanceSystem.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        /// <summary>
        /// Adds a timespan value if the date is already in future
        /// if the date is in the past, the date is set to now + the given timespan
        /// </summary>
        /// <param name="dateTime">date time</param>
        /// <param name="timeSpan">TimeSpan to add</param>
        public static DateTime AddTimeSpanEnsureInFuture(DateTime dateTime,TimeSpan timeSpan)
        {
            if (dateTime < DateTime.Now)
                dateTime = DateTime.Now;
            return dateTime.AddTicks(timeSpan.Ticks);
        }
        
        /// <summary>
        /// Adds a certain amount of seconds if the date is already in future
        /// if the date is in the past, the date is set to now + the given days
        /// </summary>
        /// <param name="dateTime">date time</param>
        /// <param name="seconds">seconds to add</param>
        public static DateTime AddSecondsEnsureInFuture(DateTime dateTime,int seconds)
        {
            if (dateTime < DateTime.Now)
                dateTime = DateTime.Now;
            return dateTime.AddSeconds(seconds);
        }

        /// <summary>
        /// Checks weather this date time is the default date time value or not
        /// </summary>
        /// <param name="dateTime">date time</param>
        /// <returns>true if date time was default value,false otherwise</returns>
        public static bool IsDefault(this DateTime dateTime)
        {
            return dateTime == default(DateTime);
        }

        /// <summary>
        /// Gets time from unix time (epoch) in milliseconds
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns>Datetime</returns>
        public static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddMilliseconds(unixTime);
        }
        
        /// <summary>
        /// Converts a datetime object to unix timestamp
        /// </summary>
        /// <param name="date">datetime to convert</param>
        /// <returns>epoch time stamp for the given date</returns>
        public static long ToUnixTime(this DateTime date)
        {
            return Convert.ToInt64((date.ToUniversalTime() - Epoch).TotalSeconds);
        }

        public static DateTime GetMonthStartDate(this DateTime date, int monthShift = 0)
        {
            DateTime monthStartDate = date.AddMonths(monthShift);
            if (monthStartDate.Day < 26)
            {
                monthStartDate = monthStartDate.AddMonths(-1);
            }
            monthStartDate = monthStartDate.AddDays(26 - monthStartDate.Day); // Set Day value to 26

            return monthStartDate;
        }

        public static DateTime GetMonthEndDate(this DateTime date, int monthShift = 0)
        {
            DateTime monthEndDate = date.AddMonths(monthShift);
            if (monthEndDate.Day >= 26)
            {
                monthEndDate = monthEndDate.AddMonths(1);
            }
            monthEndDate = monthEndDate.AddDays(25 - monthEndDate.Day); // Set Day value to 25

            return monthEndDate;
        }
    }
}
