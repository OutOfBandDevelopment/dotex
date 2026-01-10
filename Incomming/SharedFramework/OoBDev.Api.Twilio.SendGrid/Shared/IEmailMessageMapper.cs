using OoBDev.Communications.Contracts.Channels;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace OoBDev.Api.Twilio.SendGrid.Shared
{
    public interface IEmailMessageMapper
    {
        Task<SendGridMessage> GetMessageAsync(IEmailMessage message);
    }
}
