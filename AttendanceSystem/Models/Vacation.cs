using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.Models
{
    public class Vacation
    {
        [Required]
        public string ID { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [DefaultValue(RequestStatus.Pending)]
        public RequestStatus Status { get; set; }

        [DefaultValue(VacationType.Personal)]
        public VacationType VacationType { get; set; }

        public string Comment{ get; set; }

        [Required]
        public string UserID { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
