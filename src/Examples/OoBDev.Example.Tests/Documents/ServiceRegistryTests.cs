using OoBDev.Common;
using OoBDev.Documents;
using OoBDev.Examples.Tests.TestTargets;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace OoBDev.Examples.Tests.Documents;

[TestClass]
public class ServiceRegistryTests
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
    public void Create_IBlobContainer__ContainerTargetClass_Test()
    {
        var wrapper = ServiceProvider().GetRequiredService<IBlobContainer<ContainerTargetClass>>();
        Assert.IsNotNull(wrapper);
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public void Create_IBlobContainer__ContainerTargetClassWithTag_Test()
    {
        var wrapper = ServiceProvider().GetRequiredService<IBlobContainer<ContainerTargetClassWithTag>>();
        Assert.IsNotNull(wrapper);
    }

}
