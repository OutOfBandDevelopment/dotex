using OoBDev.Common.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.Common.Logging;

/// <summary>
/// Mapping for entry to write to audit log
/// </summary>
[ConnectionStringName("Logs")]
[StoredProcedure("[Log].[ins_AudiLogData]")]
public record AuditLogEntry
{
    /// <summary>
    /// Note: this needs to be a value found in [ApplicationInputs].[dbo].[Application]::[ApplicationName]
    /// </summary>
    //Needs entry for ApplicationName
    [QueryParameter]
    [StringLength(50)]
    public required string ApplicationName { get; init; }

    /// <summary>
    /// Note: this needs to be a value found in [Logs].[Log].[LogType]::[Name] 
    /// </summary>
    [QueryParameter]
    [StringLength(50)]
    public required string LogTypeName { get; init; }

    /// <summary>
    /// Note: this is a session correlation id
    /// </summary>
    [QueryParameter]
    [StringLength(36)]
    public required string ApplicationSessionID { get; init; }

    /// <summary>
    /// Note: this should be HttpContext.Request.Path
    /// </summary>
    [QueryParameter]
    [StringLength(255)]
    public string? Description { get; init; }

    /// <summary>
    /// Note: this is the name of the property used for Primary Input
    /// </summary>
    [QueryParameter]
    [StringLength(36)]
    public string? Identifier { get; init; } = string.Empty;

    /// <summary>
    /// Note: this is the value of the property used for Primary Input
    /// </summary>
    [QueryParameter]
    [StringLength(50)]
    public string? IdentifierValue { get; init; } = string.Empty;

    /// <summary>
    /// Note: Request.HttpContext.Connection.RemoteIpAddress.ToString(),
    /// </summary>
    [QueryParameter]
    [StringLength(50)]
    public string? IPAddress { get; init; }

    /// <summary>
    /// json payload for description
    /// </summary>
    [QueryParameter(IsJson = true)]
    public required AuditLogInfo AuditInfo { get; init; }
}
