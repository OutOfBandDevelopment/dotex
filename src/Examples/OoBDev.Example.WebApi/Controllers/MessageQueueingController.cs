using OoBDev.WebApi.Models;
using OoBDev.WebApi.Provider;
using OoBDev.WebApi.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Controller for handling message queueing operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageQueueingController"/> class.
/// </remarks>
/// <param name="provider">The example message provider.</param>
[ApiController]
[Route("api/[controller]")]
public class MessageQueueingController(
    IExampleMessageProvider provider,
    QueueChannelCatalog channels,
    ReceivedMessageLog receivedLog
    ) : ControllerBase
{
    /// <summary>
    /// Lists the configured message queue channels (provider, queue, receiver flag); secrets are never returned.
    /// </summary>
    /// <returns>One status per channel.</returns>
    [HttpGet("channels")]
    [AllowAnonymous]
    public IReadOnlyList<QueueChannelStatusModel> Channels() => channels.List();

    /// <summary>
    /// Lists the most recent messages the example handler received, newest first.
    /// </summary>
    /// <returns>Up to <see cref="ReceivedMessageLog.Capacity"/> messages.</returns>
    [HttpGet("received")]
    [AllowAnonymous]
    public IReadOnlyList<ReceivedMessageModel> Received() => receivedLog.Recent();

    /// <summary>
    /// Sends a message to the queue publicly without requiring authentication.
    /// </summary>
    /// <param name="model">The example message model.</param>
    /// <param name="correlationId">Optional correlation ID for tracking purposes.</param>
    /// <returns>A task representing the asynchronous operation and containing a string result.</returns>
    [HttpPost("public")]
    [AllowAnonymous]
    public async Task<string> PublicSend([FromBody] ExampleMessageModel model, string? correlationId = default) =>
        await provider.PostAsync(model, correlationId);

    /// <summary>
    /// Sends a message to the queue with authorization, requiring the caller to be authenticated.
    /// </summary>
    /// <param name="model">The example message model.</param>
    /// <param name="correlationId">Optional correlation ID for tracking purposes.</param>
    /// <returns>A task representing the asynchronous operation and containing a string result.</returns>
    [HttpPost("private")]
    [Authorize]
    public async Task<string> AuthenticatedSend([FromBody] ExampleMessageModel model, string? correlationId = default) =>
        await provider.PostAsync(model, correlationId);
}
