namespace OoBDev.System.IO.Devices;

/// <summary>One changed value in a pending write.</summary>
public sealed record DeviceConfigChange(string FieldId, string? From, string? To);
