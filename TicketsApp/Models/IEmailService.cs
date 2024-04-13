using MimeKit;

namespace TicketsApp.Models
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage, List<MimePart> attachments);
    }
}
