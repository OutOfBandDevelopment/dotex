using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace OoBDev.Api.Twilio.SendGrid.Shared
{
    public interface IEmailClient
    {
        Task<Response> SendMessageAsync(SendGridMessage message);
    }
}
