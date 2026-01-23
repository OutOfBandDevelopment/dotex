using System.Xml.Schema;

namespace OoBDev.ToolKit.Xml.Schema;

public class XmlValidationResult
{
    public XmlSchemaException Exception { get; init; } = null!;
    public string Message { get; init; } = null!;
    public XmlSeverityType Severity { get; init; }
}
