namespace OoBDev.DevTerm.Configuration;

/// <summary>The outcome of one <see cref="StreamCaptureConverter.ConvertAsync"/> call.</summary>
public sealed record StreamConversionResult(bool Success, string? OutputPath, string? Error);
