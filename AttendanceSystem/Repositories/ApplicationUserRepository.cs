using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class ApplicationUserRepository
    {
        private readonly AttendanceSystemContext context;

        public ApplicationUserRepository(AttendanceSystemContext context)
        {
            this.context = context;
        }

        public async Task<ApplicationUser> GetByID(string id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<ApplicationUser> GetUserWithVacations(string id)
        {
            return await context.Users.Include(u => u.Vacations).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task Delete(string id)
        {
            ApplicationUser user = await context.Users.FindAsync(id);
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
        }

        public async Task Disable(string id)
        {
            ApplicationUser user = await context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsEnabled = false;
                await context.SaveChangesAsync();
            }
        }

        public async Task Enable(string id)
        {
            ApplicationUser user = await context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsEnabled = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<ApplicationUser>> GetAllUsers()
        {
            return await context.Users
                .Include(record => record.Vacations)
                .OrderBy(record => record.DateJoined)
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetActiveUsers()
        {
            return await context.Users
                .Where(record => record.IsEnabled)
                .Include(record => record.Vacations)
                .OrderBy(record => record.DateJoined)
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetUsersWithVacations()
        {
           return await context.Users
                .OrderBy(record => record.DateJoined)
                .Include(record => record.Vacations)
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetPureUsers()
        {
            return await context.Users
                .OrderBy(record => record.DateJoined)
                .ToListAsync();
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }

        public async Task<int> GetDailySalary(string userID)
        {
            double salary = await context.Users.Where(record => record.Id == userID).Select(record => record.Salary).FirstOrDefaultAsync();
            int dailySalary = (int)(salary/30.0);
            return dailySalary;
        }
        
        public async Task IncrementYearlyVacationForAllUsers()
        {
            List<ApplicationUser> allUsers = await GetActiveUsers();
            foreach (ApplicationUser user in allUsers)
            {
                user.VacationBalance += user.YearlyIncrement;
                await context.SaveChangesAsync();
            }
        }
    }
}
