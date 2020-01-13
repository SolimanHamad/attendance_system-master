using System;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    /** This model has the event records of the employees
     * The primary key is ID **/
    public class UserEvent
    {
        [Key]
        public string ID { get; set; }
        [Required]
        public DateTime Time { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
        [Required]
        public Event Event { get; set; }
        [Required]
        public string WorkingDayID { get; set; }
        [Required]       
        public virtual WorkingDay WorkingDay { get; set; }


        public static string LastEventString(UserEvent userEvent)
        {
            string shownText = "";
            if (userEvent != null)
            {
                DateTime eventDateTime = userEvent.Time;
                switch (userEvent.Event)
                {
                    case Event.CheckedIn:
                        shownText = "<small>You clocked in</small> <br/>";
                        break;
                    case Event.CheckedOut:
                        shownText = "<small>You clocked out</small> <br/>";
                        break;
                    default:
                        shownText = "<small>Your last event was</small><br/>";
                        break;
                }

                if (eventDateTime.Date == DateTime.Today)                
                    return shownText +"Today @ "+ eventDateTime.ToString("hh:mm tt");
                if (eventDateTime.Date == DateTime.Today.AddDays(-1))
                    return shownText + "Yesterday @ " + eventDateTime.ToString("hh:mm tt");
                return shownText +"On "+ eventDateTime.ToString("ddd, dd MMM @ hh:mm tt");
            }
            return shownText;
        }

        public string TimeString()
        {
            if (Event == Event.CheckedIn)
            {
                return "Clocked in @ " + this.Time.ToShortTimeString();
            }
            return "Clocked out @ " + this.Time.ToShortTimeString();
        }
    }

    /** The set of events an user can
     * have is determined by this Event enum **/
    public enum Event
    {
        [Display(Name = "Check out")]
        CheckedOut, // The default value
        [Display(Name = "Check in")]
        CheckedIn
    }
}
