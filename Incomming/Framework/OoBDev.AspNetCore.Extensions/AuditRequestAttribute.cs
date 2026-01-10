using System;

namespace OoBDev.AspNetCore.Extensions;

/// <summary>
/// When this action is called the request/response will be captured into Logs.Log.AuditLog
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class AuditRequestAttribute : Attribute
{
    public const string DefaultLogTypeName = "Application Audit";

    /// <summary>
    /// Optional: name of the property to use for Identifier and IdentifierValue
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    /// this needs to be a value found in [Logs].[Log].[LogType]::[Name]  
    /// </summary>
    public string LogTypeName { get; set; } = DefaultLogTypeName;

    /// <summary>
    /// Do not include the request payload with audit
    /// </summary>
    public bool ExcludeRequest { get; set; }

    /// <summary>
    /// Do not include the response payload with audit
    /// </summary>
    public bool ExcludeResponse { get; set; }
}
