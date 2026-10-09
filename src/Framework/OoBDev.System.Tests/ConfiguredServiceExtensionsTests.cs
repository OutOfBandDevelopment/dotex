using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;

namespace OoBDev.System.Tests;

[TestClass]
public class ConfiguredServiceExtensionsTests
{
    private interface IThing { string Name { get; } }
    private sealed class DefaultThing : IThing { public string Name => "default"; }
    private sealed class OtherThing : IThing { public string Name => "other"; }

    private static ServiceProvider Build(string? configured)
    {
        var values = new Dictionary<string, string?>();
        if (configured != null)
            values["Things:Type"] = configured;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddTransient<IThing, DefaultThing>()
            .AddKeyedTransient<IThing, OtherThing>("other")
            .TryAddConfiguredKeyedService<IThing>("Things:Type", "selected")
            .BuildServiceProvider();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Selected_NoConfiguration_UsesDefault()
    {
        using var services = Build(null);

        Assert.AreEqual("default", services.GetRequiredKeyedService<IThing>("selected").Name);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Selected_ConfiguredKey_UsesKeyedService()
    {
        using var services = Build("other");

        Assert.AreEqual("other", services.GetRequiredKeyedService<IThing>("selected").Name);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Selected_UnknownKey_Throws()
    {
        using var services = Build("missing");

        Assert.ThrowsExactly<InvalidOperationException>(() => services.GetRequiredKeyedService<IThing>("selected"));
    }
}
