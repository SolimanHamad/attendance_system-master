using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AttendanceSystem.Services
{
    public class SMTPService : IEmailService
    {
        private readonly string host;
        private readonly int port;
        private readonly NetworkCredential credentials;
        private readonly string fromEmail;

        public SMTPService(IConfiguration configuration)
        {
            host = configuration.GetSection("SMTP")["Host"];
            port = int.Parse(configuration.GetSection("SMTP")["Port"]);
            credentials = new NetworkCredential(configuration.GetSection("SMTP")["User"], configuration.GetSection("SMTP")["Password"]);
            fromEmail = configuration.GetSection("SMTP")["Sender"];
        }

        public void SendEmail(string recipient, string subject, string subTitle, string message, string actionName = "", string actionUrl = "")
        {
            using(SmtpClient client = new SmtpClient(host, port) { UseDefaultCredentials = false, Credentials = credentials })
            {
                // Load the HTML template
                StringBuilder builder = new StringBuilder();
                string templateFile = actionName.Length > 0 ? "wwwroot/Templates/ActionTemplate.html" : "wwwroot/Templates/MessageTemplate.html";
                using (StreamReader streamReader = File.OpenText(templateFile))
                {
                    builder.Append(streamReader.ReadToEnd());
                }
                builder.Replace("{{MessageTitle}}", subject);
                builder.Replace("{{MessageSubtitle}}", subTitle);
                builder.Replace("{{MessageText}}", message);
                builder.Replace("{{ActionName}}", actionName);
                builder.Replace("{{ActionUrl}}", actionUrl);

                MailMessage mailMessage = new MailMessage(fromEmail, recipient, subject, builder.ToString())
                {
                    IsBodyHtml = true
                };
                try{client.Send(mailMessage);}
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}