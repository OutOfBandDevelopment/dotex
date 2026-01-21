namespace OoBDev.System.Security;

/// <summary>
/// Provides access to information about the current user.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Gets the username of the current user.
    /// </summary>
    string? UserName { get; }
}
