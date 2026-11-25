using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>Runs one SQL script against a database (a driver-specific adapter).</summary>
public interface IScriptExecutor
{
    Task ExecuteAsync(DatabaseTarget target, string scriptName, string scriptText, CancellationToken cancellationToken = default);
}
