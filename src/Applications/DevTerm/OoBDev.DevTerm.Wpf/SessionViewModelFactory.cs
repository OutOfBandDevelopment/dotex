using System.Collections.Generic;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Loopback;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Builds sessions from the registered adapters and presenters, or from saved connection profiles.</summary>
/// <param name="loopbackOptions">The loopback adapter options.</param>
/// <param name="sessions">The session factory.</param>
/// <param name="presenters">The presenter catalog.</param>
/// <param name="profiles">The saved connection profiles.</param>
/// <param name="logDialog">Asks for log files and notes, or null.</param>
public sealed class SessionViewModelFactory(IOptions<LoopbackTransportOptions> loopbackOptions, ISessionFactory sessions, PresenterCatalog presenters, ConnectionProfileStore profiles, ISessionLogDialog? logDialog = null) : ISessionViewModelFactory
{
    private int _count;

    /// <inheritdoc />
    public SessionViewModel CreateLoopback()
    {
        var session = sessions.Create(new LoopbackTransport(loopbackOptions), new Pipeline([presenters.Get("ascii")]));
        return new SessionViewModel(session, $"Loopback {++_count}", logDialog: logDialog);
    }

    /// <inheritdoc />
    public ConnectionProfileStore ProfileStore => profiles;

    /// <inheritdoc />
    public IReadOnlyList<string> ListProfiles() => profiles.List();

    /// <inheritdoc />
    public SessionViewModel CreateFromProfile(string name) => CreateFromOptions(profiles.Load(name), name);

    /// <inheritdoc />
    public SessionViewModel CreateFromOptions(CliOptions options, string title)
    {
        var built = DevTermSessionBuilder.Build(options);
        return new SessionViewModel(built.Session, title, new SessionTab(built.Session, built.Catalog, options, built.Services), logDialog);
    }

    /// <inheritdoc />
    public ConnectionEditorViewModel CreateEditor() => new(profiles, new CliOptions());
}
