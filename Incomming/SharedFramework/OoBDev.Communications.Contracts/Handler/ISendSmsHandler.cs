using OoBDev.Communications.Contracts.Channels;
using System.Threading.Tasks;

namespace OoBDev.Communications.Contracts.Handler
{
    public interface ISendSmsHandler
    {
        Task SendMessageAsync(ISmsMessage message);
    }
}
