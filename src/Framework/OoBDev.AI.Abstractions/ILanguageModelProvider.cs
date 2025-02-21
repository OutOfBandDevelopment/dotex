using OoBDev.AI.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AI;

/// <summary>
/// Represents a provider for a language model.
/// </summary>
public interface ILanguageModelProvider
{
    /// <summary>
    /// Gets a response asynchronously based on the provided prompt details and user input.
    /// </summary>
    /// <param name="promptDetails">The details of the prompt.</param>
    /// <param name="userInput">The user input.</param>
    /// <param name="cancellationToken">The Cancellation Token.</param>
    /// <returns>A task representing the asynchronous operation that returns the response.</returns>
    Task<string> GetResponseAsync(string promptDetails,
        string userInput,
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

    /// <summary>
    /// Gets a streamed response asynchronously based on the provided prompt details and user input.
    /// </summary>
    /// <param name="promptDetails">The details of the prompt.</param>
    /// <param name="userInput">The user input.</param>
    /// <param name="cancellationToken">The Cancellation Token.</param>
    /// <returns>An asynchronous enumerable of strings representing the streamed response.</returns>
    IAsyncEnumerable<string> GetStreamedResponseAsync(
        string promptDetails,
        string userInput,
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

    /// <summary>
    /// Gets a response asynchronously based on the provided prompt details and user input.
    /// </summary>
    /// <param name="assistantConfinment">The confinment of the AI Assistant</param>
    /// <param name="systemInteractions">The previous generated responses by the AI</param>
    /// <param name="userInput">The user input including any previous messages in the chat</param>
    /// <param name="cancellationToken">The Cancellation Token.</param>
    /// <returns>A task representing the asynchronous operation that returns the response.</returns>
    IAsyncEnumerable<string> GetStreamedContextResponseAsync(string assistantConfinment,
        List<string> systemInteractions,
        List<string> userInput,
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

    /// <summary>
    /// Gets the embedded response for the data provided.
    /// </summary>
    /// <param name="data">The data to be embedded there is a token limit of 2048 tokens.</param>    
    /// <param name="cancellationToken">The Cancellation Token.</param>
    /// <returns>Float array with the embedding data</returns>
    Task<float[]> GetEmbeddedResponseAsync(string data,
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

    /// <summary>
    /// Gets a response asynchronously based on the provided prompt details and user input.
    /// </summary>
    /// <param name="assistantConfinment">The confinement of the AI Assistant</param>
    /// <param name="systemInteractions">The previous generated responses by the AI</param>
    /// <param name="userInput">The user input including any previous messages in the chat</param>
    /// <param name="cancellationToken">The Cancellation Token.</param>
    /// <returns>A task representing the asynchronous operation that returns the response.</returns>
    Task<string> GetContextResponseAsync(string assistantConfinment,
        List<string> systemInteractions,
        List<string> userInput,
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

    /// <summary>
    /// Gets a response asynchronously based on the provided prompt details and user input.
    /// </summary>    
    /// <param name="assistantConfinment">The confinement of the AI Assistant</param>
    /// <param name="ragData">The data to restrict the response</param>
    /// <param name="userInput">The user input.</param>
    /// <param name="cancellationToken">The Cancellation Token.</param>
    /// <returns>A task representing the asynchronous operation that returns the response.</returns>
    Task<string> GetRAGResponseAsync(string assistantConfinment,
        string ragData,
        string userInput,
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable

    /// <summary>
    /// Gets a response asynchronously based on the ragData and userQuery
    /// </summary>
    /// <param name="ragData">The details of the prompt.</param>
    /// <param name="userQuery">The userQuery</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the generated response.</returns>
    IAsyncEnumerable<string> GetRAGResponseCitiationsAsync(List<KeyValuePairModel> ragData,
        string userQuery,
#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
}
