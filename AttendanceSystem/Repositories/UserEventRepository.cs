using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Extensions;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class UserEventRepository
    {
        private readonly AttendanceSystemContext db;
        private readonly WorkingDayRepository workingDayRepository;
        private readonly VacationRepository vacationRepository;

        public UserEventRepository(AttendanceSystemContext context)
        {
            db = context;
            workingDayRepository = new WorkingDayRepository(context);
            vacationRepository = new VacationRepository(context);
        }

        /** Add user event if it is not the same as the last recorded event
         * (prevents adding two consecutive same events, e.g. check in, check in) **/
        public async Task AddIfValid(string userID, UserEvent userEvent)
        {
            // Get the last event of the user
            UserEvent lastEvent = await GetLastEvent(userID);

            // Check if the new event is different than last event
            if (lastEvent?.Event != userEvent.Event || lastEvent.Time.Date != userEvent.Time.Date)
            {
                db.UserEventRecords.Add(userEvent);
                await db.SaveChangesAsync();
            }
        }

        public async Task Add(UserEvent userEvent)
        {
            db.UserEventRecords.Add(userEvent);
            await db.SaveChangesAsync();
        }

        public async Task Update(UserEvent userEvent)
        {
            db.UserEventRecords.Update(userEvent);
            await db.SaveChangesAsync();
        }

        public async Task Remove(UserEvent userEvent) 
        {
            db.UserEventRecords.Remove(userEvent);
            await db.SaveChangesAsync();

            WorkingDay workingDay = await workingDayRepository.GetWorkingDayByID(userEvent.WorkingDayID);
            await workingDayRepository.UpdateTimes(workingDay);
            await workingDayRepository.CheckOffensesInRange(workingDay.UserID, workingDay.Date, workingDay.Date);
        }

        public async Task<List<WorkingDay>> GetMonthlyUserWorkingDays(string userID, DateTime date)
         {
             return await db.WorkingDays
                 .Where(record => record.UserID == userID && record.Date.Month == date.Month && record.Date.Year == date.Year)
                 .Include(record => record.Events)
                 .Include(record => record.Offenses)
                 .OrderBy(record => record.Date)
                 .ToListAsync();
         }
        
        public async Task<List<WorkingDay>> GetCompanyMonthlyUserWorkingDays(string userID, DateTime date)
        {
            DateTime monthStart = date.GetMonthStartDate();
            DateTime monthEnd = date.GetMonthEndDate();
            return await db.WorkingDays
                .Where(record => record.UserID == userID && record.Date >= monthStart && record.Date <= monthEnd)
                .Include(record => record.Events)
                .Include(record => record.Offenses)
                .OrderBy(record => record.Date)
                .ToListAsync();
        }

        public async Task<List<TimeLineDayViewModel>> GetMonthlyUserTimeline(string userID, DateTime date, bool isHome)
        {
            bool redundant = false;
            DateTime monthStart = date.GetMonthStartDate();
            DateTime monthEnd = date.GetMonthEndDate();
            List <TimeLineDayViewModel> allDays = new List<TimeLineDayViewModel>(); 
            // loop thru work days 
            List<WorkingDay> workingDaysList = isHome ? await GetMonthlyUserWorkingDays(userID, date) : await GetCompanyMonthlyUserWorkingDays(userID, date);
            foreach(WorkingDay workingDay in workingDaysList)
            {
                TimeLineDayViewModel addDay = new TimeLineDayViewModel
                {
                    UserID = workingDay.UserID,
                    Date = workingDay.Date,
                    CheckedIn = workingDay.CheckIn,
                    CheckedOut = workingDay.CheckOut,
                    WorkedTime = workingDay.WorkingTime,
                    Events = workingDay.Events,
                    IsOnVacation = false,
                    VacationType = "",
                    IsAbsent = workingDay.Offenses.Any(o => o.Degree == Models.Enums.OffenseDegree.Absence)
                };
                //check if the day is already added 
                foreach(TimeLineDayViewModel day in allDays)
                {
                    if (day.Date == addDay.Date)
                        redundant = true;
                }

                if(!redundant)
                    allDays.Add(addDay);
            }

            List<Vacation> vacationsList = await vacationRepository.GetMonthlyUserVacation(userID, date);
            foreach(Vacation vacation in vacationsList) 
            {
                // loop over each vacation duration. Loop over the current month only
                // DateTime vacationStart = vacation.StartDate < monthStart? monthStart : vacation.StartDate;
                // DateTime vacationEnd = vacation.EndDate> monthEnd? monthEnd: vacation.EndDate; 
                for (DateTime vacationDay = vacation.StartDate;
                    vacationDay.Date <= vacation.EndDate;
                    vacationDay = vacationDay.AddDays(1))
                {
                    TimeLineDayViewModel day = allDays.FirstOrDefault(d => d.Date == vacationDay.Date);
                    if (day != null)
                    {
                        day.VacationType = "Half Day Vacation";
                        day.IsOnVacation = true;
                    }
                    // make sure the day is not a weekend
                    else if (vacationDay.Date >= monthStart && vacationDay.Date <= monthEnd &&
                             vacationDay.DayOfWeek != DayOfWeek.Friday && vacationDay.DayOfWeek != DayOfWeek.Saturday)
                    {
                        allDays.Add(new TimeLineDayViewModel
                        {
                            UserID = vacation.UserID,
                            Date = vacationDay.Date,
                            CheckedIn = null,
                            CheckedOut = null,
                            WorkedTime = null,
                            Events = null,
                            IsOnVacation = true,
                            VacationType = vacation.VacationType + " Vacation"
                        });
                    }
                }
            }
            return allDays.OrderBy(d => d.Date).ToList(); 
        }


        public async Task<List<WorkingDay>> GetMonthlyUserTimeline(string userID, DateTime startDate, DateTime endDate)
        {
            return await db.WorkingDays
                .Where(record => record.UserID == userID && record.Date >= startDate && record.Date <= endDate)
                .Include(record => record.Events)
                .OrderBy(record => record.Date)
                .ToListAsync();
        }

        public async Task<UserEvent> GetEventByID(string eventID)
        {
            return await db.UserEventRecords
                .Where(record => record.ID == eventID)
                .Include(record => record.WorkingDay.Events)
                .FirstOrDefaultAsync();
        }

        public async Task<UserEvent> GetLastEvent(string userID)
        {
            return await db.UserEventRecords
                .Where(record => record.WorkingDay.UserID == userID)
                .OrderByDescending(record => record.Time)
                .FirstOrDefaultAsync();
        }

        public async Task<UserEvent> GetLastEventInDate(string userID, DateTime date)
        {
            return await db.UserEventRecords
                .Where(record => record.WorkingDay.UserID == userID && record.Time.Date == date.Date)
                .OrderByDescending(record => record.Time)
                .FirstOrDefaultAsync();
        }
    }
}
