using OoBDev.Communications.Contracts.Channels;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.Handler
{
    public interface ISendEmailHandler
    {
        Task SendMessageAsync(IEmailMessage message);
    }
}
