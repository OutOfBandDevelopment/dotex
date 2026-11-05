using System;
using OoBDev.System.IO;
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
    private TempDirectoryHandle _folder = null!;
    private string _dir = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        ActiveTheme.Reset();
        _folder = new TempDirectoryHandle();
        _dir = _folder.DirectoryPath;
    }

    [TestCleanup]
    public void Cleanup()
    {
        ActiveTheme.Reset();
        _folder.Dispose();
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
