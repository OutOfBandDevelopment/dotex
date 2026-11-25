using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Scanning;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.ClamAv.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class RegistrationTests
{
    private static ServiceProvider Build(Dictionary<string, string?> values)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddClamAvScanner(config);
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task TryAddClamAvScanner_ResolvesPlainAndKeyed_WithDefaults()
    {
        await using var provider = Build([]);

        Assert.IsInstanceOfType<ClamAvContentScanner>(provider.GetRequiredService<IContentScanner>());
        Assert.IsInstanceOfType<ClamAvContentScanner>(provider.GetRequiredKeyedService<IContentScanner>(ClamAvGlobals.ProviderKey));
        Assert.AreEqual(3310, provider.GetRequiredService<IOptions<ClamAvOptions>>().Value.Port);
    }

    [TestMethod]
    public async Task TryAddClamAvScanner_BadPort_FailsValidation()
    {
        await using var provider = Build(new() { ["ClamAv:Port"] = "0" });

        Assert.ThrowsExactly<OptionsValidationException>(() => provider.GetRequiredService<IOptions<ClamAvOptions>>().Value);
    }
}
