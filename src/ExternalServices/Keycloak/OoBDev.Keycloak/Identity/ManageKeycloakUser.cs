using OoBDev.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Keycloak.Identity;

/// <summary>
/// The Keycloak identity manager. Lookup is supported through the read-only admin directory; creating and removing users is not
/// (the example identity screen is read only, owner 2026-11-10).
/// </summary>
/// <param name="directory">The read-only admin API reader.</param>
public class ManageKeycloakUser(KeycloakDirectoryReader directory) : IIdentityManager
{
    /// <summary>Not supported: the Keycloak adapter is read only.</summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="lastName">The last name of the user.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="NotSupportedException">Always.</exception>
    public Task<(string objectId, string? password)> CreateIdentityUserAsync(string email, string firstName, string lastName) =>
        throw new NotSupportedException();

    /// <summary>Finds the users whose email address matches exactly.</summary>
    /// <param name="emailAddress">The email address to search for.</param>
    /// <returns>The matching users, or <c>null</c> when none match.</returns>
    public async Task<IList<UserIdentityModel>?> GetIdentityUsersByEmail(string emailAddress)
    {
        var users = await directory.ListUsersAsync(email: emailAddress).ConfigureAwait(false);
        return users.Count == 0
            ? null
            : [.. users.Select(u => new UserIdentityModel
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                EmailAddress = u.Email,
                UserName = u.UserName,
            })];
    }

    /// <summary>Not supported: the Keycloak adapter is read only.</summary>
    /// <param name="userId">The object ID of the user to remove.</param>
    /// <returns>Never returns.</returns>
    /// <exception cref="NotSupportedException">Always.</exception>
    public Task<bool> RemoveIdentityUserAsync(string userId) =>
        throw new NotSupportedException();
}
