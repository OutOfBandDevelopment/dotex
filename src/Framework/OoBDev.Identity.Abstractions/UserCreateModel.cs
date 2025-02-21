namespace OoBDev.Identity;

/// <summary>
/// Represents a model for creating a user in Microsoft B2C Identity.
/// </summary>
public record UserCreateModel
{
    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public required string EmailAddress { get; set; }
}
