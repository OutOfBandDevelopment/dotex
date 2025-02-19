using OoBDev.AI;
using OoBDev.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Search.Providers;

/// <summary>
/// Provides functionality to generate summaries for documents.
/// </summary>
[Obsolete]
[ExcludeFromCodeCoverage]
public class DocumentSummaryGenerationProvider
{
    private const int MAX_LENGTH = 4096;

    private readonly IMessageCompletion _messageCompletion;
    private readonly string _modelName;
    private readonly string _promptTemplate;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentSummaryGenerationProvider"/> class.
    /// </summary>
    /// <param name="messageCompletion">The message completion service.</param>
    /// <param name="modelName">The name of the model to use for completion.</param>
    /// <param name="promptTemplate">The template for the completion prompt.</param>
    public DocumentSummaryGenerationProvider(
        IMessageCompletion messageCompletion,
        string modelName = "mistral:instruct", //TODO: move these to configs
        string promptTemplate = @"Write a single paragraph of less than 5 sentences to summarize the following text 
{0}"
        )
    {
        _messageCompletion = messageCompletion;
        _modelName = modelName;
        _promptTemplate = promptTemplate;
        ;
    }

    /// <summary>
    /// Generates a summary for the provided content asynchronously.
    /// </summary>
    /// <param name="content">The content for which to generate a summary.</param>
    /// <returns>A summary of the content.</returns>
    public virtual async Task<string> GenerateSummaryAsync(string content)
    {
        // if (content.Length < MAX_LENGTH) return content;

        var x = 0;
        var chunks = content.ReplaceLineEndings(" ").SplitBy(MAX_LENGTH);
        var sb = new StringBuilder();
        foreach (var chunk in chunks)
        {
            x++;
            var result = await GetCompletionAsync(_modelName, _promptTemplate, chunk);
            sb.Append(result).Append(' ');
        }
        if (x <= 1)
            return sb.ToString();
        var final = await GetCompletionAsync(_modelName, _promptTemplate, sb.ToString());
        return final;
    }

    internal async Task<string> GetCompletionAsync(string modelName, string promptTemplate, string content)
    {
        var prompt = string.Format(promptTemplate, content);

        var completion = await _messageCompletion.GetCompletionAsync(modelName, prompt);

        return completion;
    }
}
