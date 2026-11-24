namespace OoBDev.Data.DesiredState;

/// <summary>The outcome of one tool run.</summary>
public sealed record ToolResult(int ExitCode, string StandardOutput, string StandardError);
