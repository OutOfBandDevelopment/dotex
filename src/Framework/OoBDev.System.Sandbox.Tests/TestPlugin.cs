using System;
using System.IO;

namespace OoBDev.System.Sandbox.Tests;

internal static class TestPlugin
{
    public static string Dll { get; } = Path.Combine(AppContext.BaseDirectory, "OoBDev.System.Sandbox.Plugin.dll");
}
