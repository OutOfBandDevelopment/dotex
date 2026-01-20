using OoBDev.Communications.Contracts.Channels;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace OoBDev.Twilio.SendGrid.Shared
{
    public interface IEmailMessageMapper
    {
        Task<SendGridMessage> GetMessageAsync(IEmailMessage message);
    }
}
