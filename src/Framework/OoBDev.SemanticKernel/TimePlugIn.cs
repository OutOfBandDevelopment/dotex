using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OoBDev.System.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OoBDev.SemanticKernel;

public class TimePlugIn : IKernelPlugIn
{
    private readonly ILogger _logger;
    private readonly IDataConverter _converter;

    public TimePlugIn(
        ILogger<TimePlugIn> logger,
        IDataConverter converter
        )
    {
        _logger = logger;
        _converter = converter;
    }

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

