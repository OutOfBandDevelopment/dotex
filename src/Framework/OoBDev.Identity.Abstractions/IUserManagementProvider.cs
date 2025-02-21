using System.Threading.Tasks;

namespace OoBDev.Identity;

/// <summary>
/// Provides methods for managing user accounts.
/// </summary>
public interface IUserManagementProvider
{
    /// <summary>
    /// Creates a user account asynchronously based on the provided model.
    /// </summary>
    /// <param name="model">The model containing user account information.</param>
    /// <returns>A task representing the asynchronous operation, containing the created user model.</returns>
    Task<UserCreatedModel> CreateAccountAsync(UserCreateModel model);
}
