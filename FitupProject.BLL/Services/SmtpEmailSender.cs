using System.Net;
using System.Net.Mail;
using FitupProject.BLL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FitupProject.BLL.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _cfg["Smtp:Host"]!;
            var port = int.Parse(_cfg["Smtp:Port"]!);
            var user = _cfg["Smtp:User"]!;
            var pass = _cfg["Smtp:Pass"]!;
            var from = _cfg["Smtp:From"]!;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, pass)
            };

            using var mail = new MailMessage(from, toEmail, subject, htmlBody)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }
}
