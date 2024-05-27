using System.Xml.Schema;

namespace OoBDev.System.Xml.Schema;

public class XmlValidationResult
{
    public XmlSchemaException Exception { get; init; }
    public string Message { get; init; }
    public XmlSeverityType Severity { get; init; }
}
