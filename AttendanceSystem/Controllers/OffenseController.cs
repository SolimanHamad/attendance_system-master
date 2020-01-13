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
using AttendanceSystem.Models.Enums;
using AttendanceSystem.Repositories;

namespace AttendanceSystem.Controllers
{   
    [Authorize]
    public class OffenseController : Controller
    {
        private readonly OffenseRepository offenseRepository;
        private readonly ApplicationUserRepository applicationUserRepository;
        private readonly WorkingDayRepository workingDayRepository;

        public OffenseController(AttendanceSystemContext db)
        {
            offenseRepository = new OffenseRepository(db);
            workingDayRepository = new WorkingDayRepository(db);
            applicationUserRepository = new ApplicationUserRepository(db);
        }

        [Authorize(Roles = ApplicationUser.AdminRole)]
        [HttpGet]
        public async Task<IActionResult> OffensesApproval()
        {
            List<Offense> offenses = await offenseRepository.GetNeedApprovalOffenses();
            ViewBag.LeaveEarlyCount = offenses.Count(record => record.Degree == OffenseDegree.LeaveEarly);
            ViewBag.LatesCount = offenses.Count(record => record.Degree == OffenseDegree.LateLessThanHourNoMakeUp 
            || record.Degree == OffenseDegree.LateMoreThanHourNoMakeUp);
            ViewBag.AbsencesCount = offenses.Count(record => record.Degree == OffenseDegree.Absence);
            return View("OffensesApproval", offenses);
        }

        [HttpGet]
        [NonAction] // Can not be called directly with URL
        public async Task<IActionResult> ViewOffenses(string id, DateTime s, DateTime e, int n = 0)
        {
            DateTime startDate = s.AddMonths(n);
            DateTime endDate = e.AddMonths(n);
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
            int dailySalary = await applicationUserRepository.GetDailySalary(id);
            ViewBag.PenaltyAmount = (int)((dailySalary * ViewBag.TotalPenaltyPercent) / 100.0) * -1;

            IEnumerable<Offense> offenses = await offenseRepository.GetUserOffenses(id, startDate, endDate);

            return View("Offenses", offenses);
        }

        [HttpGet]
        [Route("Offenses/{id}/{s}/{e}")]
        public async Task<IActionResult> OffensesInRange(string id, long s, long e, int n = 0)
        {
            return await ViewOffenses(id, new DateTime(s), new DateTime(e), n);
        }
       
        [Route("Offenses/")]
        [HttpGet]
        public async Task<IActionResult> MyOffenses(int n = 0)
        {
            return await UserOffenses(User.FindFirst(ClaimTypes.NameIdentifier).Value, n);
        }

        [HttpGet]
        [Route("Offenses/{id}")]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> UserOffenses(string id, int n = 0)
        {
            return await ViewOffenses(id, DateTime.Today.GetMonthStartDate(), DateTime.Today.GetMonthEndDate(), n);
        }

        [HttpGet]
        [Route("OffensesReport/")]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> OffensesReport(int n=0)
        {
            return await OffensesReport(DateTime.Today.GetMonthStartDate().Ticks, DateTime.Today.GetMonthEndDate().Ticks, n);
        }

        [HttpGet]
        [Route("OffensesReport/{s}/{e}")]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> OffensesReport(long s, long e, int n=0)
        {
            DateTime startDate = new DateTime(s).AddMonths(n);
            DateTime endDate = new DateTime(e).AddMonths(n);

            ViewBag.StartDate = startDate.ToString("dd-MM-yyyy");
            ViewBag.EndDate = endDate.ToString("dd-MM-yyyy");
            ViewBag.Next = n + 1;
            ViewBag.Previous = n - 1;
            ViewBag.Absences = 0;
            ViewBag.Lates = 0;
            ViewBag.MakeUps = 0;
            ViewBag.Penalty = 0;
            ViewBag.PenaltyAmount = 0;

            IEnumerable<ApplicationUser> users = await applicationUserRepository.GetPureUsers();
            List<UserOffensesReport> userReports = new List<UserOffensesReport>();

            foreach (ApplicationUser user in users)
            {
                int absents = await offenseRepository.CountOfAbsents(user.Id, startDate, endDate);
                ViewBag.Absences += absents;
                int lates = await offenseRepository.CountOfLates(user.Id, startDate, endDate);
                ViewBag.Lates += lates;
                int makeUps = await offenseRepository.CountOfMakeUps(user.Id, startDate, endDate);
                ViewBag.MakeUps += makeUps;
                int penaltyPercent = await offenseRepository.SumOfPenaltyPercent(user.Id, startDate, endDate);
                ViewBag.Penalty += penaltyPercent;
                int dailySalary = await applicationUserRepository.GetDailySalary(user.Id);
                int penaltyAmount = (int) ((dailySalary * penaltyPercent) / 100.0) * -1;
                ViewBag.PenaltyAmount += penaltyAmount;

                userReports.Add(new UserOffensesReport(user.FullName, absents, lates, makeUps, penaltyPercent, penaltyAmount));
            }
            return View(userReports);
        }

        [HttpGet]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> NotifiedLeaveEarly(string id)
        {
            Offense offense = await offenseRepository.GetOffenseByID(id);
            offense.NeedApproval = false;
            await offenseRepository.Update(offense);            
            TempData["StatusMessage"] = "Offense has been marked as notified.";
            return await OffensesApproval();
        }

        [HttpGet]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> NotNotifiedLeaveEarly(string id)
        {
            Offense offense = await offenseRepository.GetOffenseByID(id);
            offense.PenaltyPercent = 50;
            offense.NeedApproval = false;
            await offenseRepository.Update(offense);
            TempData["StatusMessage"] = "Offense has been marked as not notified and 50% penalty imposed.";
            return await OffensesApproval();
        }

        [HttpGet]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> ExcusedAbsence(string id)
        {
            Offense offense = await offenseRepository.GetOffenseByID(id);
            offense.NeedApproval = false;
            offense.PenaltyPercent = 0;
            await offenseRepository.Update(offense);
            WorkingDay workingDay = offense.WorkingDay;
            await workingDayRepository.CheckAbsencesInRange(workingDay.UserID, workingDay.Date.AddDays(1), DateTime.Today.AddDays(-1));
            TempData["StatusMessage"] = "Offense has been marked as excused and no penalty imposed.";
            return await OffensesApproval();
        }

        [HttpGet]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> UnexcusedAbsence(string id)
        {
            Offense offense = await offenseRepository.GetOffenseByID(id);
            offense.NeedApproval = false;
            await offenseRepository.Update(offense);
            TempData["StatusMessage"] = "Absence has been marked as unexcused.";
            return await OffensesApproval();
        }
    }
}
