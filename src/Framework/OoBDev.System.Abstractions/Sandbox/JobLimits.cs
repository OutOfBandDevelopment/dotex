namespace OoBDev.System.Sandbox;

/// <summary>The resource limits for one plugin (REQ-005). Zero means no limit.</summary>
/// <param name="MemoryMb">Per-process memory cap, enforced by the job object.</param>
/// <param name="MaxProcesses">Active process cap, enforced by the job object; 1 means the plugin cannot start a child.</param>
/// <param name="CpuSeconds">Processor time cap, enforced by the host's watcher.</param>
/// <param name="WallSeconds">Wall-clock cap, enforced by the host's watcher.</param>
public sealed record JobLimits(int MemoryMb = 0, int MaxProcesses = 0, int CpuSeconds = 0, int WallSeconds = 0);
