using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Extensions;
using AttendanceSystem.Models;
using AttendanceSystem.Models.Enums;
using AttendanceSystem.Utilities;
using AttendanceSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Repositories
{
    public class VacationRepository
    {
        private readonly AttendanceSystemContext context;

        public VacationRepository(AttendanceSystemContext context)
        {
            this.context = context;
        }

        public async Task<Vacation> GetByID(string id)
        {
            return await context.Vacations
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.ID == id);
        }

        public async Task Add(Vacation vacation)
        {
            context.Vacations.Add(vacation);
            await context.SaveChangesAsync();
        }

        public async Task<List<VacationViewModel>> GetNotRejectedUserVacation(string userId)
        {
            return await context.Vacations
                .Include(v => v.User)
                .Where(v => v.UserID == userId && v.Status != RequestStatus.Rejected)
                .Select(v => new VacationViewModel(v))
                .ToListAsync();
        }

        public async Task<double> GetUnpaidUserVacationsInRange(string userId, DateTime start , DateTime end)
        {
            return await context.Vacations
                .Include(v => v.User)
                .Where(v => v.UserID == userId && v.StartDate >= start && v.StartDate <= end && 
                v.VacationType == VacationType.Unpaid && v.Status == RequestStatus.Approved)
                .Select(v => new VacationViewModel(v))
                .SumAsync(v => v.TotalDays);
        }

        public async Task<List<Vacation>> GetMonthlyUserVacation(string userId, DateTime date)
        {
            DateTime monthStart = date.GetMonthStartDate();
            DateTime monthEnd = date.GetMonthEndDate();
            return await context.Vacations
            .Include(v => v.User)
            .Where(v => v.UserID == userId && v.Status == RequestStatus.Approved &&
                        v.StartDate <= monthEnd && v.EndDate >= monthStart)
            .ToListAsync();
        }

        public async Task<List<VacationViewModel>> GetUserVacationByYear(string userId, int year)
        {
            return await context.Vacations
                .Include(v => v.User)
                .Where(v => v.UserID == userId && v.CreatedAt.Year == year)
                .OrderByDescending(v => v.StartDate)
                .Select(v => new VacationViewModel(v))
                .ToListAsync();
        }

        public async Task<double> GetUserTotalPersonalVacationDaysByYear(string userId, int year)
        {
            return await context.Vacations
                .Include(v => v.User)
                .Where(v => v.UserID == userId && v.CreatedAt.Year == year && v.VacationType == VacationType.Personal)
                .Select(v => new VacationViewModel()
                {
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                })
                .SumAsync(v => v.TotalDays);
        }

        public async Task Delete(string id)
        {
            Vacation vacation = await context.Vacations.FindAsync(id);
            if (vacation != null)
            {
                context.Vacations.Remove(vacation);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Vacation>> GetAllVacationSorted
            (string name = null,
            DateTime? issueDate = null, 
            DateTime? startDate = null,
            DateTime? endDate = null, 
            double totalDays = 0,
            VacationType? vacationType = null, 
            RequestStatus? requestStatus = null, 
            string sortBy = null) 
        {
            // Search 
            IQueryable<Vacation> vacation = context.Vacations.AsQueryable();

            if (!name.IsNullOrEmpty())
            {
                vacation = vacation.Where(v => v.User.FullName.ToLower().Contains(name.ToLower()));
            }
            if (vacationType.HasValue)
            {
                vacation = vacation.Where(v => v.VacationType == vacationType);
            }
            if (requestStatus.HasValue)
            {
                vacation = vacation.Where(v => v.Status == requestStatus);
            }
            if (issueDate.HasValue)
            {
                vacation = vacation.Where(v => v.CreatedAt.Date == issueDate.Value);
            }
            if (startDate.HasValue && !endDate.HasValue)
            {
                vacation = vacation.Where(v => v.StartDate.Date == startDate.Value);
            }
            if (endDate.HasValue && !startDate.HasValue)
            {
                vacation = vacation.Where(v => v.EndDate.Date == endDate.Value);
            }
            if (startDate.HasValue && endDate.HasValue)
            {
                vacation = vacation.Where(v => v.StartDate.Date <= endDate.Value && v.EndDate.Date >= startDate.Value);
            }
            if (totalDays > 0)
            {
                vacation = vacation.Where( v => VacationUtilities.GetWorkingDays(v.StartDate, v.EndDate) == totalDays);
            }

            // Sort
            switch (sortBy)
            {
                case "Name_desc":
                    vacation = vacation.OrderByDescending(v => v.User.FullName);
                    break;
                case "Name":
                    vacation = vacation.OrderBy(v => v.User.FullName);
                    break;
                case "TotalDays_desc":
                    vacation = vacation.OrderByDescending(v => VacationUtilities.GetWorkingDays(v.StartDate, v.EndDate));
                    break;
                case "TotalDays":
                    vacation = vacation.OrderBy(v => VacationUtilities.GetWorkingDays(v.StartDate, v.EndDate));
                    break;
                case "StartDate_desc":
                    vacation = vacation.OrderByDescending(v => v.StartDate);
                    break;
                case "StartDate":
                    vacation = vacation.OrderBy(v => v.StartDate);
                    break;
                case "EndDate_desc":
                    vacation = vacation.OrderByDescending(v => v.EndDate);
                    break;
                case "EndDate":
                    vacation = vacation.OrderBy(v => v.EndDate);
                    break;
                case "IssueDate_desc":
                    vacation = vacation.OrderByDescending(v => v.CreatedAt);
                    break;
                case "IssueDate":
                    vacation = vacation.OrderBy(v => v.CreatedAt);
                    break;
                case "RequestStatus_desc":
                    vacation = vacation.OrderByDescending(v => v.Status);
                    break;
                case "RequestStatus":
                    vacation = vacation.OrderBy(v => v.Status);
                    break;
                case "VacationType_desc":
                    vacation = vacation.OrderByDescending(v => v.VacationType);
                    break;
                case "VacationType":
                    vacation = vacation.OrderBy(v => v.VacationType);
                    break;
                case "comment_desc":
                    vacation = vacation.OrderByDescending(v => v.Comment);
                    break;
                case "comment":
                    vacation = vacation.OrderBy(v => v.Comment);
                    break;
                default:
                    vacation = vacation.OrderBy(v => v.Status);
                    break;
            }
            return await vacation.Include(v => v.User).ToListAsync();
        }

        public async Task Update(Vacation vacation)
        {
            context.Vacations.Update(vacation);
            await context.SaveChangesAsync();
        }

        public async Task AddVacationToAllEmployees(VacationViewModel model, List<ApplicationUser> users)
        {
            foreach (ApplicationUser user in users)
            {
                context.Vacations.Add(new Vacation
                {
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Comment = model.Comment,
                    Status = RequestStatus.Approved,
                    VacationType = VacationType.General,
                    UserID = user.Id
                });
            }
            await context.SaveChangesAsync();
        }

        public async Task<List<HeatMapViewModel>> GetExcusedVacationCountsPerDay()
        {
            var vacationCounts = await context.Vacations
                .Include(v => v.User)
                .Where(v => v.StartDate == v.EndDate && v.VacationType == VacationType.Excused)
                .GroupBy(v => new { v.StartDate.DayOfWeek, v.User })
                .ToListAsync();

            List<HeatMapViewModel> heatMap = new List<HeatMapViewModel>();
            foreach (var item in vacationCounts)
            {
                HeatMapViewModel record = heatMap.Find(r => r.User == item.Key.User) ?? new HeatMapViewModel { User = item.Key.User };

                switch (item.Key.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        record.SundayCount = item.Count();
                        break;
                    case DayOfWeek.Monday:
                        record.MondayCount = item.Count();
                        break;
                    case DayOfWeek.Tuesday:
                        record.TuesdayCount = item.Count();
                        break;
                    case DayOfWeek.Wednesday:
                        record.WednesdayCount = item.Count();
                        break;
                    case DayOfWeek.Thursday:
                        record.ThursdayCount = item.Count();
                        break;
                }

                if(!heatMap.Contains(record))
                    heatMap.Add(record);
            }
            return heatMap;
        }

        public async Task<List<HeatMapViewModel>> GetExcusedVacationCountsPerDayAsPercentages()
        {
            List<HeatMapViewModel> heatMap = await GetExcusedVacationCountsPerDay();
            foreach (HeatMapViewModel item in heatMap)
            {
                double total = item.SundayCount + item.MondayCount + item.TuesdayCount + item.WednesdayCount + item.ThursdayCount;
                item.SundayCount = Math.Truncate((item.SundayCount / total) * 10000) / 100;
                item.MondayCount = Math.Truncate((item.MondayCount / total) * 10000) / 100;
                item.TuesdayCount = Math.Truncate((item.TuesdayCount / total) * 10000) / 100;
                item.WednesdayCount = Math.Truncate((item.WednesdayCount / total) * 10000) / 100;
                item.ThursdayCount = Math.Truncate((item.ThursdayCount / total) * 10000) / 100;
                item.TotalCount = total;
            }
            return heatMap;
        }
    }
}
