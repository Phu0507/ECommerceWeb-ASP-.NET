using System.Net;
using System.Net.Mail;

namespace Shopping_web.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("ddaothanhphu36@gmail.com", "xxgclnryxdiielid")
            };
            return client.SendMailAsync(
                new MailMessage(from: "ddaothanhphu36@gmail.com", to: email, subject, htmlMessage)
            );
        }
    }
}
