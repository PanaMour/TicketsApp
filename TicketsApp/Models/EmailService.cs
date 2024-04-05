namespace TicketsApp.Models
{
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;

    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            var smtpClient = new SmtpClient("smtp.google.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("ticketsappnoreply@gmail.com", "ticketsappnoreply"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("ticketsappnoreply@google.com"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }

}
