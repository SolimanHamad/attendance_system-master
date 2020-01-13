using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class LoanRepository
    {
        private readonly AttendanceSystemContext context;

        public LoanRepository(AttendanceSystemContext context)
        {
            this.context = context;
        }

        public async Task Add(Loan loan)
        {
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            Loan loan = await context.Loans.FindAsync(id);
            if (loan != null)
            {
                context.Loans.Remove(loan);
                await context.SaveChangesAsync();
            }
        }

        public async Task Update(Loan loan)
        {
            context.Loans.Update(loan);
            await context.SaveChangesAsync();
        }

        public async Task<bool> UserHasActiveLoan(string userId)
        {
            return await context.Loans
                .Include(l => l.User)
                .Where(l => l.UserID == userId && l.RemainingAmount > 0)
                .AnyAsync();
        }

        public async Task<Loan> GetLoanById(string id)
        {
            return await context.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<bool> LoanIsInactiveAsync(string id)
        {
            return (await context.Loans.FindAsync(id)).RemainingAmount == 0;
        }

        public async Task<Loan> GetActiveLoanByUserId(string userId)
        {
            return await context.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.UserID == userId && l.RemainingAmount > 0);
        }
    }
}
