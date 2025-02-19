using OoBDev.Communications.Models;
using OoBDev.MailKit.Services;
using OoBDev.MessageQueueing;
using MailKit;
using MailKit.Search;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MailKit.Hosting;

/// <summary>
/// Hosted service responsible for starting and stopping message receivers based on the configured providers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EmailMessageReceiverHost"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
/// <param name="queue">The queue.</param>
/// <param name="imapClientFactory">The client factory.</param>
/// <param name="config">imap config.</param>
/// <param name="messageFactory">The message factory.</param>
public class EmailMessageReceiverHost(
    ILogger<EmailMessageReceiverHost> logger,
    IMessageQueueSender<ReceivedEmailMessageModel> queue,
    IImapClientFactory imapClientFactory,
    IOptions<MailKitImapClientOptions> config,
    IMimeMessageFactory messageFactory
    ) : IHostedService, IDisposable
{
    private readonly List<Task> _tasks = [];
    private readonly CancellationTokenSource _tokenSource = new();

    private bool _disposed = false;

    /// <summary>
    /// Disposes of the resources used by the <see cref="EmailMessageReceiverHost"/>.
    /// </summary>
    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);
        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="EmailMessageReceiverHost"/>.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // note: dispose managed state (managed objects).
            logger.LogInformation("Request Dispose");
            _tokenSource.Cancel();
            logger.LogInformation("Complete Dispose");
        }

        // note: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // note: set large fields to null.

        _disposed = true;
    }

    /// <summary>
    /// Starts the message receiver host.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the start operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
#if !DEBUG
throw new NotSupportedException("this is still in development and is not currently supported");
#endif

        var client = await imapClientFactory.CreateAsync();

        await client.NoOpAsync(cancellationToken);

        var folder = client.Inbox;

        var folderAccess = await folder.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

        // https://github.com/jstedfast/MailKit?tab=readme-ov-file#searching-an-imap-folder

        var fetched = await folder.SearchAsync(SearchQuery.NotDeleted.And(SearchQuery.NotSeen), cancellationToken);

        //var fetched = await folder.FetchAsync(0, -1, new FetchRequest
        //{
        //    Headers = new HeaderSet(
        //        [HeaderId.Received]
        //        )
        //    {
        //        Exclude = true,
        //    },
        //    Items = MessageSummaryItems.UniqueId | MessageSummaryItems.BodyStructure | MessageSummaryItems.Flags,

        //});

        foreach (var itemId in fetched)
        {
            var message = await folder.GetMessageAsync(itemId);

            var received = messageFactory.ToReceived(
                message,
                config.Value.Uri == null ? $"{config.Value.Host}:{config.Value.Port}" : config.Value.Uri.ToString(),
                folder.Name
                );

            await queue.SendAsync(received, received.ReferenceId);

            var storedResult = await folder.StoreAsync(itemId, new StoreFlagsRequest(StoreAction.Add, MessageFlags.Seen) { Silent = true }, cancellationToken);

            /*
folder.Store (uid, new StoreFlagsRequest (StoreAction.Add, MessageFlags.Deleted) { Silent = true });
folder.Expunge ();
             */
        }

        //if (_tasks.Count > 0)
        //{
        //    throw new InvalidOperationException($"Already started!");
        //}

        //_logger.LogInformation("Request Start");

        //var providers = factory.Create().ToArray();

        //var token = _tokenSource.Token;

        //foreach (var provider in providers)
        //{
        //    _tasks.Add(Task.Run(() => StartProvider(provider, token), cancellationToken));
        //}

        //_logger.LogInformation("Completed Start");

        //return Task.CompletedTask;
    }

    //private async Task StartProvider(IMessageReceiverProvider provider, CancellationToken token)
    //{
    //    while (!token.IsCancellationRequested)
    //    {
    //        try
    //        {
    //            _logger.LogInformation($"Starting: {{{nameof(provider)}}}", provider);
    //            var task = provider.RunAsync(token);
    //            _logger.LogInformation($"Started: {{{nameof(provider)}}}", provider);
    //            await task;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError($"Exception: {{{nameof(provider)}}}: {{{nameof(Exception)}}}", provider, ex.Message);
    //            _logger.LogDebug($"Error: {{{nameof(provider)}}}: {{{nameof(Exception)}}}", provider, ex.ToString());

    //            _logger.LogInformation($"Waiting for restart: {{{nameof(provider)}}}", provider);
    //            await Task.Delay(10000); // TODO: this should be configurable
    //        }
    //    }
    //}

    /// <summary>
    /// Stops the message receiver host.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the stop operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Request Stop");

        await _tokenSource.CancelAsync();
        await Task.WhenAll(_tasks);
        _tasks.Clear();

        logger.LogInformation("Completed Stop");
    }
}
