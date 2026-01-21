using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OoBDev.SemanticKernel;

/// <summary>
/// Semantic Kernel plugin that provides information about the current user.
/// </summary>
public class CurrentUserPlugIn : IKernelPlugIn
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUserPlugIn"/> class.
    /// </summary>
    /// <param name="logger">The logger for plugin operations.</param>
    public CurrentUserPlugIn(
        ILogger<CurrentUserPlugIn> logger
        )
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the current username.
    /// </summary>
    /// <returns>The current username, or null if unavailable.</returns>
    [KernelFunction("current_user")]
    [Description("get the current username")]
    [return: Description("current user")]
    public Task<string?> WhoAmI()
    {
        _logger.LogInformation("Called: {method}", nameof(WhoAmI));
        return Task.FromResult<string?>(Environment.UserName);
    }
}

