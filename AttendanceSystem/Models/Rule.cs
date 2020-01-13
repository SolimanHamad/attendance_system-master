using System.ComponentModel.DataAnnotations;
using AttendanceSystem.Models.Enums;

namespace AttendanceSystem.Models
{
    public class Rule
    {
        public Rule() {}

        public Rule(OffenseDegree offenseDegree, Occurrence occurrence, int penaltyPercent)
        {
            OffenseDegree = offenseDegree;
            Occurrence = occurrence;
            PenaltyPercent = penaltyPercent;
        }

        [Key]
        public string ID { get; set; }
        [Required]
        public OffenseDegree OffenseDegree { get; set; }
        [Required]
        public Occurrence Occurrence { get; set; }
        [Required]
        public int PenaltyPercent { get; set; }

    }

    public enum Occurrence
    {
        First = 1,
        Second = 2,
        Third = 3,
        Forth = 4,
        FifthOrMore = 5
    }
}
