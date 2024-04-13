namespace TicketsApp.Models
{
    using MailKit.Net.Smtp;
    using MimeKit;
    using System.Threading.Tasks;

    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string htmlMessage, List<MimePart> attachments = null)
        {
            /* var smtpClient = new SmtpClient("smtp.gmail.com")
             {
                 //Port = 587,
                 Port= 465,
                 Credentials = new NetworkCredential("ticketsappnoreply@gmail.com", "ticketsappnoreply123"),
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

             await smtpClient.SendMailAsync(mailMessage);*/

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse("ticketsappnoreply@gmail.com"));
            emailMessage.To.Add(MailboxAddress.Parse(to));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    bodyBuilder.Attachments.Add(attachment);
                }
            }
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
