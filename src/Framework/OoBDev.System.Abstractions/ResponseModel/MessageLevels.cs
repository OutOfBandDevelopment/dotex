using OoBDev.System.ComponentModel;
using OoBDev.System.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// response message level
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverterEx<MessageLevels>))]
public enum MessageLevels
{
    /// <summary>
    /// extra detailed information 
    /// </summary>
    [EnumValue("trace")]
    Trace,

    /// <summary>
    /// information to assist in troubleshooting
    /// </summary>
    [EnumValue("debug")]
    Debug,

    /// <summary>
    /// extra information about process
    /// </summary>
    [EnumValue("info")]
    Information,

    /// <summary>
    /// notifications and assumptions about processing that did not effect output
    /// </summary>
    [EnumValue("warning")]
    Warning,

    /// <summary>
    /// errors that caused the system to not complete your request as you may have expected
    /// </summary>
    [EnumValue("error")]
    Error,

    /// <summary>
    /// information about processing that failed
    /// </summary>
    [EnumValue("fatal")]
    Critical,

    /// <summary>
    /// unknown value
    /// </summary>
    [EnumValue("unknown")]
    None,
}
