using System.Threading;
using System.Threading.Tasks;

namespace BotChat;

public interface IRunner
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
