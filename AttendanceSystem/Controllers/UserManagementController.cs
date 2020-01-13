using System;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using Microsoft.AspNetCore.Mvc;
using AttendanceSystem.Models;
using AttendanceSystem.Repositories;
using AttendanceSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace AttendanceSystem.Controllers
{
    [Authorize(Roles = ApplicationUser.AdminRole)]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationUserRepository applicationUserRepository;

        public UserManagementController(UserManager<ApplicationUser> userManager, AttendanceSystemContext attendanceSystemContext)
        {
            this.userManager = userManager;
            applicationUserRepository = new ApplicationUserRepository(attendanceSystemContext);
        }

        // GET: UserManagement
        public IActionResult Index() => View();
        
        // GET: UserManagement/Create
        public IActionResult Create() => View(new UserManagementModel());

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserManagementModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = MapToModel(new ApplicationUser(), model);
                IdentityResult result = await userManager.CreateAsync(user, model.Password);

                if (model.IsAdmin)
                {
                    await userManager.AddToRoleAsync(user, ApplicationUser.AdminRole);
                }

                if (result.Succeeded)
                {
                    TempData["StatusMessage"] = "New account has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // ajax validation for email 
        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            if (User.Identity.IsAuthenticated)
            {
                ApplicationUser authUser = await userManager.GetUserAsync(HttpContext.User);

                if (authUser.Email == email)
                {
                    return Json(true);
                }
            }
            
            return Json(true);
        }


        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplicationUser user = await applicationUserRepository.GetByID(id);
            if (user == null)
            {
                return NotFound();
            }

            bool isAdmin = await userManager.IsInRoleAsync(user, ApplicationUser.AdminRole);
            return View(new UserManagementModel(user, isAdmin));
        }

        
        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserManagementModel model)
        {
            if (!await UserExists(id))
                return NotFound();

            // TODO: make it seperate view model without those two fields, if you really want to :)
            ModelState.Remove("Password");
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                ApplicationUser user = MapToModel(await applicationUserRepository.GetByID(id), model);
                if (model.IsAdmin) 
                    await userManager.AddToRoleAsync(user, ApplicationUser.AdminRole);
                else 
                    await userManager.RemoveFromRoleAsync(user, ApplicationUser.AdminRole);

                await applicationUserRepository.UpdateAsync(user);
                TempData["StatusMessage"] = "Account has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        
        // GET: UserManagement/Disable/5
        public async Task<IActionResult> Disable(string id)
        {
            if (id == null)
                return NotFound();

            ApplicationUser user = await applicationUserRepository.GetByID(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        // POST: UserManagement/Disable/5
        [HttpPost, ActionName("Disable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableConfirmed(string id)
        {
            await applicationUserRepository.Disable(id);
            TempData["StatusMessage"] = "Account has been disabled successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: UserManagement/Enable/5
        [HttpPost, ActionName("Enable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableConfirmed(string id)
        {
            await applicationUserRepository.Enable(id);
            TempData["StatusMessage"] = "Account has been enabled successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        // GET: UserManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            ApplicationUser user = await applicationUserRepository.GetByID(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        // POST: UserManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await applicationUserRepository.Delete(id);
            TempData["StatusMessage"] = "Account has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UserExists(string id)
        {
            return (await applicationUserRepository.GetByID(id) != null);
        }

        private ApplicationUser MapToModel(ApplicationUser user, UserManagementModel model)
        {
            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.DateJoined = model.DateJoined;
            user.VacationBalance = model.VacationBalance;
            user.YearlyIncrement = model.YearlyIncrement;
            user.GracePeriod = new TimeSpan(0, (int)model.GracePeriod, 0);
            user.WorkStartTime = model.WorkStartTime;
            user.WorkEndTime = model.WorkEndTime;
            user.Salary = model.Salary;
            user.PhoneNumber = model.PhoneNumber;
            user.InsuranceDeduction = model.InsuranceDeduction;
            user.HousingAllowance = model.HousingAllowance;
            user.TransportationAllowance = model.TransportationAllowance;
            user.CheckAttendance = model.CheckAttendance;
            return user;
        }
    }
}
