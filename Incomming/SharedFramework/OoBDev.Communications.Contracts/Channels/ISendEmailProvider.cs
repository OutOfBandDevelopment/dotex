using System.Threading.Tasks;

namespace OoBDev.Communications.Contracts.Channels
{
    public interface ISendEmailProvider
    {
        Task<string?> ScheduleSendMessageAsync(IEmailMessage message, string? messageId = null);
        Task SendMessageAsync(IEmailMessage message);
    }
}
