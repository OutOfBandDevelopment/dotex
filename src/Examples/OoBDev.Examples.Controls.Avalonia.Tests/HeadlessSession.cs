using Avalonia.Headless;

namespace OoBDev.Examples.Controls.Avalonia.Tests;

/// <summary>The one headless session of the test process; Avalonia allows only one application per process.</summary>
public static class HeadlessSession
{
    /// <summary>The shared session.</summary>
    public static HeadlessUnitTestSession Instance { get; } = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));
}
