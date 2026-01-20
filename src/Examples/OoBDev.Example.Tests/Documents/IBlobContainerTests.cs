using OoBDev.Common;
using OoBDev.Documents;
using OoBDev.Documents.Containers;
using OoBDev.Documents.Models;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Examples.Tests.Documents;

[TestClass]
public class IBlobContainerTests
{
    public required TestContext TestContext { get; set; }

    private ServiceProvider ServiceProvider()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AzureBlobProviderOptions:ConnectionString","DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://192.168.1.170:10000/devstoreaccount1;" },
            })
            .Build()
            ;
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace)
                )
            .TryAllCommonExtensions(config,
                systemBuilder: new(),
                aspNetBuilder: new(),
                jwtBuilder: new(),
                identityBuilder: new(),
                externalBuilder: new(),
                hostingBuilder: new()
                )
            .BuildServiceProvider()
            ;
        return serviceProvider;
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task Create_IBlobContainer__Docs_Test()
    {
        var wrapper = ServiceProvider().GetRequiredService<IBlobContainer<Docs>>();

        var ms = new MemoryStream();
        var writer = new StreamWriter(ms) { AutoFlush = true };
        writer.WriteLine("Hello!");
        ms.Position = 0;

        var metadata = await wrapper.GetContentMetaDataAsync("helloWorld.txt");

        if (metadata == null)
            await wrapper.StoreContentAsync(
                new() { Content = ms, ContentType = "text/plain", FileName = "helloWorld.txt" },
                new Dictionary<string, string>
                {
                {"other", "stuff" }
                });

        var result = await wrapper.GetContentMetaDataAsync("helloWorld.txt");

        TestContext.AddResult(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public void Create_IBlobContainer__Summary_Test()
    {
        var wrapper = ServiceProvider().GetRequiredService<IBlobContainer<Summary>>();
        Assert.IsNotNull(wrapper);
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public void Create_IBlobContainer__Summary_List_Test()
    {
        var wrapper = ServiceProvider().GetRequiredService<IBlobContainer<Summary>>();
        var items = wrapper.QueryContent().ToArray();
        TestContext.AddResult(items);
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public void Test()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AzureBlobProviderOptions:ConnectionString","DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://192.168.1.170:10000/devstoreaccount1;" },
            })
            .Build()
            ;
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace)
                )
            .AddTransient<IBlobContainerProviderFactory, TestBlobFactory>()
            .TryAllCommonExtensions(config,
                systemBuilder: new(),
                aspNetBuilder: new(),
                jwtBuilder: new(),
                identityBuilder: new(),
                externalBuilder: new(),
                hostingBuilder: new()
                )
            .BuildServiceProvider()
            ;

        var converter = serviceProvider.GetRequiredService<IBlobContainer<Docs>>();

    }

    public class Docs { }
    public class Summary { }

    public class TestBlobFactory : IBlobContainerProviderFactory
    {
        public IBlobContainerProvider? Create(string containerName) => new TestBlobContainer()
        {
            ContainerName = containerName,
        };
    }
    public class TestBlobContainer : IBlobContainerProvider
    {
        public required string ContainerName { get; set; }
        public Task DeleteContentAsync(string path) => throw new global::System.NotImplementedException();
        public Task<ContentReference?> GetContentAsync(string path) => throw new global::System.NotImplementedException();
        public Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path) => throw new global::System.NotImplementedException();
        public IQueryable<ContentMetaDataReference> QueryContent() => throw new global::System.NotImplementedException();
        public Task StoreContentAsync(ContentReference reference, Dictionary<string, string>? metadata = null, bool overwrite = false) => throw new global::System.NotImplementedException();
        public Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference) => throw new global::System.NotImplementedException();
    }

}
