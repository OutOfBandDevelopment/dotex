using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.AI;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.LlmCodeGen.Cli.Tests;

[TestClass]
public class LlmCodeGenServiceTests
{
    private static (IServiceProvider services, IMessageCompletion ollama, IMessageCompletion groq) Create()
    {
        var ollama = new Mock<IMessageCompletion>().Object;
        var groq = new Mock<IMessageCompletion>().Object;
        var services = new ServiceCollection()
            .AddKeyedSingleton("ollama", ollama)
            .AddKeyedSingleton("groq-cloud", groq)
            .BuildServiceProvider();
        return (services, ollama, groq);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("ollama", false)]
    [DataRow("OLLAMA", false)]
    [DataRow("groq", true)]
    [DataRow("Groq", true)]
    [DataRow("groq-cloud", true)]
    public void ResolveCompletion_KnownProvider_ReturnsItsAdapter(string provider, bool isGroq)
    {
        // Stage
        var (services, ollama, groq) = Create();

        // Test
        var completion = LlmCodeGenService.ResolveCompletion(services, provider);

        // Assert
        Assert.AreSame(isGroq ? groq : ollama, completion);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ResolveCompletion_UnknownProvider_NamesItInTheError()
    {
        // Stage
        var (services, _, _) = Create();

        // Test
        var error = Assert.ThrowsExactly<InvalidOperationException>(() => LlmCodeGenService.ResolveCompletion(services, "nope"));

        // Assert
        StringAssert.Contains(error.Message, "nope");
    }
}
