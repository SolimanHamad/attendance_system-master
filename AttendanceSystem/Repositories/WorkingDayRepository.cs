using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Jobs;
using AttendanceSystem.Models;
using AttendanceSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class WorkingDayRepository
    {
        private readonly AttendanceSystemContext db;
        private readonly OffenseRepository offenseRepository;
        
        public WorkingDayRepository(AttendanceSystemContext context)
        {
            db = context;
            offenseRepository = new OffenseRepository(db);
        }

        public async Task Add(WorkingDay workingDay)
        {
            db.WorkingDays.Add(workingDay);
            await db.SaveChangesAsync();
        }

        public async Task AddComment(string userID, string comment)
        {
            await Add(new WorkingDay
            {
                Comment = comment,
                UserID = userID,
                Events = new List<UserEvent> { new UserEvent { Event = Event.CheckedIn } },
                CheckIn = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0)
            });
        }

        public async Task<WorkingDay> GetWorkingDayByID(string workingDayID)
        {
            return await db.WorkingDays
                .Where(record => record.ID == workingDayID)
                .Include(record => record.Events)
                .Include(record => record.Offenses)
                .Include(record => record.User)
                .FirstOrDefaultAsync();
        }

        public async Task<WorkingDay> GetWorkingDayByUserID(string userId)
        {
            return await db.WorkingDays.FirstOrDefaultAsync(record => record.UserID == userId && record.Date == DateTime.Today);
        }

        public async Task<WorkingDay> GetCurrentWorkingDay(string userID)
        {
            return await db.WorkingDays
                .Where(record => record.UserID == userID && record.Date == DateTime.Today)
                .Include(record => record.Events)
                .Include(record => record.Offenses)
                .Include(record => record.User)
                .FirstOrDefaultAsync();
        }

        public async Task<WorkingDay> GetYesterdaysWorkingDay(string userID)
        {
            return await db.WorkingDays
                .Where(record => record.UserID == userID && record.Date == DateTime.Today.AddDays(-1))
                .Include(record => record.Events)
                .Include(record => record.Offenses)                
                .Include(record => record.User)
                .FirstOrDefaultAsync();
        }

        public async Task<WorkingDay> GetWorkingDayByDate(string userID, DateTime date)
        {
            return await db.WorkingDays
                .Where(record => record.UserID == userID && record.Date.Date == date.Date)
                .Include(record => record.Events)
                .Include(record => record.Offenses)
                .Include(record => record.User)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateTimes(WorkingDay workingDay)
        {
            // Update check in and check out time for the working day
            UpdateCheckInCheckOutTime(workingDay);
            // Calculate & set the total working time of the working day
            workingDay.SetWorkingTime();
            // Update workingDay object in the database
            db.Update(workingDay);
            await db.SaveChangesAsync();
        }

        private static void UpdateCheckInCheckOutTime(WorkingDay workingDay)
        {
            List<UserEvent> orderedEvents = workingDay.Events?.OrderBy(record => record.Time).ToList();
            UserEvent firstCheckIn = orderedEvents?
                .Where(record => record.Event == Event.CheckedIn)
                .FirstOrDefault();
            UserEvent lastCheckOut = orderedEvents?
                .Where(record => record.Event == Event.CheckedOut)
                .LastOrDefault();

            if (firstCheckIn != null)
                workingDay.CheckIn = firstCheckIn.Time;
            else
                workingDay.CheckIn = null;

            if (lastCheckOut != null)
                workingDay.CheckOut = lastCheckOut.Time;
            else
                workingDay.CheckOut = null;
        }

        private async Task<List<WorkingDay>> GetWorkingDaysInRange(string userID, DateTime startDate, DateTime endDate)
        {
            return await db.WorkingDays
                .Where(record => record.UserID == userID && record.Date >= startDate.Date && record.Date <= endDate.Date)
                .OrderBy(record => record.Date)
                .Include(record => record.User).ThenInclude(u => u.Vacations)
                .Include(record => record.Events)
                .Include(record => record.Offenses)
                .ToListAsync();
        }

        public async Task CheckOffensesInRange(string userID, DateTime startDate, DateTime endDate)
        {
            List<WorkingDay> workingDays = await GetWorkingDaysInRange(userID, startDate, endDate);
            DailyJob offensesChecker = new DailyJob(db);

            foreach (WorkingDay workingDay in workingDays)
            {
                if (workingDay.IsAbsent())
                {
                    if (workingDay.Offenses?.Any() ?? false)
                        await offenseRepository.RemoveAll(workingDay.Offenses);
                    
                    // Check if is on vacation
                    if (!workingDay.User.GetCurrentVacationType(workingDay.Date).HasValue)
                        await offensesChecker.RecordDetectedOffense(workingDay.User, workingDay, OffenseDegree.Absence);
                }
                else
                    await offensesChecker.CheckWorkingDayOffenses(workingDay);
            }
        }

        public async Task CheckAbsencesInRange(string userID, DateTime startDate, DateTime endDate)
        {
            List<WorkingDay> workingDays = await GetWorkingDaysInRange(userID, startDate, endDate);
            DailyJob offensesChecker = new DailyJob(db);

            foreach (WorkingDay workingDay in workingDays.Where(workingDay => workingDay.IsAbsent()))
            {
                if (workingDay.Offenses?.Any() ?? false)
                    await offenseRepository.RemoveAll(workingDay.Offenses);
                await offensesChecker.RecordDetectedOffense(workingDay.User, workingDay, OffenseDegree.Absence);
            }
        }

        public async Task DeleteEmptyDays()
        {
            List<WorkingDay> emptyDays = await db.WorkingDays.Where(wd => wd.Offenses.Count == 0 && wd.Events.Count == 0).ToListAsync();
            if (emptyDays.Count > 0)
            {
                db.WorkingDays.RemoveRange(emptyDays);
                await db.SaveChangesAsync();    
            }
        }
    }
}
