namespace AttendanceSystem.Services
{
    public interface IEmailService
    {
        void SendEmail(string recipient, string title, string subTitle, string message, string actionName ="", string actionUrl = "");
    }
}
