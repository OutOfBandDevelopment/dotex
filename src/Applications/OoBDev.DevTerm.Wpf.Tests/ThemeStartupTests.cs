using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Theme start-up: command line, saved preference and persistence of later choices.</summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class ThemeStartupTests
{
    private string _dir = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        ActiveTheme.Reset();
        _dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}")).FullName;
    }

    [TestCleanup]
    public void Cleanup()
    {
        ActiveTheme.Reset();
        Directory.Delete(_dir, recursive: true);
    }

    [TestMethod]
    public void Initialize_ThemeOnCommandLine_SelectsItWithoutSavingIt()
    {
        // Stage
        var store = new AppPreferencesStore(Path.Combine(_dir, "prefs.json"));

        // Test
        ThemeStartup.Initialize(["--theme", BuiltInThemes.DarkName], store);

        // Assert
        Assert.AreSame(BuiltInThemes.Dark, ActiveTheme.Current);
        Assert.IsFalse(File.Exists(store.Path));
    }

    [TestMethod]
    public void Initialize_ThenSelect_PersistsTheChoiceForNextStart()
    {
        // Stage
        var store = new AppPreferencesStore(Path.Combine(_dir, "prefs.json"));
        ThemeStartup.Initialize([], store);

        // Test
        ActiveTheme.Select(BuiltInThemes.DarkName);
        ActiveTheme.Reset();
        ThemeStartup.Initialize([], store);

        // Assert
        Assert.AreSame(BuiltInThemes.Dark, ActiveTheme.Current);
    }
}
