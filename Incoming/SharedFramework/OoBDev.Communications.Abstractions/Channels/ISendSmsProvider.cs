using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.Channels
{
    public interface ISendSmsProvider
    {
        Task<string?> ScheduleSendMessageAsync(ISmsMessage message, string? messageId = null);
        Task SendMessageAsync(ISmsMessage message);
    }
}
