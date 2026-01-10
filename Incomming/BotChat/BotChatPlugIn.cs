using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using BotChat.Clients;
using BotChat.KernelHost;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BotChat;

public class BotChatPlugIn : IKernelPlugIn
{
    private readonly IBotChatClient _client;
    private readonly ILogger _logger;

    public BotChatPlugIn(
        IBotChatClient client,
        ILogger<BotChatPlugIn> logger
        )
    {
        _client = client;
        _logger = logger;
    }

    [KernelFunction("match_submission_by_name")]
    [Description("Gets submissions for person by name")]
    [return: Description("list of submissions")]
    public async Task<MatchResponse> GetSubmissions(string name)
    {
        _logger.LogInformation("Called: {method}", nameof(GetSubmissions));
        _logger.LogInformation("Looking for submissions related to {name}", name);

        var result = await _client.MatchAsync(new MatchRequest
        {
            InsuredName = name,
            InsuredThreshold = 0.5f,

            //PolicyNumberDateDays = 180,
            InsuredDistanceMetric = "cosine",
        });

        _logger.LogInformation("{name} - \"{Message}\" {TotalRecords}", name, result.MessageFlags, result.TotalRecords);

        return result;
    }

    [KernelFunction("get_submission_by_quote_id")]
    [Description("Gets submissions similar to the one provided")]
    [return: Description("list of submissions")]
    public async Task<MatchResponse> GetSimilarSubmissions(string quoteId)
    {
        _logger.LogInformation("Called: {method}", nameof(GetSimilarSubmissions));
        _logger.LogInformation("Looking for submissions similar to {quoteId}", quoteId);

        var result = await _client.MatchByQuoteIdAsync(new MatchQuoteIdRequest
        {
            QuoteId = quoteId,

            InsuredThreshold = 0.5f,
            NaicsCodeThreshold = 4,
            ZipCodeThreshold = 4,

            //PolicyNumberDateDays = 180,
            InsuredDistanceMetric = "cosine",
        });

        _logger.LogInformation("{name} - \"{Message}\" {TotalRecords}", quoteId, result.MessageFlags, result.TotalRecords);

        return result;
    }
}

