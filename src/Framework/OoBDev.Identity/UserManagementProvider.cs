using System;
using System.Threading.Tasks;

namespace OoBDev.Identity;

/// <summary>
/// Represents a provider for user management operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserManagementProvider"/> class.
/// </remarks>
/// <param name="user">The user manager for managing graph users.</param>
public class UserManagementProvider(
    IIdentityManager? user = null
    ) : IUserManagementProvider
{
    private readonly IIdentityManager? _user = user;

    /// <summary>
    /// Creates a new user account asynchronously.
    /// </summary>
    /// <param name="model">The model containing user information for account creation.</param>
    /// <returns>A task representing the asynchronous operation. The result is a model containing the created user's information.</returns>
    public virtual async Task<UserCreatedModel> CreateAccountAsync(UserCreateModel model)
    {
        if (_user == null) throw new NotSupportedException($"No identity management provider registered");

        var (objectid, password) = await _user.CreateIdentityUserAsync(model.EmailAddress, model.FirstName, model.LastName);
        return new UserCreatedModel
        {
            Password = password ?? "",
            Username = objectid,
        };
    }
}
