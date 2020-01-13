using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Data.QueryFilter;
using AttendanceSystem.Models;
using AttendanceSystem.Models.Enums;
using AttendanceSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Controllers
{
    [Route("api/")]
    [ApiController]
    public class MiscellaneousController : ControllerBase
    {
        private readonly AttendanceSystemContext context;

        public MiscellaneousController(AttendanceSystemContext context)
        {
            this.context = context;
        }

        // GET: api/notification/
        [HttpGet("notification")]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<object> GetNotificationAsync()
        {
            return new
            {
                VacationsRequests = await context.Vacations.CountAsync(v => v.Status == RequestStatus.Pending),
                MissedEvents = await context.MissedEventRequests.CountAsync(e => e.Approval == Approval.Pending),
                EditEvents = await context.EditEventRequests.CountAsync(e => e.Approval == Approval.Pending),
                OffensesApproval = await context.Offenses.CountAsync(e => e.NeedApproval)
            };
        }

        [HttpGet("vacations")]
        public async Task<IActionResult> GetVacations(QueryFilter filter)
        {
            return Ok(await (await filter.ApplyFilter(context.Vacations, HttpContext))
                .Include(v => v.User)
                .Select(v => new VacationViewModel(v))
                .ToListAsync());
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(QueryFilter filter)
        {
            return Ok(await (await filter.ApplyFilter(context.Users, HttpContext)).ToListAsync());
        }

        [HttpGet("loans")]
        public async Task<IActionResult> GetLoans(QueryFilter filter)
        {
            return Ok(await (await filter.ApplyFilter(context.Loans, HttpContext))
                .Include(l => l.User)
                .Select(l => new LoanViewModel(l))
                .ToListAsync());
        }
    }
}