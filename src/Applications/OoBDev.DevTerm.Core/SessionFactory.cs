using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
namespace OoBDev.DevTerm;

/// <summary>The default <see cref="ISessionFactory"/>.</summary>
public sealed class SessionFactory : ISessionFactory
{
    /// <inheritdoc />
    public Session Create(IDeviceAdapter transport, Pipeline pipeline) => new(transport, pipeline);
}
