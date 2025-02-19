namespace OoBDev.Identity;

/// <summary>
/// Represents a model for user identity information.
/// </summary>
public record UserIdentityModel
{
    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the password of the user.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user should be forced to change their password at the next sign-in.
    /// </summary>
    public bool? ForcePasswordChangeNextSignIn { get; set; }
}
