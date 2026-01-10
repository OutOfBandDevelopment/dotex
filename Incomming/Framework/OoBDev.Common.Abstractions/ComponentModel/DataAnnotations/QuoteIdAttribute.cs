using System.ComponentModel.DataAnnotations;

namespace OoBDev.Common.ComponentModel.DataAnnotations;

/// <summary>
/// Specifies that a data field must match a specific pattern for Quote IDs.
/// Inherits from <see cref="RegularExpressionAttribute"/>.
/// </summary>
public class QuoteIdAttribute : RegularExpressionAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuoteIdAttribute"/> class
    /// with a predefined regular expression pattern.
    /// </summary>
    /// <remarks>
    /// The regular expression ensures the value consists only of uppercase letters, 
    /// numbers, underscores, or dashes.
    /// </remarks>
    public QuoteIdAttribute() : base("^[A-Z0-9_-]+$") =>
        ErrorMessage = "QuoteID must consist of uppercase letters, numbers, underscores, or dashes.";
}
