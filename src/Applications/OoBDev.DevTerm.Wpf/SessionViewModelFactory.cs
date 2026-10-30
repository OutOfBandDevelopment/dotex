using System;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm.Loopback;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Builds sessions from the registered adapters and presenters.</summary>
/// <param name="loopbackOptions">The loopback adapter options.</param>
/// <param name="sessions">The session factory.</param>
/// <param name="presenters">The presenter catalog.</param>
public sealed class SessionViewModelFactory(IOptions<LoopbackTransportOptions> loopbackOptions, ISessionFactory sessions, PresenterCatalog presenters) : ISessionViewModelFactory
{
    private int _count;

    /// <inheritdoc />
    public SessionViewModel CreateLoopback()
    {
        var session = sessions.Create(new LoopbackTransport(loopbackOptions), new Pipeline([presenters.Get("ascii")]));
        return new SessionViewModel(session, $"Loopback {++_count}");
    }
}
