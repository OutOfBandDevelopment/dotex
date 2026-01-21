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
public class TimePlugIn : IKernelPlugIn
{
    private readonly ILogger _logger;
    private readonly IDataConverter _converter;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimePlugIn"/> class.
    /// </summary>
    /// <param name="logger">The logger for plugin operations.</param>
    /// <param name="converter">The data converter for parsing time values.</param>
    public TimePlugIn(
        ILogger<TimePlugIn> logger,
        IDataConverter converter
        )
    {
        _logger = logger;
        _converter = converter;
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
        _logger.LogInformation("Called: {method}", nameof(GetCurrentTime));
        var now = DateTimeOffset.Now;
        _logger.LogDebug("Current time: {now}", now);
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
        _logger.LogInformation("Called: {method}({timeOffset})", nameof(AdjustCurrentTime), timeOffset);

        var timeOffsetValue = _converter.ConvertTo<TimeSpan>(timeOffset);

        var now = DateTimeOffset.UtcNow.Add(timeOffsetValue);
        _logger.LogDebug("Current time: {now} ({timeOffset})", now, timeOffset);
        return Task.FromResult(now);
    }
}

