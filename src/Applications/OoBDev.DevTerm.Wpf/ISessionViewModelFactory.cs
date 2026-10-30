using System.Collections.Generic;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Creates the session view models the shell opens.</summary>
public interface ISessionViewModelFactory
{
    /// <summary>Creates a session over the loopback adapter with the ASCII presenter.</summary>
    SessionViewModel CreateLoopback();

    /// <summary>The names of the saved connection profiles.</summary>
    IReadOnlyList<string> ListProfiles();

    /// <summary>Creates a session from a saved connection profile.</summary>
    /// <param name="name">The profile name.</param>
    /// <exception cref="global::System.IO.FileNotFoundException">No profile has that name.</exception>
    SessionViewModel CreateFromProfile(string name);

    /// <summary>Creates a session from connection options.</summary>
    /// <param name="options">The options.</param>
    /// <param name="title">The tab title.</param>
    SessionViewModel CreateFromOptions(CliOptions options, string title);

    /// <summary>Creates a connection editor over the saved profiles.</summary>
    ConnectionEditorViewModel CreateEditor();
}
