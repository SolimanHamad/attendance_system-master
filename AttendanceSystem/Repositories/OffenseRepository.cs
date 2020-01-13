using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Extensions;
using AttendanceSystem.Models;
using AttendanceSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class OffenseRepository
    {
        private readonly AttendanceSystemContext db;

        public OffenseRepository(AttendanceSystemContext db)
        {
            this.db = db;
        }

        public async Task Add(Offense offense)
        {
            db.Offenses.Add(offense);
            await db.SaveChangesAsync();
        }

        public async Task<List<Offense>> GetUserOffenses(string userID, DateTime startDate, DateTime endDate)
        {
            return await db.Offenses
                .Where(record => record.WorkingDay.UserID == userID && record.WorkingDay.Date >= startDate && record.WorkingDay.Date <= endDate)
                .OrderBy(record => record.WorkingDay.Date)
                .Include(record => record.WorkingDay.User)
                .ToListAsync();
        }

        public async Task<List<Offense>> GetNeedApprovalOffenses()
        {
            return await db.Offenses
                .Where(record => record.NeedApproval)
                .OrderByDescending(record => record.WorkingDay.Date)
                .Include(record => record.WorkingDay.User)
                .ToListAsync();
        }

        public async Task<Offense> GetOffenseByID(string offenseID)
        {
            return await db.Offenses
                .Where(record => record.ID == offenseID)
                .Include(record => record.WorkingDay)
                .FirstOrDefaultAsync();
        }

        public async Task Update(Offense offense)
        {
            db.Offenses.Update(offense);
            await db.SaveChangesAsync();
        }

        public async Task RemoveAll(IEnumerable<Offense> offenses)
        {
            db.Offenses.RemoveRange(offenses);
            await db.SaveChangesAsync();
        }

        public async Task RemoveInRange(string userID, DateTime startDate, DateTime endDate)
        {
            List<Offense> offensesList = await db.Offenses.Where(record => record.WorkingDay.UserID == userID 
                && record.WorkingDay.Date >= startDate.Date && record.WorkingDay.Date <= endDate.Date)
                .ToListAsync();

            if (offensesList != null && offensesList.Count > 0)
            {
                db.Offenses.RemoveRange(offensesList);
                await db.SaveChangesAsync();
            }
        }

        public async Task<int> CountOffensesInLast90Days(string userID, WorkingDay workingDay, OffenseDegree degree)
        {
            return await db.Offenses
                .CountAsync(record => record.WorkingDay.UserID == userID && record.WorkingDay.Date >= workingDay.Date.AddDays(-90) && record.WorkingDay.Date < workingDay.Date && record.Degree == degree && record.HasAllowance == false);
        }

        public async Task<int> CountAbsenceInLast90Days(string userID, WorkingDay workingDay)
        {
            return await db.Offenses
                .CountAsync(record => record.WorkingDay.UserID == userID && record.WorkingDay.Date >= workingDay.Date.AddDays(-90) && record.WorkingDay.Date < workingDay.Date && record.Degree == OffenseDegree.Absence && record.PenaltyPercent > 0);
        }

        public async Task<int> CountOfLates(string userID, DateTime start, DateTime end)
        {
            return await db.Offenses
                .CountAsync(record => record.WorkingDay.UserID == userID && record.WorkingDay.Date >= start && record.WorkingDay.Date <= end && record.Degree != OffenseDegree.LeaveEarly && record.Degree != OffenseDegree.Absence);
        }

        public async Task<int> CountAllowancesInThisMonth(string userID, DateTime endDate, OffenseDegree degree)
        {
            DateTime startDate = endDate.GetMonthStartDate();
            return await db.Offenses.CountAsync(record => record.HasAllowance && record.WorkingDay.UserID == userID && record.Degree == degree && record.WorkingDay.Date < endDate && record.WorkingDay.Date >= startDate);
        }

        public async Task<int> CountOfAbsents(string userID, DateTime start, DateTime end)
        {
            return await db.Offenses
                .CountAsync(record => record.WorkingDay.UserID == userID && record.WorkingDay.Date >= start && record.WorkingDay.Date <= end && record.Degree == OffenseDegree.Absence);
        }

        public async Task<int> CountOfMakeUps(string userID, DateTime start, DateTime end)
        {
            return await db.Offenses
                .CountAsync(record => record.WorkingDay.UserID == userID && record.WorkingDay.Date >= start && record.WorkingDay.Date <= end && (record.Degree == OffenseDegree.LateLessThanHourWithMakeUp || record.Degree == OffenseDegree.LateMoreThanHourWithMakeUp));
        }

        public async Task<int> SumOfPenaltyPercent(string userID, DateTime start, DateTime end)
        {
            return await db.Offenses
                .Where(record => record.WorkingDay.UserID == userID && record.WorkingDay.Date >= start && record.WorkingDay.Date <= end && record.HasAllowance == false).Select(record => record.PenaltyPercent).SumAsync();
        }
    }
}
