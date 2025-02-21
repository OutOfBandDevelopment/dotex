using Azure.Identity;
using OoBDev.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.B2C.Identity;

/// <summary>
/// Implementation of  <see cref="IIdentityManager"/> for managing users in Microsoft Graph.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ManageGraphUser"/> class.
/// </remarks>
/// <param name="log">The logger.</param>
/// <param name="config">The configuration.</param>
public class ManageGraphUser(
    ILogger<ManageGraphUser> log,
    IOptions<MicrosoftIdentityOptions> config
        ) : IIdentityManager
{
    private readonly ILogger _log = log;
    private readonly IOptions<MicrosoftIdentityOptions> _config = config;

    /// <summary>
    /// Creates a new identity user asynchronously with the specified email, first name, and last name.
    /// </summary>
    /// <param name="email">The email address for the new user.</param>
    /// <param name="firstName">The first name for the new user.</param>
    /// <param name="lastName">The last name for the new user.</param>
    /// <returns>A tuple containing the object ID and password of the created user, or null if the user already exists.</returns>
    public async Task<(string objectId, string? password)> CreateIdentityUserAsync(string email, string firstName, string lastName)
    {
        try
        {
            // Set up the Microsoft Graph service client with client credentials
            var graphClient = new GraphServiceClient(GetAuthProvider());

            // Query existing users by email, iff it already exists, return that id.
            var existingUsers = await graphClient.Users.GetAsync(user =>
            {
                user.QueryParameters.Select = ["Id", "Mail"];
                user.QueryParameters.Top = 1;
                user.QueryParameters.Filter = $"mail eq '{email}'";
            });

            var existingId = existingUsers?.Value?.FirstOrDefault()?.Id;

            if (!string.IsNullOrWhiteSpace(existingId))
            {
                _log.LogInformation($"Exists: {{{nameof(email)}}}", email);
                return (existingId, null);
            }

            var password = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            _log.LogInformation($"Creating: {{{nameof(email)}}}", email);
            var graphUser = await graphClient.Users.PostAsync(new User
            {
                GivenName = firstName,
                Surname = lastName,
                DisplayName = $"{firstName} {lastName}",
                Mail = email,
                Identities =
                [
                    new ObjectIdentity()
                    {
                        SignInType = "emailAddress",
                        Issuer = $"{_config.Value.Tenant}.onmicrosoft.com",
                        IssuerAssignedId = email,
                    }
                ],
                PasswordProfile = new PasswordProfile()
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = true
                },
                PasswordPolicies = "DisablePasswordExpiration",
            });

            if (graphUser?.Id == null)
                throw new ApplicationException($"Unable to create user for {email}");

            _log.LogInformation($"Created: {{{nameof(email)}}}", email);
            return (graphUser.Id, password);
        }
        catch (Exception ex)
        {
            _log.LogError($"Error: {{{nameof(ex.Message)}}}", ex.Message);
            _log.LogDebug($"Error: {{{nameof(Exception)}}}", ex);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a list of user identity models based on the provided email address.
    /// </summary>
    /// <param name="emailAddress">The email address to query users by.</param>
    /// <returns>A list of user identity models matching the specified email address, or null if no matches are found.</returns>
    public async Task<List<UserIdentityModel>?> GetIdentityUsersByEmail(string emailAddress)
    {
        //Set up the Microsoft Graph service client with client credentials
        var graphClient = new GraphServiceClient(GetAuthProvider());

        // Query existing users by email, iff it already exists, return that id.
        var existingUsers = await graphClient.Users.GetAsync(user =>
        {
            user.QueryParameters.Select = [
                nameof(User.Id),
                nameof(User.Mail),
                nameof(User.GivenName),
                nameof(User.Surname),
                "PasswordProfile.ForceChangePasswordNextSignIn",
            ];
            user.QueryParameters.Top = 1;
            user.QueryParameters.Filter = $"mail eq '{emailAddress}'";
        });

        return existingUsers?.Value?.Select(r => new UserIdentityModel()
        {
            UserName = r.Id,
            FirstName = r.GivenName,
            LastName = r.Surname,
            EmailAddress = r.Mail,
            ForcePasswordChangeNextSignIn = r.PasswordProfile?.ForceChangePasswordNextSignIn ?? false
        })?.ToList() ?? [];
    }

    /// <summary>
    /// Removes an identity user asynchronously based on the specified object ID.
    /// </summary>
    /// <param name="userId">The object ID of the user to be removed.</param>
    /// <returns>True if the user is successfully removed, false otherwise.</returns>
    public async Task<bool> RemoveIdentityUserAsync(string userId)
    {
        try
        {
            // Set up the Microsoft Graph service client with client credentials
            var graphClient = new GraphServiceClient(GetAuthProvider());

            var existingUsers = await graphClient.DirectoryObjects.GetByIds.PostAsGetByIdsPostResponseAsync(
                new global::Microsoft.Graph.DirectoryObjects.GetByIds.GetByIdsPostRequestBody
                {
                    Ids = [userId],
                });

            if (existingUsers?.Value?.Count > 0)
            {
                _log.LogInformation($"Deleting: {{{nameof(userId)}}}", userId);
                await graphClient.Users[userId].DeleteAsync();
                _log.LogInformation($"Deleted: {{{nameof(userId)}}}", userId);
            }
            else
            {
                _log.LogInformation($"Does not exist: {{{nameof(userId)}}}", userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _log.LogError($"Error: {{{nameof(ex.Message)}}}", ex.Message);
            _log.LogDebug($"Error: {{{nameof(Exception)}}}", ex);
            return false;
        }
    }

    /// <summary>
    /// Gets the authentication provider for Microsoft Graph.
    /// </summary>
    /// <returns>The authentication provider.</returns>
    private ClientSecretCredential GetAuthProvider()
    {
        var config = new
        {
            clientId = _config.Value.ClientID,
            tenantId = _config.Value.Issuer,
            clientSecret = _config.Value.ClientSecret,
        };

        var token = new ClientSecretCredential(config.tenantId, config.clientId, config.clientSecret);

        return token;
    }
}
