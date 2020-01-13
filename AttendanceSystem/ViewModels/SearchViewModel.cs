using System;
using System.Collections.Generic;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.ViewModels
{
    public class SearchViewModel
    {
        public string Name { get; set; } = "";
        public DateTime? IssueDate { get; set; } 
        public  DateTime? StartDate { get; set; }
        public  DateTime? EndDate { get; set; }
        public  double TotalDays { get; set; } 
        public  VacationType? VacationType { get; set; } 
        public  RequestStatus? RequestStatus { get; set; }
        public string SortBy { get; set; }
        public List<VacationViewModel> Vacations { get; set; }


    }    
}
