using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace OoBDev.Twilio.SendGrid.Shared
{
    public interface IEmailClient
    {
        Task<Response> SendMessageAsync(SendGridMessage message);
    }
}
