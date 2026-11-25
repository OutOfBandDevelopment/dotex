using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Compiles a <see cref="FeatureManifest"/> to idempotent ensure scripts (<c>001_extensions.sql</c>, <c>020_queues.sql</c>, <c>030_streams.sql</c>)
/// for <see cref="SqldefProvider.Postgres"/>, <see cref="SqldefProvider.SqlServer"/> and <see cref="SqldefProvider.Sqlite"/>.
/// Queues and streams are portable tables (<c>queue_{name}</c>, <c>stream_{name}</c>).
/// </summary>
public static partial class ManifestCompiler
{
    public static DesiredSchema Compile(FeatureManifest manifest, string engine)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(engine);
        engine = engine.ToLowerInvariant();
        if (engine is not (SqldefProvider.Postgres or SqldefProvider.SqlServer or SqldefProvider.Sqlite))
        {
            throw new NotSupportedException($"The manifest compiler does not support '{engine}'; write its ensure scripts by hand.");
        }

        var scripts = new List<DesiredScript>();
        if (manifest.Extensions.Count > 0)
        {
            if (engine != SqldefProvider.Postgres)
            {
                throw new NotSupportedException($"Extensions exist only for postgres, not '{engine}'.");
            }

            scripts.Add(new DesiredScript("001_extensions.sql", Join(manifest.Extensions.Select(e => $"CREATE EXTENSION IF NOT EXISTS {Identifier(e, "extension")};"))));
        }

        if (manifest.Queues.Count > 0)
        {
            scripts.Add(new DesiredScript("020_queues.sql", Join(manifest.Queues.Select(q => QueueTable(engine, Identifier(q, "queue"))))));
        }

        if (manifest.Streams.Count > 0)
        {
            scripts.Add(new DesiredScript("030_streams.sql", Join(manifest.Streams.Select(s => StreamTable(engine, Identifier(s, "stream"))))));
        }

        return new DesiredSchema(scripts);
    }

    private static string Join(IEnumerable<string> statements) => string.Join("\n", statements) + "\n";

    private static string Identifier(string name, string kind)
    {
        if (string.IsNullOrEmpty(name) || !IdentifierPattern().IsMatch(name))
        {
            throw new ArgumentException($"'{name}' is not a valid {kind} name (letters, digits and underscore, starting with a letter or underscore, at most 40 characters).", nameof(name));
        }

        return name;
    }

    private static string QueueTable(string engine, string name) => engine switch
    {
        SqldefProvider.Postgres => $"CREATE TABLE IF NOT EXISTS queue_{name} (id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY, payload jsonb NOT NULL, visible_at timestamptz NOT NULL DEFAULT now(), attempts int NOT NULL DEFAULT 0);",
        SqldefProvider.SqlServer => $"IF OBJECT_ID(N'queue_{name}', N'U') IS NULL CREATE TABLE queue_{name} (id bigint IDENTITY(1,1) PRIMARY KEY, payload nvarchar(max) NOT NULL, visible_at datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET(), attempts int NOT NULL DEFAULT 0);",
        _ => $"CREATE TABLE IF NOT EXISTS queue_{name} (id integer PRIMARY KEY AUTOINCREMENT, payload text NOT NULL, visible_at text NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')), attempts integer NOT NULL DEFAULT 0);",
    };

    private static string StreamTable(string engine, string name) => engine switch
    {
        SqldefProvider.Postgres => $"CREATE TABLE IF NOT EXISTS stream_{name} (seq bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY, payload jsonb NOT NULL, created_at timestamptz NOT NULL DEFAULT now());",
        SqldefProvider.SqlServer => $"IF OBJECT_ID(N'stream_{name}', N'U') IS NULL CREATE TABLE stream_{name} (seq bigint IDENTITY(1,1) PRIMARY KEY, payload nvarchar(max) NOT NULL, created_at datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET());",
        _ => $"CREATE TABLE IF NOT EXISTS stream_{name} (seq integer PRIMARY KEY AUTOINCREMENT, payload text NOT NULL, created_at text NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')));",
    };

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]{0,39}$")]
    private static partial Regex IdentifierPattern();
}
