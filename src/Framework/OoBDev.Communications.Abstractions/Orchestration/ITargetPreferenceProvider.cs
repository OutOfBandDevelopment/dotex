using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Looks up how a target wants to be reached.</summary>
public interface ITargetPreferenceProvider
{
    /// <summary>Gets the preference of <paramref name="targetId"/>, or <c>null</c> when the target is unknown.</summary>
    Task<TargetPreference?> GetAsync(string targetId, CancellationToken cancellationToken = default);
}
