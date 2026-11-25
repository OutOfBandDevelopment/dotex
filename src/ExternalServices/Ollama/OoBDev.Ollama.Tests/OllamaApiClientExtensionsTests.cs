using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OllamaSharp;
using OllamaSharp.Models;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Ollama.Tests;

[TestClass]
public class OllamaApiClientExtensionsTests
{
    public required TestContext TestContext { get; set; }

    private static Mock<IOllamaApiClient> Client(params string[] localModels)
    {
        var client = new Mock<IOllamaApiClient>();
        client.Setup(c => c.ListLocalModelsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IEnumerable<Model>>(localModels.Select(n => new Model { Name = n }).ToList()));
        client.Setup(c => c.CopyModelAsync(It.IsAny<CopyModelRequest>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return client;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task EnsureModelExists_ModelPresent_CopiesNothingAsync()
    {
        // Stage
        var client = Client("phi3:latest", "other:latest");

        // Test
        await client.Object.EnsureModelExistsAsync("phi3", NullLogger.Instance);

        // Assert
        TestContext.WriteLine("Copies requested: {0}", client.Invocations.Count(i => i.Method.Name == nameof(IOllamaApiClient.CopyModelAsync)));
        client.Verify(c => c.CopyModelAsync(It.IsAny<CopyModelRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task EnsureModelExists_ModelMissingWithSource_CopiesTheNamedSourceAsync()
    {
        // Stage
        var client = Client("llama3:latest", "mistral:latest");

        // Test
        await client.Object.EnsureModelExistsAsync("phi3", NullLogger.Instance, "mistral:latest");

        // Assert
        client.Verify(c => c.CopyModelAsync(It.Is<CopyModelRequest>(r => r.Source == "mistral:latest" && r.Destination == "phi3"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task EnsureModelExists_ModelMissingWithoutSource_CopiesTheFirstLocalModelAsync()
    {
        // Stage
        var client = Client("llama3:latest", "mistral:latest");

        // Test
        await client.Object.EnsureModelExistsAsync("phi3", NullLogger.Instance);

        // Assert
        client.Verify(c => c.CopyModelAsync(It.Is<CopyModelRequest>(r => r.Source == "llama3:latest" && r.Destination == "phi3"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task EnsureModelExists_NoLocalModel_ThrowsAsync()
    {
        // Stage
        var client = Client();

        // Test
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => client.Object.EnsureModelExistsAsync("phi3", NullLogger.Instance));

        // Assert
        TestContext.WriteLine("Message: {0}", ex.Message);
        client.Verify(c => c.CopyModelAsync(It.IsAny<CopyModelRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
