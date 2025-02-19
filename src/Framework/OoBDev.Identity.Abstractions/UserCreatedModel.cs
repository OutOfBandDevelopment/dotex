namespace OoBDev.Identity;

/// <summary>
/// Represents the model for a user created as a result of account creation.
/// </summary>
public record UserCreatedModel
{
    /// <summary>
    /// Gets or sets the username associated with the created user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the password associated with the created user.
    /// </summary>
    public required string Password { get; set; }
}
