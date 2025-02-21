using OoBDev.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Keycloak.Identity;

/// <summary>
/// Manages user operations in Keycloak for both graph and identity aspects.
/// </summary>
/// <param name="log">The logger for logging purposes.</param>
/// <param name="config">The configuration options for Keycloak identity.</param>
public class ManageKeycloakUser(
    ILogger<ManageKeycloakUser> log,
    IOptions<KeycloakIdentityOptions> config
        ) : IIdentityManager
{
    private readonly ILogger _log = log;
    private readonly IOptions<KeycloakIdentityOptions> config = config;

    /// <summary>
    /// Creates a new identity user asynchronously with the specified details.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="lastName">The last name of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the object ID and an optional password for the created user.</returns>
    public Task<(string objectId, string? password)> CreateIdentityUserAsync(string email, string firstName, string lastName) =>
        throw new NotImplementedException();

    /// <summary>
    /// Retrieves a list of user identity models based on the specified email address.
    /// </summary>
    /// <param name="emailAddress">The email address to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of user identity models or <c>null</c> if no users are found.</returns>
    public Task<List<UserIdentityModel>?> GetIdentityUsersByEmail(string emailAddress) =>
           throw new NotImplementedException();

    /// <summary>
    /// Removes an identity user asynchronously based on the specified object ID.
    /// </summary>
    /// <param name="userId">The object ID of the user to remove.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the user was successfully removed; otherwise, <c>false</c>.</returns>
    public Task<bool> RemoveIdentityUserAsync(string userId) =>
        throw new NotImplementedException();
}
