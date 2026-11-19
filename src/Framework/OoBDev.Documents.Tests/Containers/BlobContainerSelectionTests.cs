using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Documents.Containers;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Tests.Containers;

[TestClass]
public class BlobContainerSelectionTests
{
    public required TestContext TestContext { get; set; }

    private static IBlobContainerFactory Build(IDictionary<string, string?> settings, out ServiceProvider services, string? fileRoot = null)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var collection = new ServiceCollection().AddLogging();
        _ = collection.TryAddDocumentServices();
        _ = collection.TryAddBlobContainerSelection(configuration);
        _ = collection.TryAddFileSystemBlobServices(configuration);
        collection.AddKeyedTransient<IBlobContainerProviderFactory>("other", (_, _) => Mock.Of<IBlobContainerProviderFactory>(f => f.Create(It.IsAny<string>()) == Mock.Of<IBlobContainerProvider>()));
        collection.AddTransient(_ => Mock.Of<IBlobContainerProvider>());
        services = collection.BuildServiceProvider();
        return services.GetRequiredService<IBlobContainerFactory>();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_TwoContainers_ResolveTheirOwnConfiguredProviders()
    {
        using var temp = new TempFileFactory(null!).GetTempDirectory();
        var factory = Build(new Dictionary<string, string?>
        {
            ["BlobContainerOptions:Providers:invoices"] = DocumentsGlobals.FileSystemBlobProviderKey,
            ["BlobContainerOptions:Providers:exports"] = "other",
            ["FileSystemBlobOptions:RootDirectory"] = temp.DirectoryPath,
        }, out var services);
        using var _ = services;

        var invoices = factory.Create("invoices");
        var exports = factory.Create("exports");

        TestContext.WriteLine($"invoices: {invoices.GetType().Name}, exports: {exports.GetType().Name}");
        Assert.IsInstanceOfType<FileSystemBlobContainerProvider>(invoices);
        Assert.IsNotInstanceOfType<FileSystemBlobContainerProvider>(exports);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_UnknownProviderKey_ThrowsNamingTheKey()
    {
        var factory = Build(new Dictionary<string, string?> { ["BlobContainerOptions:Providers:invoices"] = "no-such-provider" }, out var services);
        using var _ = services;

        var error = Assert.ThrowsExactly<global::System.InvalidOperationException>(() => factory.Create("invoices"));

        TestContext.WriteLine(error.Message);
        StringAssert.Contains(error.Message, "no-such-provider");
        StringAssert.Contains(error.Message, "invoices");
    }
}
