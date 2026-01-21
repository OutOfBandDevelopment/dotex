using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Abstractions
{
    public interface IEventHubSource<TChannel>
    {
        Task SendAsync(
            IEventData item,
            Guid? targetUser = null,
            [CallerMemberName] string? caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string? callerPath = null
            );
    }
}
