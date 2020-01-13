using AttendanceSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Data;
using AttendanceSystem.Models.Enums;
using AttendanceSystem.Repositories;

namespace AttendanceSystem.Jobs
{
    public class DailyJob
    {
        private readonly WorkingDayRepository workingDayRepository;
        private readonly ApplicationUserRepository usersRepository;
        private readonly RuleRepository ruleRepository;
        private readonly OffenseRepository offenseRepository;
        private readonly MonthlyAllowanceRuleRepository monthlyAllowanceRuleRepository;

        public DailyJob(AttendanceSystemContext context)
        {
            monthlyAllowanceRuleRepository = new MonthlyAllowanceRuleRepository(context);
            offenseRepository = new OffenseRepository(context);
            ruleRepository = new RuleRepository(context);
            usersRepository = new ApplicationUserRepository(context);
            workingDayRepository = new WorkingDayRepository(context);
        }
        
        public async Task CheckAllUsersOffensesYesterday()
        {
            await workingDayRepository.DeleteEmptyDays();
            foreach (ApplicationUser user in await usersRepository.GetActiveUsers())
            {
                if (user.CheckAttendance && !user.WasOnVacationYesterday()) // If user was not on vacation, and we take his attendance 
                {
                    WorkingDay userWorkingDay = await workingDayRepository.GetYesterdaysWorkingDay(user.Id);

                    if (userWorkingDay != null) // User was not absent
                    {
                        // Check if user conduct any offense 
                        await CheckWorkingDayOffenses(userWorkingDay);
                    }
                    else // User was absent
                    {
                        // Record offense: absent
                        await RecordDetectedOffense(user, null, OffenseDegree.Absence);
                    }
                }
            }
        }

        public async Task CheckWorkingDayOffenses(WorkingDay workDay)
        {
            // Inner method to avoid passing same arguments on each call for RecordDetectedOffense(...)
            async Task HandleOffense(OffenseDegree degree)
            {
                await RecordDetectedOffense(workDay.User, workDay, degree);
            }
            
            // Remove any offenses recorded previously for this working day
            if (workDay.Offenses?.Any() ?? false)
                await offenseRepository.RemoveAll(workDay.Offenses);

            if (workDay.IsLate()) // User is late
            {
                if (workDay.IsLateMoreThanHour()) // User late more than one hour
                {
                    // User did NOT make up for his lateness
                    if (workDay.IsStayedLessThanRequired())
                    {
                        // Record offense: late more than hour and did not make up
                        await HandleOffense(OffenseDegree.LateMoreThanHourNoMakeUp);
                    }
                    else // User DID make up for his lateness
                    {
                        // Record offense: late more than hour and make up
                        await HandleOffense(OffenseDegree.LateMoreThanHourWithMakeUp);
                    }
                }
                else // User late less than hour
                {
                    // User did NOT make up for his lateness
                    if (workDay.IsStayedLessThanRequired())
                    {
                        // Record offense: late less than hour and did not make up
                        await HandleOffense(OffenseDegree.LateLessThanHourNoMakeUp);
                    }
                    else // User DID make up for his lateness
                    {
                        // Record offense: late less than hour and make up
                        await HandleOffense(OffenseDegree.LateLessThanHourWithMakeUp);
                    }
                }
                if (workDay.CheckOut.HasValue && workDay.User.WorkEndTime > workDay.CheckOut.Value.TimeOfDay)
                {
                    await HandleOffense(OffenseDegree.LeaveEarly);
                }
            }
            else if (workDay.IsStayedLessThanRequired()) // User left early but was not late
            {
                // Record offense: left early
                await HandleOffense(OffenseDegree.LeaveEarly);
            }
        }

        public async Task RecordDetectedOffense(ApplicationUser user, WorkingDay workingDay, OffenseDegree degree)
        {                      
            WorkingDay userWorkingDay;
            if (workingDay == null) // user was absent
            {
                // Create new empty working day object for the day that the user miss
                userWorkingDay = new WorkingDay(user.Id, DateTime.Today.AddDays(-1));
                await workingDayRepository.Add(userWorkingDay);
            }
            else
            {
                userWorkingDay = workingDay;
            }
            Offense offense;

            if (await HasAllowance(user.Id, userWorkingDay, degree))
            {
                offense = new Offense(userWorkingDay.ID, 0, degree) {HasAllowance = true};
            }
            else
            {
                // Get how many times this offense occurred within 
                // the last 90 days and increment it by one (for absence, count the unexcused ones only)
                int occurrence;
                if (degree == OffenseDegree.Absence)
                {
                    occurrence = await offenseRepository.CountAbsenceInLast90Days(user.Id, userWorkingDay) + 1;
                }
                else
                {
                    occurrence = await offenseRepository.CountOffensesInLast90Days(user.Id, userWorkingDay, degree) + 1;
                }
                // If the occurence is not within the Occurrence enum, assign it to the last enum (FifthOrMore)
                if (!Enum.IsDefined(typeof(Occurrence), occurrence))
                {
                    Array occurrencesArray = Enum.GetValues(typeof(Occurrence));
                    // Get the last object in the occurence enum & assign it to occurrence
                    occurrence = (int)occurrencesArray.GetValue(occurrencesArray.Length - 1);
                }
                // Get penalty percent per day desired for this offense
                int penaltyPercent = ruleRepository.GetPenaltyPercent(degree, (Occurrence)occurrence);
                // Create new offence object & save it
                offense = new Offense(userWorkingDay.ID, penaltyPercent, degree);
            // Check if it needs approval
            if (degree == OffenseDegree.Absence || degree == OffenseDegree.LeaveEarly)
                offense.NeedApproval = true;
            }
            await offenseRepository.Add(offense);
        }

        private async Task<bool> HasAllowance(string userID, WorkingDay workingDay, OffenseDegree degree)
        {
            int allowanceOccurrence = await offenseRepository.CountAllowancesInThisMonth(userID, workingDay.Date, degree);
            int allowanceRule = await monthlyAllowanceRuleRepository.GetAllowance(degree);
            // Check the logically-related offenses (feeding allowances) 
            if (degree == OffenseDegree.LateLessThanHourNoMakeUp || degree == OffenseDegree.LateLessThanHourWithMakeUp)
            {
                // LateMoreThanHourWithMakeUp can feed LateLessThanHourWithMakeUp
                // LateMoreThanHourNoMakeUp can feed LateLessThanHourNoMakeUp 
                // (difference between related offense degrees is 2 in the enum OffenseDegree)
                int feedingAllowanceOccurrence = await offenseRepository.CountAllowancesInThisMonth(userID, workingDay.Date, degree + 2);
                int feedingAllowanceRule = await monthlyAllowanceRuleRepository.GetAllowance(degree + 2);
                int balance = feedingAllowanceRule - feedingAllowanceOccurrence;
                if (balance > 0)
                    allowanceRule += balance;
            } // Check if the user has allowances taken from this degree
            else if(degree == OffenseDegree.LateMoreThanHourNoMakeUp || degree == OffenseDegree.LateMoreThanHourWithMakeUp)
            {
                int feedingAllowanceRule = await monthlyAllowanceRuleRepository.GetAllowance(degree - 2);
                int feedingAllowanceOccurrence = await offenseRepository.CountAllowancesInThisMonth(userID, workingDay.Date, degree - 2);
                int balance = feedingAllowanceRule - feedingAllowanceOccurrence;
                if(balance < 0)
                    allowanceRule += balance;
            }
            return allowanceOccurrence < allowanceRule;                
        }
    }
}
