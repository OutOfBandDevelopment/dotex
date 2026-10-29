namespace OoBDev.DevTerm;

/// <summary>The default <see cref="ISessionFactory"/>.</summary>
public sealed class SessionFactory : ISessionFactory
{
    /// <inheritdoc />
    public Session Create(ITransport transport, Pipeline pipeline) => new(transport, pipeline);
}
