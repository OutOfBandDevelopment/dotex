using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.System.Tests;

[TestClass]
public class OptionsServiceCollectionExtensionsTests
{
    private sealed class SampleOptions
    {
        [Required]
        public string? Name { get; set; }

        [Range(1, 10)]
        public int Count { get; set; } = 1;
    }

    private static ServiceProvider Build(Dictionary<string, string?> values, bool validateOnStart = true)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return new ServiceCollection()
            .AddValidatedOptions<SampleOptions>(configuration, nameof(SampleOptions), validateOnStart)
            .Services.BuildServiceProvider();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddValidatedOptions_ValidSection_BindsValues()
    {
        using var services = Build(new() { ["SampleOptions:Name"] = "a", ["SampleOptions:Count"] = "5" });

        var options = services.GetRequiredService<IOptions<SampleOptions>>().Value;

        Assert.AreEqual("a", options.Name);
        Assert.AreEqual(5, options.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddValidatedOptions_MissingRequired_ThrowsOnUse()
    {
        using var services = Build(new() { ["SampleOptions:Count"] = "5" }, validateOnStart: false);

        Assert.ThrowsExactly<OptionsValidationException>(() => _ = services.GetRequiredService<IOptions<SampleOptions>>().Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddValidatedOptions_OutOfRange_ThrowsOnUse()
    {
        using var services = Build(new() { ["SampleOptions:Name"] = "a", ["SampleOptions:Count"] = "99" }, validateOnStart: false);

        Assert.ThrowsExactly<OptionsValidationException>(() => _ = services.GetRequiredService<IOptions<SampleOptions>>().Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddValidatedOptions_Strict_FailsStartupValidation()
    {
        using var services = Build(new() { ["SampleOptions:Count"] = "5" });

        Assert.ThrowsExactly<OptionsValidationException>(() => services.GetRequiredService<IStartupValidator>().Validate());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddValidatedOptions_Relaxed_RegistersNoStartupValidation()
    {
        using var services = Build(new() { ["SampleOptions:Count"] = "5" }, validateOnStart: false);

        Assert.IsNull(services.GetService<IStartupValidator>());
    }
}
