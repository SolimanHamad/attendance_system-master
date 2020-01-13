using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Repositories;

namespace AttendanceSystem.Controllers
{
    public class RuleController : Controller
    {
        private readonly RuleRepository ruleRepository;
        private readonly MonthlyAllowanceRuleRepository monthlyAllowanceRuleRepository;

        public RuleController(AttendanceSystemContext db)
        {
            monthlyAllowanceRuleRepository = new MonthlyAllowanceRuleRepository(db);
            ruleRepository = new RuleRepository(db);
        }

        [HttpGet]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> Index()
        {
            return View(new RuleViewModel
            {
                Rules = await ruleRepository.GetAllRules(), 
                Allowances = await monthlyAllowanceRuleRepository.GetAllMonthlyAllowanceRules()
            });
        }

        [HttpPost]
        [Authorize(Roles = ApplicationUser.AdminRole)]
        public async Task<IActionResult> Index(RuleViewModel model)
        {
            if (ModelState.IsValid)
            {
                await ruleRepository.AddRules(model.Rules);
                await monthlyAllowanceRuleRepository.AddMonthlyAllowanceRules(model.Allowances);
                TempData["StatusMessage"] = "Changes have been saved successfully.";
            }
            else
            {
                TempData["StatusMessage"] = "Error: Some fields are invalid!";
            }
            return View(model);
        }
    }
}
