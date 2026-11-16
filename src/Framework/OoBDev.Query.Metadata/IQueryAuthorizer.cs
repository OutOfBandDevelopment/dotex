namespace OoBDev.Query.Metadata;

/// <summary>
/// Answers whether the current caller holds an application right. A kind that names a right is closed to every caller until an authorizer allows it.
/// </summary>
public interface IQueryAuthorizer
{
    /// <summary>Checks a right.</summary>
    /// <param name="right">The right named by <see cref="QueryKindAttribute.Right"/>.</param>
    /// <returns>True when the caller holds it.</returns>
    bool IsAllowed(string right);
}
