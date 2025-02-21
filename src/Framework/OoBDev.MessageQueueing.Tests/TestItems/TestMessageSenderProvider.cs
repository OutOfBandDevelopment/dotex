using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests.TestItems;

public class TestMessageSenderProvider(
    TestContext context,
    ILogger<TestMessageSenderProvider> logger
        ) : IMessageSenderProvider
{
    public const string ProviderName = "test-provider";

    private readonly TestContext _context = context;
    private readonly ILogger _logger = logger;

    public Task<string?> SendAsync(object message, IMessageContext context)
    {
        _logger.LogInformation($"{nameof(SendAsync)}({{{nameof(message)}}}, {{{nameof(context)}}})", message, context);
        _context.AddResult(message, fileName: nameof(message));
        _context.AddResult(context, fileName: nameof(context));
        return Task.FromResult((string?)null);
    }
}
