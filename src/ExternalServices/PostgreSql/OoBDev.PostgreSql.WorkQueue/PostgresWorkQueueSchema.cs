using System.Globalization;

namespace OoBDev.PostgreSql.WorkQueue;

/// <summary>
/// Generates the migration script of the work queue tables from the configured names.
/// </summary>
public static class PostgresWorkQueueSchema
{
    /// <summary>The script that creates the schema, both tables and their indexes; it can be run repeatedly.</summary>
    /// <param name="options">The names to use.</param>
    /// <returns>The SQL text.</returns>
    public static string CreateScript(PostgresWorkQueueOptions options)
    {
        var schema = Quote(options.Schema);
        var tasks = $"{schema}.{Quote(options.TasksTable)}";
        var dead = $"{schema}.{Quote(options.DeadLettersTable)}";
        var events = $"{schema}.{Quote(options.EventsTable)}";
        var readers = $"{schema}.{Quote(options.EventReadersTable)}";
        return string.Format(CultureInfo.InvariantCulture, """
            CREATE SCHEMA IF NOT EXISTS {0};

            CREATE TABLE IF NOT EXISTS {1} (
                stage           text        NOT NULL,
                item_key        text        NOT NULL,
                status          text        NOT NULL DEFAULT 'pending',
                attempts        integer     NOT NULL DEFAULT 0,
                claimed_by      text        NULL,
                claimed_at      timestamptz NULL,
                lease_until     timestamptz NULL,
                next_attempt_at timestamptz NULL,
                detail          text        NULL,
                created         timestamptz NOT NULL,
                updated         timestamptz NOT NULL,
                PRIMARY KEY (stage, item_key)
            );

            CREATE INDEX IF NOT EXISTS {2} ON {1} (stage, status, next_attempt_at);

            CREATE TABLE IF NOT EXISTS {3} (
                id          bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                stage       text        NOT NULL,
                item_key    text        NOT NULL,
                attempts    integer     NOT NULL,
                reason      text        NOT NULL,
                last_error  text        NULL,
                claimed_by  text        NULL,
                died_at     timestamptz NOT NULL,
                redriven_at timestamptz NULL
            );

            CREATE INDEX IF NOT EXISTS {4} ON {3} (stage, died_at DESC);

            CREATE TABLE IF NOT EXISTS {5} (
                id      bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                at      timestamptz NOT NULL,
                source  text        NOT NULL,
                message text        NOT NULL
            );

            CREATE TABLE IF NOT EXISTS {6} (
                reader  text        PRIMARY KEY,
                last_id bigint      NOT NULL,
                updated timestamptz NOT NULL
            );
            """,
            schema,
            tasks,
            Quote($"ix_{options.TasksTable}_claim"),
            dead,
            Quote($"ix_{options.DeadLettersTable}_stage"),
            events,
            readers);
    }

    internal static string Quote(string identifier) => $"\"{identifier}\"";
}
