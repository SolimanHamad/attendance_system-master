using AttendanceSystem.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.Data
{
    public static class SeedData
    {

        public static async Task Initialize(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AttendanceSystemContext context)
        {
            //Look for any users
            if (userManager.Users.Any())
            {
                return;   // DB has been seeded
            }

            var user = new ApplicationUser
            {
                Email = "dahan@remal.com",
                UserName = "dahan@remal.com",
                FirstName = "Ali",
                LastName = "AlDahan",
                Salary = 6000,
                VacationBalance = 3,
                YearlyIncrement = 25,
                WorkStartTime = new TimeSpan(9, 0, 0),
                WorkEndTime = new TimeSpan(16, 0, 0),
                GracePeriod = new TimeSpan(0, 15, 0)
            };

            await userManager.CreateAsync(user, "112233");
            await roleManager.CreateAsync(new IdentityRole(ApplicationUser.AdminRole));
            await userManager.AddToRoleAsync(user, ApplicationUser.AdminRole);

            // seeding rules 
            await context.Rules.AddRangeAsync(new List<Rule>
            {
                new Rule(OffenseDegree.LeaveEarly, Occurrence.First, 0),
                new Rule(OffenseDegree.LeaveEarly, Occurrence.Second, 0),
                new Rule(OffenseDegree.LeaveEarly, Occurrence.Third, 10),
                new Rule(OffenseDegree.LeaveEarly, Occurrence.Forth, 20),
                new Rule(OffenseDegree.LeaveEarly, Occurrence.FifthOrMore, 30),

                new Rule(OffenseDegree.LateLessThanHourWithMakeUp, Occurrence.First, 0),
                new Rule(OffenseDegree.LateLessThanHourWithMakeUp, Occurrence.Second, 0),
                new Rule(OffenseDegree.LateLessThanHourWithMakeUp, Occurrence.Third, 0),
                new Rule(OffenseDegree.LateLessThanHourWithMakeUp, Occurrence.Forth, 5),
                new Rule(OffenseDegree.LateLessThanHourWithMakeUp, Occurrence.FifthOrMore, 10),

                new Rule(OffenseDegree.LateLessThanHourNoMakeUp, Occurrence.First, 0),
                new Rule(OffenseDegree.LateLessThanHourNoMakeUp, Occurrence.Second, 10),
                new Rule(OffenseDegree.LateLessThanHourNoMakeUp, Occurrence.Third, 20),
                new Rule(OffenseDegree.LateLessThanHourNoMakeUp, Occurrence.Forth, 30),
                new Rule(OffenseDegree.LateLessThanHourNoMakeUp, Occurrence.FifthOrMore, 40),

                new Rule(OffenseDegree.LateMoreThanHourWithMakeUp, Occurrence.First, 10),
                new Rule(OffenseDegree.LateMoreThanHourWithMakeUp, Occurrence.Second, 20),
                new Rule(OffenseDegree.LateMoreThanHourWithMakeUp, Occurrence.Third, 30),
                new Rule(OffenseDegree.LateMoreThanHourWithMakeUp, Occurrence.Forth, 40),
                new Rule(OffenseDegree.LateMoreThanHourWithMakeUp, Occurrence.FifthOrMore, 50),
                
                new Rule(OffenseDegree.LateMoreThanHourNoMakeUp, Occurrence.First, 20),
                new Rule(OffenseDegree.LateMoreThanHourNoMakeUp, Occurrence.Second, 40),
                new Rule(OffenseDegree.LateMoreThanHourNoMakeUp, Occurrence.Third, 60),
                new Rule(OffenseDegree.LateMoreThanHourNoMakeUp, Occurrence.Forth, 80),
                new Rule(OffenseDegree.LateMoreThanHourNoMakeUp, Occurrence.FifthOrMore, 100),
                
                new Rule(OffenseDegree.Absence, Occurrence.First, 100),
                new Rule(OffenseDegree.Absence, Occurrence.Second, 200),
                new Rule(OffenseDegree.Absence, Occurrence.Third, 300),
                new Rule(OffenseDegree.Absence, Occurrence.Forth, 400),
                new Rule(OffenseDegree.Absence, Occurrence.FifthOrMore, 400)
            });

            // Monthly allowance rules seeds
            await context.MonthlyAllowanceRules.AddRangeAsync(new List<MonthlyAllowanceRule> {
                new MonthlyAllowanceRule(OffenseDegree.LeaveEarly, 0),
                new MonthlyAllowanceRule(OffenseDegree.LateLessThanHourWithMakeUp, 5),
                new MonthlyAllowanceRule(OffenseDegree.LateLessThanHourNoMakeUp, 0),
                new MonthlyAllowanceRule(OffenseDegree.LateMoreThanHourWithMakeUp, 3),
                new MonthlyAllowanceRule(OffenseDegree.LateMoreThanHourNoMakeUp, 0),
                new MonthlyAllowanceRule(OffenseDegree.Absence, 0)
            });

            await context.SaveChangesAsync();
        }
    }
}