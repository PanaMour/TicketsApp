namespace TicketsApp.Models
{
    using MailKit.Net.Smtp;
    using MimeKit;
    using System.Threading.Tasks;

    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("ticketsappnoreply@google.com", "ticketsappnoreply@google.com"));
            emailMessage.To.Add(new MailboxAddress("", to));
            emailMessage.Subject = subject;
            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("ticketsappnoreply@gmail.com", "jbgk nlxx ueib iuxk");
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }

}
