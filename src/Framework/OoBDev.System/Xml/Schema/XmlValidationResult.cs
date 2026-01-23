using System.Xml.Schema;

namespace OoBDev.System.Xml.Schema;

/// <summary>
/// Represents the result of an XML schema validation operation, containing validation error or warning information.
/// </summary>
public class XmlValidationResult
{
    /// <summary>
    /// Gets or initializes the exception that occurred during validation, if any.
    /// </summary>
    public XmlSchemaException? Exception { get; init; }

    /// <summary>
    /// Gets or initializes the validation message describing the error or warning.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets or initializes the severity level of the validation result (Error or Warning).
    /// </summary>
    public XmlSeverityType Severity { get; init; }
}
