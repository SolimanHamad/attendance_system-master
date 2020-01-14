using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Extensions;
using AttendanceSystem.Repositories;

namespace AttendanceSystem.Controllers
{
    [Authorize]
    public class UserEventController : Controller
    {
        private readonly UserEventRepository userEventRepository;
        private readonly WorkingDayRepository workingDayRepository;
        private readonly OffenseRepository offenseRepository;
        private readonly ApplicationUserRepository applicationUserRepository;

        public UserEventController(AttendanceSystemContext context)
        {
            userEventRepository = new UserEventRepository(context);
            applicationUserRepository = new ApplicationUserRepository(context);
            offenseRepository = new OffenseRepository(context);
            workingDayRepository = new WorkingDayRepository(context);
        }

        [HttpGet]
        public async Task<IActionResult> AddEvent(Event newEvent)
        {
            string userID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            WorkingDay workingDay = await workingDayRepository.GetCurrentWorkingDay(userID);

            if (workingDay == null)
            {
                workingDay = new WorkingDay()
                {
                    UserID = userID
                };
                await workingDayRepository.Add(workingDay);
            }

            UserEvent lastEvent = workingDay.Events?.OrderByDescending(e => e.Time).FirstOrDefault();
            if (lastEvent != null && DateTime.Now.Minute == lastEvent.Time.Minute && DateTime.Now.Subtract(lastEvent.Time).TotalSeconds < 60)
            {
                TempData["StatusMessage"] = "Multiple events can't be recorded in the same minute.";
            }
            else
            {
                await userEventRepository.AddIfValid(userID,
                    new UserEvent
                    {
                        WorkingDayID = workingDay.ID,
                        Event = newEvent
                    });
                await workingDayRepository.UpdateTimes(workingDay);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string eventID)
        {
            UserEvent userEvent = await userEventRepository.GetEventByID(eventID);
            await userEventRepository.Remove(userEvent);

            long dayTicks = userEvent.WorkingDay.Date.Ticks;
            TempData["StatusMessage"] = "Event has been deleted successfully!";

            return await ViewDayEvents(dayTicks, userEvent.WorkingDay.UserID);
        }

        [HttpGet] 
        [NonAction] // This action can not be accessed directly from the URL
        private async Task<IActionResult> ViewTimeline(string id, DateTime s, DateTime e, int n = 0)
        {
            DateTime startDate = s.AddMonths(n);
            DateTime endDate = e.AddMonths(n);
            int dailySalary = await applicationUserRepository.GetDailySalary(id);
            ViewBag.UserID = id;
            ViewBag.Name = (await applicationUserRepository.GetByID(id)).FullName;
            ViewBag.Next = n + 1;
            ViewBag.Previous = n - 1;
            ViewBag.StartDate = startDate.ToString("dd-MM-yyyy");
            ViewBag.EndDate = endDate.ToString("dd-MM-yyyy");
            ViewBag.Absents = await offenseRepository.CountOfAbsents(id, startDate, endDate);
            ViewBag.Lates = await offenseRepository.CountOfLates(id, startDate, endDate);
            ViewBag.MakeUps = await offenseRepository.CountOfMakeUps(id, startDate, endDate);
            ViewBag.TotalPenaltyPercent = await offenseRepository.SumOfPenaltyPercent(id, startDate, endDate);
            ViewBag.PenaltyAmount = (int)((dailySalary * ViewBag.TotalPenaltyPercent) / 100.0) * -1;
            IEnumerable<TimeLineDayViewModel> timeline = await userEventRepository.GetMonthlyUserTimeline(id, startDate, false);
            return View("Timeline", timeline);
        }

        [HttpGet]
        [Route("Timeline/")]
        public async Task<IActionResult> ViewMonthlyTimeline(int n = 0)
        {
            return await ViewUserTimeline(User.FindFirst(ClaimTypes.NameIdentifier).Value, n);            
        }

        [HttpGet]
        [Route("Timeline/{id}/{s}/{e}")]
        public async Task<IActionResult> ViewTimelineInRange(string id, long s, long e, int n=0)
        {
            return await ViewTimeline(id, new DateTime(s), new DateTime(e), n);
        }

        [HttpGet]
        [Route("Timeline/{id}")]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> ViewUserTimeline(string id, int n = 0)
        {
            return await ViewTimeline(id, DateTime.Today.GetMonthStartDate(), DateTime.Today.GetMonthEndDate(), n);
        }


        [HttpGet]
        public async Task<IActionResult> ViewDayEvents(long ticks, string userID)
        {
            DateTime date = new DateTime(ticks);
            ViewBag.Date = new DateTime(ticks).ToShortDateString();
            WorkingDay workingDay = await workingDayRepository.GetWorkingDayByDate(userID, date) ?? new WorkingDay();
            IEnumerable<UserEvent> dayEvents = workingDay.Events?.OrderBy(record => record.Time).ToList();
            return View("DayEvents", dayEvents);
        }

        [HttpGet]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> DailyAttendance(int n=0)
        {
            DateTime date = DateTime.Today.AddDays(n);
            ViewBag.Date = date.ToString("ddd dd, MMM yyyy");
            ViewBag.Current = n;
            IEnumerable<ApplicationUser> allUsers = await applicationUserRepository.GetUsersWithVacations();
            List<DailyAttendanceViewModel> dailyAttendance = new List<DailyAttendanceViewModel>();

            foreach(ApplicationUser user in allUsers)
            {
                UserEvent userLastEvent = await userEventRepository.GetLastEventInDate(user.Id, date);
                DailyAttendanceViewModel dailyAttendanceRecord = new DailyAttendanceViewModel();
                dailyAttendanceRecord.UserName = user.FullName;
                dailyAttendanceRecord.CheckAttendance = user.CheckAttendance;
                if (user.GetCurrentVacationType(DateTime.Today).HasValue)
                {
                    dailyAttendanceRecord.Type = user.GetCurrentVacationType(DateTime.Today).Value; 
                    dailyAttendanceRecord.LastEvent = "";
                    dailyAttendanceRecord.Status = AttendanceStatus.Vacation;
                }
                else if (userLastEvent != null)
                {
                    dailyAttendanceRecord.LastEvent = userLastEvent.TimeString();
                    dailyAttendanceRecord.Status = userLastEvent.Event == Event.CheckedIn ? AttendanceStatus.Present : AttendanceStatus.Left;
                }
                else
                {
                    dailyAttendanceRecord.LastEvent = "";
                    dailyAttendanceRecord.Status = AttendanceStatus.Missing;
                }
                dailyAttendance.Add(dailyAttendanceRecord);
            }
            return View("DailyAttendance", dailyAttendance);
        }
    }
}
