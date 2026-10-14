using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OoBDev.System.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OoBDev.SemanticKernel;

/// <summary>
/// Semantic Kernel plugin that provides time-related functions.
/// </summary>
public partial class TimePlugIn : IKernelPlugIn
{
    private readonly ILogger _logger;
    private readonly IDataConverter _converter;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimePlugIn"/> class.
    /// </summary>
    /// <param name="logger">The logger for plugin operations.</param>
    /// <param name="converter">The data converter for parsing time values.</param>
    /// <param name="timeProvider">The clock; defaults to <see cref="TimeProvider.System"/>.</param>
    public TimePlugIn(
        ILogger<TimePlugIn> logger,
        IDataConverter converter,
        TimeProvider? timeProvider = null
        )
    {
        _logger = logger;
        _converter = converter;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Gets the current local time.
    /// </summary>
    /// <returns>The current time as a <see cref="DateTimeOffset"/>.</returns>
    [KernelFunction("current_time")]
    [Description("get the current time")]
    [return: Description("current time")]
    public Task<DateTimeOffset> GetCurrentTime()
    {
        LogCalled(nameof(GetCurrentTime));
        var now = _timeProvider.GetLocalNow();
        LogNow(now);
        return Task.FromResult(now);
    }

    /// <summary>
    /// Gets the current UTC time adjusted by a specified time offset.
    /// </summary>
    /// <param name="timeOffset">The UTC time offset as a .NET TimeSpan string.</param>
    /// <returns>The adjusted current time as a <see cref="DateTimeOffset"/>.</returns>
    [KernelFunction("adjusted_time")]
    [Description("current time by timezone")]
    [return: Description("current time")]
    public Task<DateTimeOffset> AdjustCurrentTime(
        [Description("UTC time offset as dotnet TimeSpan string")] string timeOffset
        )
    {
        LogCalledWithOffset(nameof(AdjustCurrentTime), timeOffset);

        var timeOffsetValue = _converter.ConvertTo<TimeSpan>(timeOffset);

        var now = _timeProvider.GetUtcNow().Add(timeOffsetValue);
        LogNowWithOffset(now, timeOffset);
        return Task.FromResult(now);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Called: {method}")]
    private partial void LogCalled(string method);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Current time: {now}")]
    private partial void LogNow(DateTimeOffset now);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Called: {method}({timeOffset})")]
    private partial void LogCalledWithOffset(string method, string timeOffset);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Current time: {now} ({timeOffset})")]
    private partial void LogNowWithOffset(DateTimeOffset now, string timeOffset);
}

