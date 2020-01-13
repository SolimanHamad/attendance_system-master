using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using AttendanceSystem.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Extensions;
using AttendanceSystem.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace AttendanceSystem.Controllers
{

    [Authorize(Roles = ApplicationUser.AdminRole)]
    public class SalarySheetController : Controller
    {
        private readonly ApplicationUserRepository applicationUserRepository;
        private readonly VacationRepository vacationRepository;
        private readonly OffenseRepository offenseRepository;
        private readonly SalarySheetRepository salarySheetRepository;
        private readonly LoanRepository loanRepository;

        public SalarySheetController(AttendanceSystemContext db)
        {
            applicationUserRepository = new ApplicationUserRepository(db);
            vacationRepository = new VacationRepository(db);
            loanRepository = new LoanRepository(db);
            salarySheetRepository = new SalarySheetRepository(db);
            offenseRepository = new OffenseRepository(db);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int n = 0, bool edit = false)
        {
            DateTime startDate = DateTime.Today.AddMonths(-1).GetMonthStartDate(n);
            DateTime endDate = DateTime.Today.AddMonths(-1).GetMonthEndDate(n);
            ViewBag.Next = n;
            ViewBag.StartDate = startDate.ToString("dd-MM-yyyy");
            ViewBag.EndDate = endDate.ToString("dd-MM-yyyy");

            if (edit)
            {
                List<SalarySheet> salarySheet = await salarySheetRepository.GetSalarySheet(endDate);
                return View("Edit", salarySheet);
            }

            if (endDate < DateTime.Today)
            {
                List<SalarySheetRecord> salarySheet = await salarySheetRepository.GetSalarySheetRecords(endDate, false);

                if (!salarySheet.Any())
                {
                    IEnumerable<ApplicationUser> allUsers = await applicationUserRepository.GetActiveUsers();
                    foreach (ApplicationUser user in allUsers)
                    {
                        string userName = user.FullName;
                        int salary = (int)user.Salary;
                        int penaltiesPercent = await offenseRepository.SumOfPenaltyPercent(user.Id, startDate, endDate);
                        int dailySalary = await applicationUserRepository.GetDailySalary(user.Id);
                        int attendancePenalties = (int)((dailySalary * penaltiesPercent) / 100.0);
                        double unpaidVacationDays = await vacationRepository.GetUnpaidUserVacationsInRange(user.Id, startDate, endDate);
                        int unpaidVacationDeduction = (int)unpaidVacationDays*dailySalary;
                        int insuranceDeduction = (int)((user.InsuranceDeduction * salary) / 100.0);
                        Loan loan = await loanRepository.GetActiveLoanByUserId(user.Id);
                        int loanDeduction = loan != null ? Math.Min(loan.MonthlyPayment, loan.RemainingAmount) : 0;
                        int housingAllowance = user.HousingAllowance;
                        int transportationAllowance = user.TransportationAllowance;
                        int finalSalary = (salary + housingAllowance + transportationAllowance) - (attendancePenalties + unpaidVacationDeduction + insuranceDeduction + loanDeduction);
                        salarySheet.Add(new SalarySheetRecord(user.Id, userName, salary, attendancePenalties, unpaidVacationDeduction, insuranceDeduction, loanDeduction, housingAllowance, transportationAllowance, finalSalary, endDate, true));
                    }
                }
                return View("Index", salarySheet);
            }
            return View("Index", new List<SalarySheetRecord>());
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<SalarySheetRecord> recordList, int n = 0)
        {
            if (ModelState.IsValid)
            {
                List<SalarySheet> salarySheet = new List<SalarySheet>();
                foreach (SalarySheetRecord record in recordList)
                {
                    Loan loan = await loanRepository.GetActiveLoanByUserId(record.UserID);

                    SalarySheet salaryRecord = new SalarySheet()
                    {
                        UserID = record.UserID,
                        Salary = record.Salary,
                        AttendancePenalties = record.AttendancePenalties,
                        UnpaidVacationDeduction = record.UnpaidVacationDeduction,
                        InsuranceDeduction = record.InsuranceDeduction,
                        LoanDeduction = Math.Min(record.LoanDeduction, loan != null ? loan.RemainingAmount : 0),
                        OtherDeductions = record.OtherDeductions,
                        HousingAllowance = record.HousingAllowance,
                        TransportationAllowance = record.TransportationAllowance,
                        OtherAdditions = record.OtherAdditions,
                        Comment = record.Comment,
                        Date = record.EndDate
                    };
                    salaryRecord.SetFinalSalary();
                    salarySheet.Add(salaryRecord);

                    // Update remaining loan amount 
                    if (loan != null)
                    {
                        loan.RemainingAmount -= salaryRecord.LoanDeduction;
                        await loanRepository.Update(loan);
                    }
                }

                await salarySheetRepository.UpdateList(salarySheet);
                TempData["StatusMessage"] = "Salary Sheet has been generated & saved successfully!";
            }
            else
            {
                TempData["StatusMessage"] = "Error: Some fields are invalid!";
            }
            return await Index(n);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(List<SalarySheet> salarySheet, int n)
        {
            if (ModelState.IsValid)
            {
                foreach (SalarySheet record in salarySheet)
                {
                    record.SetFinalSalary();
                }
                await salarySheetRepository.UpdateList(salarySheet);
                TempData["StatusMessage"] = "Salary Sheet has been saved successfully!";
            }
            else
            {
                TempData["StatusMessage"] = "Error: Some fields are invalid!";
            }

            return RedirectToAction(nameof(Index), new {n} );
        }
    }
}
