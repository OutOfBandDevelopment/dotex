using Avalonia;
using Avalonia.Headless;
using Avalonia.Skia;
using OoBDev.Examples.Controls.Avalonia.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace OoBDev.Examples.Controls.Avalonia.Tests;

/// <summary>Headless application with the real Skia renderer, so screenshots show what a user sees.</summary>
public static class TestAppBuilder
{
    /// <summary>Builds the test application.</summary>
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<ControlsApp>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
