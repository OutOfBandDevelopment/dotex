using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OoBDev.SemanticKernel;

public class CurrentUserPlugIn : IKernelPlugIn
{
    private readonly ILogger _logger;

    public CurrentUserPlugIn(
        ILogger<CurrentUserPlugIn> logger
        )
    {
        _logger = logger;
    }
    [KernelFunction("current_user")]
    [Description("get the current username")]
    [return: Description("current user")]
    public Task<string?> WhoAmI()
    {
        _logger.LogInformation("Called: {method}", nameof(WhoAmI));
        return Task.FromResult(Environment.UserName);
    }
}

