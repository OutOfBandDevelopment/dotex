using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// One connection in a <see cref="ProjectFile"/>: a name plus the same connection-relevant JSON a saved profile holds
/// (transport settings and presenter choices), and optionally the tab's <see cref="History"/> (most recent first) and
/// <see cref="Log"/> setting (a <c>--log</c> value: <c>true</c> for an automatic file, or a path).
/// </summary>
public sealed record ProjectConnection(string Name, string ProfileJson, IReadOnlyList<string>? History = null, string? Log = null)
{
    /// <summary>Binds <see cref="ProfileJson"/> (and <see cref="Log"/>) onto a fresh <see cref="CliOptions"/> through the same path every profile uses.</summary>
    public CliOptions ToOptions()
    {
        var configuration = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(ProfileJson))).Build();
        var options = new CliOptions();
        DevTermConfiguration.Bind(configuration, options);
        if (Log is { Length: > 0 })
        {
            options.Log = Log;
        }

        return options;
    }
}
