using System.ComponentModel.DataAnnotations;

namespace OoBDev.System.ComponentModel.DataAnnotations;

/// <summary>
/// Specifies that a data field must match the format of a valid ZIP code.
/// Inherits from <see cref="RegularExpressionAttribute"/>.
/// </summary>
public class ZipCodeAttribute : RegularExpressionAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ZipCodeAttribute"/> class
    /// with a predefined regular expression pattern.
    /// </summary>
    /// <remarks>
    /// The regular expression ensures the value is either a 5-digit ZIP code 
    /// or a 9-digit ZIP+4 code (e.g., "12345" or "12345-6789").
    /// </remarks>
    public ZipCodeAttribute() : base(@"^\d{5}(-\d{4})?$") =>
        ErrorMessage = "A valid 5-digit or 9-digit zip-code is required.";
}
