using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Ocr;
using OoBDev.TestUtilities;

namespace OoBDev.Docling.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class RegistrationTests
{
    private static ServiceProvider Build(Dictionary<string, string?> values)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.TryAddDoclingOcr(config);
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task TryAddDoclingOcr_ResolvesPlainAndKeyed()
    {
        await using var provider = Build(new() { ["Docling:BaseUrl"] = "http://localhost:5001/" });

        Assert.IsInstanceOfType<DoclingOcr>(provider.GetRequiredService<IDocumentOcr>());
        Assert.IsInstanceOfType<DoclingOcr>(provider.GetRequiredKeyedService<IDocumentOcr>(DoclingGlobals.ProviderKey));
    }

    [TestMethod]
    public async Task TryAddDoclingOcr_MissingBaseUrl_FailsValidation()
    {
        await using var provider = Build([]);

        Assert.ThrowsExactly<OptionsValidationException>(() => provider.GetRequiredService<IDocumentOcr>());
    }
}
