namespace OoBDev.System.Security.Claims;

/// <summary>
/// Contains constants for common claims used in authentication.
/// </summary>
public static class CommonClaims
{
    /// <summary>
    /// Represents the claim for user ID.
    /// </summary>
    public const string UserId = "app__user_id";

    /// <summary>
    /// Represents the claim for user culture.
    /// </summary>
    public const string Culture = "app__user_culture";

    /// <summary>
    /// Represents the claim for extended properties.
    /// </summary>
    public const string ExtendedProperties = "app__extended_property";

    /// <summary>
    /// Represents the claim for application rights.
    /// </summary>
    public const string ApplicationRight = "app__application_right";

    /// <summary>
    /// Represents the claim for object ID.
    /// </summary>
    public const string ObjectId = "objectid";

    /// <summary>
    /// Represents the claim for object identifier.
    /// </summary>
    public const string ObjectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    /// <summary>
    /// Represents the claim for name identifier.
    /// </summary>
    public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
}
