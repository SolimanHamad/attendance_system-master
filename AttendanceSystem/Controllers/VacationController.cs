using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Models.Enums;
using AttendanceSystem.Repositories;
using AttendanceSystem.Services;
using AttendanceSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [Authorize]
    public class VacationController : Controller
    {
        private readonly VacationRepository vacationRepository;
        private readonly ApplicationUserRepository applicationUserRepository;
        private readonly OffenseRepository offenseRepository;
        private readonly WorkingDayRepository workingDayRepository;
        private readonly IEmailService emailService;

        public VacationController(AttendanceSystemContext context, IEmailService emailService)
        {
            vacationRepository = new VacationRepository(context);
            applicationUserRepository = new ApplicationUserRepository(context);
            workingDayRepository = new WorkingDayRepository(context);
            offenseRepository = new OffenseRepository(context);
            this.emailService = emailService;
        }

        public async Task<IActionResult> Index(int? y)
        {
            int year = y ?? DateTime.Now.Year;
            ApplicationUser user = await applicationUserRepository.GetByID(GetLoggedUserId());
            ViewBag.VacationBalance = user.VacationBalance;
            ViewBag.YearlyIncrement = user.YearlyIncrement;
            ViewBag.TotalRequestedVacationDays = await vacationRepository.GetUserTotalPersonalVacationDaysByYear(GetLoggedUserId(), year);
            ViewBag.Year = year;
            return View(await vacationRepository.GetUserVacationByYear(GetLoggedUserId(), year));
        }

        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> Manage()
        {
            List<VacationViewModel> vacationCollection = (await vacationRepository.GetAllVacationSorted()).Select(v => new VacationViewModel(v)).ToList();
            return View(new SearchViewModel() { Vacations = vacationCollection }); // get vacations 
        }

        [HttpPost]
        public async Task<IActionResult> Manage(SearchViewModel model)
        {
            model.Vacations = (await vacationRepository.GetAllVacationSorted(model.Name, model.IssueDate, model.StartDate, model.EndDate, model.TotalDays, model.VacationType, model.RequestStatus, model.SortBy)).Select(v => new VacationViewModel(v)).ToList();
            return View(model);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartDate,EndDate,VacationType,Comment")] VacationViewModel model)
        {

            if (ModelState.IsValid)
            {
                Vacation vacation = await MapToModel(new Vacation(), model, GetLoggedUserId());
                // validate that requested vacation days is within the allowed user vacation days
                ValidateRequestedDaysAvailability(vacation);

                if (ModelState.IsValid)
                {
                    if (await IsNewVacationConflicting(vacation.StartDate, vacation.EndDate))
                    {
                        TempData["StatusMessage"] = "The requested vacation overlaps with a previous one";
                    }
                    else
                    {
                        await DeductUserVacationBalance(new VacationViewModel(vacation));
                        await vacationRepository.Add(vacation);
                        TempData["StatusMessage"] = "New vacation has been created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            return View(model);
        }

        public IActionResult CreateHalfDay() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHalfDay([Bind("Id,StartDate,Comment")] Vacation model)
        {
            ModelState.Remove(ModelState.Keys.First());
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                ApplicationUser user = await applicationUserRepository.GetByID(GetLoggedUserId());
                model.User = user;
                model.UserID = user.Id;
                model.EndDate = model.StartDate.AddHours(12);
                model.VacationType = VacationType.Personal;

                // validate that requested vacation days is within the allowed user vacation days
                ValidateRequestedDaysAvailability(model);

                if (ModelState.IsValid)
                    //if ( await IsExistingVacationConflicting() )
                {
                    if (await IsNewVacationConflicting(model.StartDate, model.EndDate))
                    {
                        TempData["StatusMessage"] = "The requested vacation overlaps with another  one";
                    }
                    else
                    {
                        await DeductUserVacationBalance(new VacationViewModel(model));
                        await vacationRepository.Add(model);
                        TempData["StatusMessage"] = "New half-day vacation has been created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            Vacation vacation = await vacationRepository.GetByID(id);
            if (vacation == null)
                return NotFound();

            VacationViewModel model = new VacationViewModel(vacation);
            return model.TotalDays != 0.5 ? View(model) : View("EditHalfDay", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string returnUrl, [Bind("Id,StartDate,EndDate,VacationType,Status,Comment")] VacationViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            Vacation vacation = await vacationRepository.GetByID(id);
            // add back old deducted days from user vacation balance 
            await IncrementUserVacationBalance(new VacationViewModel(vacation));
            await MapToModel(vacation, model, vacation.UserID);

            // if the vacation got rejected don't validate 
            if (model.Status != RequestStatus.Rejected)
            {
                // validate that requested vacation days is within the allowed user vacation days
                ValidateRequestedDaysAvailability(vacation);
            }
            if (ModelState.IsValid)
            {
                if (await IsExistingVacationConflicting(vacation.StartDate, vacation.EndDate, vacation.ID))
                {
                    TempData["StatusMessage"] = "The requested vacation overlaps with a previous one";
                }
                else
                {
                    // only update status if it's the admin
                    if (IsAdmin())
                    {
                        vacation.Status = model.Status;
                        switch (model.Status)
                        {
                            case RequestStatus.Approved:
                                await UpdateOffenses(vacation);
                                break;
                            case RequestStatus.Rejected:
                                await workingDayRepository.CheckAbsencesInRange(vacation.UserID, vacation.StartDate, DateTime.Today.AddDays(-1));
                                break;
                        }
                    }
                    // valid requested days & personal & pending or approved ? deduct requested days from vacation balance
                    await DeductUserVacationBalance(new VacationViewModel(vacation));
                    await vacationRepository.Update(vacation);
                    TempData["StatusMessage"] = "Vacation has been updated successfully.";
                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction(nameof(Index));
                }
            }
            return (vacation.EndDate - vacation.StartDate).TotalDays != 0.5 ? View(model) : View("EditHalfDay", model);
        }

        private async Task UpdateOffenses(Vacation vacation)
        {
            // In case the user asked for vacation he already taken in past:
            // Remove offenses recorded in the vacation period (absences)
            await offenseRepository.RemoveInRange(vacation.UserID, vacation.StartDate, vacation.EndDate);
            // Re-check absences from after vacation until a day before admin approval (so that the occurrences of any recorded absence in this period became correct)
            await workingDayRepository.CheckAbsencesInRange(vacation.UserID, vacation.EndDate.AddDays(1), DateTime.Today.AddDays(-1));
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Vacation vacation = await vacationRepository.GetByID(id);
            if (vacation == null)
            {
                return NotFound();
            }

            return View(new VacationViewModel(vacation));
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, string returnUrl)
        {
            // if vacation was approved & personal add its days back 
            Vacation vacation = await vacationRepository.GetByID(id);
            await IncrementUserVacationBalance(new VacationViewModel(vacation));

            await vacationRepository.Delete(id);
            TempData["StatusMessage"] = "Vacation has been deleted successfully.";
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = ApplicationUser.AdminRole)]
        public IActionResult AddVacationToAllEmployees() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> AddVacationToAllEmployees(VacationViewModel model)
        {
            if (ModelState.IsValid)
            {
                await vacationRepository.AddVacationToAllEmployees(model, await applicationUserRepository.GetAllUsers());
                TempData["StatusMessage"] = "New general vacation has been created to all employees successfully.";
                return RedirectToAction(nameof(Manage));
            }
            return View(model);
        }

        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> AddVacationToEmployee()
        {
            ViewBag.users = await applicationUserRepository.GetAllUsers();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> AddVacationToEmployee([Bind("Id,StartDate,EndDate,UserId,Comment,VacationType")] VacationViewModel model)
        {
            ModelState.Remove(ModelState.Keys.First());
            Vacation vacation = await MapToModel(new Vacation(), model, model.UserId);
            ValidateRequestedDaysAvailability(vacation);

            if (ModelState.IsValid)
            {
                if (await IsAddedVacationConflicting(vacation.StartDate, vacation.EndDate, model.UserId))
                {
                    TempData["StatusMessage"] = "The requested vacation overlaps with a previous one";
                }
                else
                {
                    vacation.Status = RequestStatus.Approved;
                    await DeductUserVacationBalance(new VacationViewModel(vacation));
                    await vacationRepository.Add(vacation);
                    TempData["StatusMessage"] = "New vacation has been created to the selected employee successfully.";
                    return RedirectToAction(nameof(Manage));
                }
            }

            ViewBag.users = await applicationUserRepository.GetAllUsers();
            return View(model);
        }

        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> Approve(string id)
        {
            Vacation vacation = await vacationRepository.GetByID(id);
            if (vacation == null)
                return NotFound();


            // not approved => approved
            if (vacation.Status != RequestStatus.Approved)
            {
                if (vacation.Status != RequestStatus.Pending)
                {
                    if (await IsNewVacationConflicting(vacation.StartDate, vacation.EndDate))
                    {
                        TempData["StatusMessage"] = "Another vacation is overalpping";
                        return RedirectToAction(nameof(Manage));
                    }
                    vacation.Status = RequestStatus.Approved;
                    await DeductUserVacationBalance(new VacationViewModel(vacation));
                }
                vacation.Status = RequestStatus.Approved;
                await vacationRepository.Update(vacation);
                
                await UpdateOffenses(vacation);
                
                emailService.SendEmail(vacation.User.Email, "Vacation approved", "Good News",$"Your vacation starting at {vacation.StartDate} has been approved!"); 
                TempData["StatusMessage"] = "Vacation has been approved successfully.";
            }

            // approved => approved
            else
                TempData["StatusMessage"] = "Error: vacation has already been approved!";
            return RedirectToAction(nameof(Manage));
        }

        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> Reject(string id)
        {
            Vacation vacation = await vacationRepository.GetByID(id);
            if (vacation == null)
                return NotFound();

            // not rejected => rejected
            if (vacation.Status != RequestStatus.Rejected)
            {
                await IncrementUserVacationBalance(new VacationViewModel(vacation));
                vacation.Status = RequestStatus.Rejected;
                await vacationRepository.Update(vacation);
                TempData["StatusMessage"] = "Vacation has been rejected successfully.";
                await workingDayRepository.CheckAbsencesInRange(vacation.UserID, vacation.StartDate, DateTime.Today.AddDays(-1));
            }

            // rejected => rejected
            else
                TempData["StatusMessage"] = "Error: vacation has already been rejected!";
            return RedirectToAction(nameof(Manage));
        }

        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> Heatmap()
        {
            IEnumerable<HeatMapViewModel> vacationCounts = await vacationRepository.GetExcusedVacationCountsPerDay();
            return View(vacationCounts);
        }

        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> HeatMapAsPercentages()
        {
            IEnumerable<HeatMapViewModel> vacationCounts = await vacationRepository.GetExcusedVacationCountsPerDayAsPercentages();
            return View(nameof(Heatmap), vacationCounts);
        }

        private string GetLoggedUserId() => User.FindFirst(ClaimTypes.NameIdentifier).Value;

        private bool IsAdmin() => User.IsInRole(ApplicationUser.AdminRole);

        private async Task IncrementUserVacationBalance(VacationViewModel old)
        {
            if (old.Status != RequestStatus.Rejected && old.VacationType == VacationType.Personal)
            {
                ApplicationUser user = await applicationUserRepository.GetByID(old.UserId);
                user.VacationBalance += old.TotalDays;
            }
        }

        private async Task DeductUserVacationBalance(VacationViewModel updated)
        {
            if (updated.Status != RequestStatus.Rejected && updated.VacationType == VacationType.Personal)
            {
                ApplicationUser user = await applicationUserRepository.GetByID(updated.UserId);
                user.VacationBalance -= updated.TotalDays;
            }
        }

        private async Task<bool> IsNewVacationConflicting(DateTime start, DateTime end)
        {
            List<VacationViewModel> vacations = await vacationRepository.GetNotRejectedUserVacation(GetLoggedUserId());
            foreach (VacationViewModel vac in vacations)
            {
                if (vac.StartDate <= end && start <= vac.EndDate) // detect conflict in requested vacation and existing ones
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> IsAddedVacationConflicting(DateTime start, DateTime end, string userId)
        {
            List<VacationViewModel> vacations = await vacationRepository.GetNotRejectedUserVacation(userId);
            foreach (VacationViewModel vac in vacations)
            {
                if (vac.StartDate <= end && start <= vac.EndDate) // detect conflict in requested vacation and existing ones
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> IsExistingVacationConflicting(DateTime start, DateTime end, string id)
        {
            List<VacationViewModel> vacations = await vacationRepository.GetNotRejectedUserVacation(GetLoggedUserId());
            vacations.Remove(vacations.FirstOrDefault(n => n.Id == id));
            foreach (VacationViewModel vac in vacations)
            {
                if (vac.StartDate <= end && start <= vac.EndDate) // detect conflict in requested vacation and existing ones
                {
                    return true;
                }
            }
            return false;

        }

        private async Task<Vacation> MapToModel(Vacation vacation, VacationViewModel model, string userId)
        {
            ApplicationUser user = await applicationUserRepository.GetByID(userId);
            vacation.User = user;
            vacation.UserID = userId;
            if (new VacationViewModel(vacation).TotalDays == 0.5)
            {
                vacation.EndDate = model.StartDate.AddHours(12);
                ModelState.Remove(ModelState.Keys.First());
            }
            else
            {
                vacation.EndDate = model.EndDate;
            }
            vacation.StartDate = model.StartDate;
            vacation.Comment = model.Comment;
            vacation.VacationType = model.VacationType;
            return vacation;
        }

        private void ValidateRequestedDaysAvailability(Vacation vacation)
        {
            if (vacation.VacationType == VacationType.Personal)
            {
                TryValidateModel(new VacationViewModel(vacation));
            }
        }
    }
}
