namespace AttendanceSystem.ViewModels
{
    public class UserOffensesReport
    {
        public string UserName { get; set; } 
        public int Absences { get; set; } 
        public int Lates { get; set; } 
        public int MakeUps { get; set; } 
        public int PenaltyPercent { get; set; } 
        public int PenaltyAmount { get; set; } 

        public UserOffensesReport() {}

        public UserOffensesReport(string userName, int absences, int lates, int makeUps, int penaltyPercent, int penaltyAmount)
        {
            UserName = userName;
            Absences = absences;
            Lates = lates;
            MakeUps = makeUps;
            PenaltyPercent = penaltyPercent;
            PenaltyAmount = penaltyAmount;
        }
    }
}
