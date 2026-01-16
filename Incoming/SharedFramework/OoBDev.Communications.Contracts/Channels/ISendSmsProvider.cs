using System.Threading.Tasks;

namespace OoBDev.Communications.Contracts.Channels
{
    public interface ISendSmsProvider
    {
        Task<string?> ScheduleSendMessageAsync(ISmsMessage message, string? messageId = null);
        Task SendMessageAsync(ISmsMessage message);
    }
}
