using OoBDev.System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
#if DEBUG 
using Microsoft.IdentityModel.Logging;
#endif
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Authorization;

/// <summary>
/// Handles user authorization based on specified requirements.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserAuthorizationHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
public class UserAuthorizationHandler(ILogger<UserAuthorizationHandler> logger) : AuthorizationHandler<UserAuthorizationRequirement>
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Handles the user authorization requirement asynchronously.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The user authorization requirement.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserAuthorizationRequirement requirement)
    {
#if DEBUG //TODO: consider adding the ability to enable this with an environment variable so it can be used outside of local development
        IdentityModelEventSource.ShowPII = true;
#endif
        var user = context.User;

        // These should be provided by the authentication provider
        var userName = user.GetClaimValue(CommonClaims.ObjectId, CommonClaims.ObjectIdentifier, CommonClaims.NameIdentifier)?.value;

        // If the application has extended the user id claim, it should be provided as well
        var userId = user.GetClaimValue(CommonClaims.UserId)?.value; //TODO: maybe this should be injected in a different way such as making this a configurable claim and if not configured then not required.

        var isAuthorized = !string.IsNullOrWhiteSpace(userName);

        if (isAuthorized && requirement.RequireApplicationUserId)
        {
            isAuthorized &= !string.IsNullOrWhiteSpace(userId);
        }

        if (isAuthorized)
        {
            _logger.LogInformation($"{{{nameof(userName)}}} is authorized ({{{nameof(userId)}}})", userName, userId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning($"{{{nameof(userName)}}} is not authorized ({{{nameof(userId)}}})", userName, userId);
            context.Fail(new AuthorizationFailureReason(this, "User not authorized"));
        }

        return Task.CompletedTask;
    }
}
