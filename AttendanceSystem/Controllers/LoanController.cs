using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Repositories;
using AttendanceSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    public class LoanController : Controller
    {
        private readonly LoanRepository loanRepository;
        private readonly ApplicationUserRepository applicationUserRepository;

        public LoanController(AttendanceSystemContext context)
        {
            loanRepository = new LoanRepository(context);
            applicationUserRepository = new ApplicationUserRepository(context);
        }

        // GET: Loan
        public IActionResult Index() => View();

        // GET: Loan/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.users = await applicationUserRepository.GetAllUsers();
            return View();
        }

        // POST: Loan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OriginalAmount,MonthlyPayment,UserId")] LoanViewModel model)
        {
            if (await loanRepository.UserHasActiveLoan(model.UserId))
            {
                ModelState.AddModelError("", "This employee already has an active loan!");
            }

            if (ModelState.IsValid)
            {
                Loan loan = MapToModel(new Loan(), model, false);
                await loanRepository.Add(loan);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.users = await applicationUserRepository.GetAllUsers();
            return View(model);
        }

        // GET: Loan/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            Loan loan = await loanRepository.GetLoanById(id);
            if (loan == null)
                return NotFound();

            if (await loanRepository.LoanIsInactiveAsync(loan.Id))
            {
                TempData["StatusMessage"] = "Error: you can't edit an inactive (i.e. old) loan.";
                return View(nameof(Index));
            }

            return View(new LoanViewModel(loan));
        }

        // POST: Loan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,OriginalAmount,RemainingAmount,MonthlyPayment,UserId")] LoanViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                Loan loan = await loanRepository.GetLoanById(id);
                MapToModel(loan, model, true);
                await loanRepository.Update(loan);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Loan/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            Loan model = await loanRepository.GetLoanById(id);
            if (model == null)
                return NotFound();
            
            return View(new LoanViewModel(model));
        }

        // POST: Loan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await loanRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private Loan MapToModel(Loan loan, LoanViewModel model, bool isEditing)
        {
            loan.OriginalAmount = model.OriginalAmount;
            loan.RemainingAmount = isEditing ? model.RemainingAmount : model.OriginalAmount;
            loan.MonthlyPayment = model.MonthlyPayment;
            loan.UserID = model.UserId;
            return loan;
        }
    }
}
