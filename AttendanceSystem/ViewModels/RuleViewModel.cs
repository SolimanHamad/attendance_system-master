using AttendanceSystem.Models;
using System.Collections.Generic;

namespace AttendanceSystem.ViewModels
{
    public class RuleViewModel
    {
        public List<Rule> Rules { get; set; }
        public List<MonthlyAllowanceRule> Allowances { get; set; }
    }
}
