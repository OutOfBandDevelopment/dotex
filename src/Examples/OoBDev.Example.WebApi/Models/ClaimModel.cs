namespace OoBDev.WebApi.Models;

/// <summary>
/// Represents a model for a claim.
/// </summary>
public record ClaimModel
{
    /// <summary>
    /// Gets or sets the issuer of the claim.
    /// </summary>
    /// <remarks>This property is required.</remarks>
    public required string Issuer { get; init; }

    /// <summary>
    /// Gets or sets the value of the claim.
    /// </summary>
    /// <remarks>This property is required.</remarks>
    public required string Value { get; init; }

    /// <summary>
    /// Gets or sets the value type of the claim.
    /// </summary>
    /// <remarks>This property is required.</remarks>
    public required string ValueType { get; init; }

    /// <summary>
    /// Gets or sets the type of the claim.
    /// </summary>
    /// <remarks>This property is required.</remarks>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the original issuer of the claim.
    /// </summary>
    /// <remarks>This property is required.</remarks>
    public required string OriginalIssuer { get; init; }

    /// <summary>
    /// Gets or sets the subject name associated with the claim.
    /// </summary>
    /// <remarks>This property is optional.</remarks>
    public string? SubjectName { get; init; }

    /// <summary>
    /// Gets or sets the subject label associated with the claim.
    /// </summary>
    /// <remarks>This property is optional.</remarks>
    public string? SubjectLabel { get; init; }

    /// <summary>
    /// Gets or sets the authentication type of the subject associated with the claim.
    /// </summary>
    /// <remarks>This property is optional.</remarks>
    public string? SubjectAuthenticationType { get; init; }
}
