using AttendanceSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Data
{
    public class AttendanceSystemContext : IdentityDbContext
    {
        public AttendanceSystemContext(DbContextOptions<AttendanceSystemContext> options)
            : base(options)
        {            
        }

        public new DbSet<ApplicationUser> Users { get; set; }
        public DbSet<UserEvent> UserEventRecords { get; set; }
        public DbSet<EditEventRequest> EditEventRequests { get; set; }
        public DbSet<MissedEventRequest> MissedEventRequests { get; set; }
        public DbSet<Vacation> Vacations { get; set; }
        public DbSet<WorkingDay> WorkingDays { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<Offense> Offenses { get; set; }
        public DbSet<MonthlyAllowanceRule> MonthlyAllowanceRules { get; set; }
        public DbSet<SalarySheet> SalarySheets { get; set; }
        public DbSet<Loan> Loans { get; set; }

    }
}
