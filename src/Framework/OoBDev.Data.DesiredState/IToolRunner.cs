using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>Runs an external tool; abstracted so plans can be tested without the binary.</summary>
public interface IToolRunner
{
    Task<ToolResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string standardInput, IReadOnlyDictionary<string, string> environment, CancellationToken cancellationToken = default);
}
