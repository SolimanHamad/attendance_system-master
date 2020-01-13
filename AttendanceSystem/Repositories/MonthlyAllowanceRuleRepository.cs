using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class MonthlyAllowanceRuleRepository
    {
        private readonly AttendanceSystemContext db;

        public MonthlyAllowanceRuleRepository(AttendanceSystemContext db)
        {
            this.db = db;
        }

        public async Task<int> GetAllowance(OffenseDegree degree)
        {
            return await db.MonthlyAllowanceRules.Where(record => record.Degree == degree).Select(record => record.UserMonthlyAllowance).FirstOrDefaultAsync();
        }

        public async Task<List<MonthlyAllowanceRule>> GetAllMonthlyAllowanceRules()
        {
            return await db.MonthlyAllowanceRules.OrderBy(record => record.Degree).ToListAsync();
        }

        public async Task AddMonthlyAllowanceRules(List<MonthlyAllowanceRule> rules)
        {
            db.MonthlyAllowanceRules.UpdateRange(rules);
            await db.SaveChangesAsync();
        }
    }
}
