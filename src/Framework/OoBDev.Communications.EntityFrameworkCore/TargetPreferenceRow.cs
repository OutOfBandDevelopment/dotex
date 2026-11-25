namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>The stored preference of one target.</summary>
public sealed class TargetPreferenceRow
{
    /// <summary>The target id; the key.</summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>The preferred channel names, in order, separated by commas.</summary>
    public string Channels { get; set; } = string.Empty;

    /// <summary>The culture name, for example <c>en-US</c>.</summary>
    public string CultureName { get; set; } = string.Empty;

    /// <summary>The time zone id.</summary>
    public string TimeZoneId { get; set; } = string.Empty;

    /// <summary>The <c>RequestPriority</c> as a number.</summary>
    public int MinimumPriority { get; set; }

    /// <summary>Start of the quiet hours as ticks of the time of day, or <c>null</c>.</summary>
    public long? QuietHoursStartTicks { get; set; }

    /// <summary>End of the quiet hours as ticks of the time of day, or <c>null</c>.</summary>
    public long? QuietHoursEndTicks { get; set; }
}
