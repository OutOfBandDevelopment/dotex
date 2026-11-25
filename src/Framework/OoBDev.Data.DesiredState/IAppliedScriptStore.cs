using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>Remembers which one-time ensure scripts ran, by name and content hash (a database table in production).</summary>
public interface IAppliedScriptStore
{
    Task<bool> HasAppliedAsync(DatabaseTarget target, string scriptName, string hash, CancellationToken cancellationToken = default);

    Task RecordAsync(DatabaseTarget target, string scriptName, string hash, CancellationToken cancellationToken = default);
}
