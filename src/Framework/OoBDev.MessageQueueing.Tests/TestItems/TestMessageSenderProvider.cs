using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests.TestItems;

public partial class TestMessageSenderProvider(
    TestContext context,
    ILogger<TestMessageSenderProvider> logger
        ) : IMessageSenderProvider
{
    public const string ProviderName = "test-provider";

    /// <summary>The context passed to the most recent send.</summary>
    public static IMessageContext? LastContext { get; private set; }

    private readonly TestContext _context = context;
    private readonly ILogger _logger = logger;

    public Task<string?> SendAsync(object message, IMessageContext context)
    {
        LastContext = context;
        LogSend(_logger, message, context);
        _context.AddResult(message, fileName: nameof(message));
        _context.AddResult(context, fileName: nameof(context));
        return Task.FromResult((string?)null);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "SendAsync({message}, {context})")]
    private static partial void LogSend(ILogger logger, object message, IMessageContext context);
}
