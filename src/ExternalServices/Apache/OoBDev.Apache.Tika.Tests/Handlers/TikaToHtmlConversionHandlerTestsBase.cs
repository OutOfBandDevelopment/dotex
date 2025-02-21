using OoBDev.Documents.Conversion;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Tests.Handlers;

public abstract class TikaToHtmlConversionHandlerTestsBase<T> where T : IDocumentConversionHandler
{
    public async Task ConvertAsyncTestHarness(
        string resourceName,
        string sourceType,
        string targetType,
        string ext,
        TestContext testContext
        )
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApacheTikaClientOptions:Url","http://127.0.0.1:9998"}
            })
            .Build();
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace)
                )
            .TryAddApacheTikaServices(config, nameof(ApacheTikaClientOptions))
            .BuildServiceProvider()
            ;

        var converter = (IDocumentConversionHandler)ActivatorUtilities.CreateInstance<T>(serviceProvider)!;

        var resource = GetType().Assembly.GetManifestResourceNames().FirstOrDefault(l => l.EndsWith($".{resourceName}"))
            ?? throw new ApplicationException($"missing .{resourceName} resource");
        using var stream = GetType().Assembly.GetManifestResourceStream(resource)
            ?? throw new ApplicationException($"missing .{resourceName} resource");

        using var htmlStream = new MemoryStream();
        stream.CopyTo(htmlStream);
        htmlStream.Position = 0;
        using var pdfStream = new MemoryStream();

        await converter.ConvertAsync(htmlStream, sourceType, pdfStream, targetType);
        pdfStream.Position = 0;

        testContext.WriteLine($"Content Length: {pdfStream.Length}");

        testContext.AddResult(pdfStream, fileName: Path.ChangeExtension(resourceName, ext));

        Assert.IsTrue(pdfStream.Length > 240);
    }

    public async Task ConvertAsyncTestHarness(string sourceContentType, string destinationContentType)
    {
        var source = new MemoryStream();
        var destination = new MemoryStream();

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockClient = mockRepo.Create<IApacheTikaClient>();

        mockClient
            .Setup(s => s.ConvertAsync(source, sourceContentType, destination, destinationContentType))
            .Returns(Task.CompletedTask);

        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace)
                )
            .AddTransient(_ => mockClient.Object)
            .BuildServiceProvider()
            ;

        var handler = ActivatorUtilities.CreateInstance<T>(serviceProvider);

        await handler.ConvertAsync(source, sourceContentType, destination, destinationContentType);

        mockRepo.VerifyAll();
    }
}
