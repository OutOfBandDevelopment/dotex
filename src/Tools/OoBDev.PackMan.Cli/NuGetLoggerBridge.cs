using Microsoft.Extensions.Logging;
using NuGet.Common;
using System.Threading.Tasks;

namespace OoBDev.PackMan.Cli;

/// <summary>Forwards NuGet client log messages to a <see cref="Microsoft.Extensions.Logging.ILogger"/>.</summary>
/// <param name="logger">The target logger.</param>
public sealed partial class NuGetLoggerBridge(Microsoft.Extensions.Logging.ILogger logger) : LoggerBase
{
    /// <inheritdoc />
    public override void Log(ILogMessage message)
    {
        var level = Convert(message.Level);
        if (logger.IsEnabled(level))
            Write(logger, level, message.Message);
    }

    /// <inheritdoc />
    public override Task LogAsync(ILogMessage message)
    {
        Log(message);
        return Task.CompletedTask;
    }

    [LoggerMessage(Message = "{Message}")]
    private static partial void Write(Microsoft.Extensions.Logging.ILogger logger, Microsoft.Extensions.Logging.LogLevel level, string message);

    private static Microsoft.Extensions.Logging.LogLevel Convert(NuGet.Common.LogLevel level) =>
        level switch
        {
            NuGet.Common.LogLevel.Verbose => Microsoft.Extensions.Logging.LogLevel.Trace,
            NuGet.Common.LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            NuGet.Common.LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            NuGet.Common.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            NuGet.Common.LogLevel.Minimal => Microsoft.Extensions.Logging.LogLevel.Information,
            _ => Microsoft.Extensions.Logging.LogLevel.Debug,
        };
}
