using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AttendanceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AttendanceSystem.Data;
using AttendanceSystem.Repositories;
using AttendanceSystem.ViewModels;

namespace AttendanceSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationUserRepository appUser;
        private readonly VacationRepository vacationRepositry;
        private readonly WorkingDayRepository workingDayRepository;
        private readonly UserEventRepository userEventRepository;
        private readonly MissedEventRequestRepository missedEventRequestRepository;
        public HomeController(AttendanceSystemContext context)
        {
            appUser = new ApplicationUserRepository(context);
            vacationRepositry = new VacationRepository(context);
            userEventRepository = new UserEventRepository(context);
            workingDayRepository = new WorkingDayRepository(context);
            missedEventRequestRepository = new MissedEventRequestRepository(context);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string userID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserEvent lastEvent = await userEventRepository.GetLastEvent(userID) ?? new UserEvent {Event = Event.CheckedOut};
            ApplicationUser user = await appUser.GetUserWithVacations(userID);

            return View(new HomeViewModel
            {
                Countdown = lastEvent.Event == Event.CheckedIn ? await CountDown(lastEvent.Time, userID) : "",
                LastEventTime = UserEvent.LastEventString(lastEvent),
                OnVacation = user.IsOnVacation(DateTime.Now),
                NotToday = lastEvent.Time.Date != DateTime.Today,
                MissedEventAdded = await missedEventRequestRepository.GetMissedEventRequestOnDayForUser(userID, lastEvent.Time.Date),
                CheckedIn = lastEvent.Event == Event.CheckedIn,
                TimeLine = await userEventRepository.GetMonthlyUserTimeline(userID, DateTime.Now, true),
                UserIsLate = await IsLate() && lastEvent.Time.Date != DateTime.Now.Date
            }) ;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<string> CountDown(DateTime checkInTime, string userID)
        {
            ApplicationUser user = await appUser.GetUserWithVacations(userID);
            TimeSpan requiredWorkTime = user.WorkEndTime - user.WorkStartTime;
            
            // Account for half days
            if (user.IsOnHalfDayVacation(DateTime.Now))
                requiredWorkTime = requiredWorkTime.Divide(2.0);
            
            // Account for coming within grace period
            else if (checkInTime.TimeOfDay > user.WorkStartTime && checkInTime.TimeOfDay <= user.WorkStartTime.Add(user.GracePeriod))
                requiredWorkTime = user.WorkEndTime - checkInTime.TimeOfDay;
            
            // Account for time worked before
            WorkingDay workDay = await workingDayRepository.GetWorkingDayByUserID(userID);
            if(workDay?.WorkingTime.HasValue ?? false)
                requiredWorkTime -= workDay.WorkingTime.Value;
            
            // Remove time worked since last check in
            requiredWorkTime -= DateTime.Now.TimeOfDay - checkInTime.TimeOfDay;
            return requiredWorkTime.Ticks > 0 ? $"{requiredWorkTime:hh\\:mm\\:ss}" : "-1";
        }

        public async Task<bool> IsLate()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser user = await appUser.GetByID(id);
            return DateTime.Now.TimeOfDay > user.WorkStartTime.Add(user.GracePeriod);
        }

        //public async Task<string> WhatTypeOfVaction()
        //{
        //    string userID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    return await vacationRepositry.GetVacationTypeByWorkingDay(userID, DateTime.Now);
        //}


        public async Task<IActionResult> Reasoning(string comment)
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            await workingDayRepository.AddComment(id, comment);
            return RedirectToAction(nameof(Index));
        }
    }
}
