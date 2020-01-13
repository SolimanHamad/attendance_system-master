using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class RuleRepository
    {
        private readonly AttendanceSystemContext db;
        public RuleRepository(AttendanceSystemContext db)
        {
            this.db = db;
        }

        public int GetPenaltyPercent(OffenseDegree degree, Occurrence occurrence)
        {
            return db.Rules.Where(record => record.OffenseDegree == degree && record.Occurrence == occurrence).Select(record => record.PenaltyPercent).FirstOrDefault();
        }

        public async Task<List<Rule>> GetAllRules()
        {
            return await db.Rules
                .OrderBy(record => record.OffenseDegree).ThenBy(record => record.Occurrence)
                .ToListAsync();
        }

        public async Task AddRules(List<Rule> rules)
        {
            db.Rules.UpdateRange(rules);
            await db.SaveChangesAsync();
        }
    }
}
