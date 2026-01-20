using OoBDev.Communications.Contracts.Channels;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace OoBDev.Twilio.SmsMessaging.Shared
{
    public interface ISmsClient
    {
        Task<MessageResource> SendMessageAsync(ISmsMessage message);
    }
}
