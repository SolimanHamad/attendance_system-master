using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class SalarySheetRepository
    {
        private readonly AttendanceSystemContext db;

        public SalarySheetRepository(AttendanceSystemContext context)
        {
            db = context;
        }

        public async Task UpdateList(List<SalarySheet> recordList)
        {
            db.UpdateRange(recordList);
            await db.SaveChangesAsync();
        }

        public async Task<List<SalarySheet>> GetSalarySheet(DateTime endDate)
        {
            return await db.SalarySheets
                .Where(record => record.Date == endDate)
                .Include(record => record.User)
                .OrderBy(record => record.User.DateJoined)
                .ToListAsync();
        }

        public async Task<List<SalarySheetRecord>> GetSalarySheetRecords(DateTime endDate, bool editable)
        {
            return await db.SalarySheets
                .Where(record => record.Date == endDate)
                .Include(record => record.User)
                .OrderBy(record => record.User.DateJoined)
                .Select(record => new SalarySheetRecord(record, editable))
                .ToListAsync();
        }
    }
}
