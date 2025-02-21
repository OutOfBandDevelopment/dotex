using System;

namespace OoBDev.System.Tests.TestTargets;

public class TargetModel
{
    public string TargetId { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;
    public DateTime DateTime { get; set; } = DateTime.Now;

    public DateTimeOffset? DateTimeOffsetNullable { get; set; } = DateTimeOffset.Now;
    public DateTime? DateTimeNullable { get; set; } = DateTime.Now;
}
