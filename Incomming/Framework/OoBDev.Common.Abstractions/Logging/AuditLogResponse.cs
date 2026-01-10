using OoBDev.Common.ComponentModel;

namespace OoBDev.Common.Logging;

/// <summary>
/// Result from [Log].[ins_AudiLogData]
/// </summary>
public record class AuditLogResponse
{
    /// <summary>
    /// from [Logs].[Log].[LogType]::[AuditID] 
    /// </summary>
    [QueryResult(Position = 0)]
    public decimal AuditId { get; init; }
}
