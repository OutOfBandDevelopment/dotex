using OoBDev.System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using System;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Authorization;

/// <summary>
/// Handles user authorization based on specified requirements.
/// </summary>
public partial class UserAuthorizationHandler : AuthorizationHandler<UserAuthorizationRequirement>
{
    /// <summary>
    /// Environment variable that, set to <c>true</c>, shows personal data in identity model logs. Ignored in Production.
    /// </summary>
    public const string ShowPiiVariable = "OOBDEV_SHOW_PII";

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="environment">The host environment.</param>
    public UserAuthorizationHandler(ILogger<UserAuthorizationHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        if (string.Equals(Environment.GetEnvironmentVariable(ShowPiiVariable), "true", StringComparison.OrdinalIgnoreCase))
        {
            if (environment.IsProduction())
            {
                LogShowPiiIgnored(ShowPiiVariable);
            }
            else
            {
                IdentityModelEventSource.ShowPII = true;
            }
        }
    }

    /// <summary>
    /// Handles the user authorization requirement asynchronously.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The user authorization requirement.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserAuthorizationRequirement requirement)
    {
        var user = context.User;

        // These should be provided by the authentication provider
        var userName = user.GetClaimValue(CommonClaims.ObjectId, CommonClaims.ObjectIdentifier, CommonClaims.NameIdentifier)?.value;

        // If the application has extended the user id claim, it should be provided as well
        var userId = requirement.RequireApplicationUserId ? user.GetClaimValue(requirement.ApplicationUserIdClaim!)?.value : null;

        var isAuthorized = !string.IsNullOrWhiteSpace(userName);

        if (isAuthorized && requirement.RequireApplicationUserId)
        {
            isAuthorized &= !string.IsNullOrWhiteSpace(userId);
        }

        if (isAuthorized)
        {
            LogAuthorized(userName, userId);
            context.Succeed(requirement);
        }
        else
        {
            LogNotAuthorized(userName, userId);
            context.Fail(new AuthorizationFailureReason(this, "User not authorized"));
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "{variable} is set but ignored in Production")]
    private partial void LogShowPiiIgnored(string variable);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{userName} is authorized ({userId})")]
    private partial void LogAuthorized(string? userName, string? userId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "{userName} is not authorized ({userId})")]
    private partial void LogNotAuthorized(string? userName, string? userId);
}
