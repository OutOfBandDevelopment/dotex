using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Segmenters;
using OoBDev.System.Threading;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Pipelines;

/// <summary>
/// Provides a message-based device abstraction over a stream, supporting both receiving and transmitting messages.
/// </summary>
/// <typeparam name="TMessage">The type of messages to send and receive.</typeparam>
public class StreamDevice<TMessage> : IStreamDevice<TMessage>
{
    private readonly IProducerConsumerCollection<TMessage> _transmissionQueue = new ConcurrentQueue<TMessage>();

    private Stream AdapterStream => _adapter.Stream;
    private readonly IDeviceAdapter _adapter;
    private readonly IDeviceDefinition _device;
    private readonly ISegmentBuildDefinition? _segmentDefintion;
    private readonly IMessageDecoder<TMessage>? _decoder;
    private readonly IMessageEncoder<TMessage>? _encoder;
    private readonly int _minimumTransmissionDelay;
    private readonly CancellationToken _token;
    private readonly CancellationTokenSource _tokenSource;

    /// <summary>
    /// Initializes a new instance of the StreamDevice class.
    /// </summary>
    /// <param name="adapter">The device adapter providing access to the underlying stream.</param>
    /// <param name="device">The device definition describing the device's behavior.</param>
    /// <param name="token">Optional cancellation token to stop the device operations.</param>
    /// <param name="minimumTransmissionDelay">Minimum delay between message transmissions in milliseconds (default is 1000ms).</param>
    public StreamDevice(
        IDeviceAdapter adapter,
        IDeviceDefinition device,
        CancellationToken token = default,
        int minimumTransmissionDelay = 1000 //TODO should this default be overideable from the devicedefinition or it's attributes?
        )
    {
        _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        _token = _tokenSource.Token;

        _adapter = adapter;
        _device = device;
        _minimumTransmissionDelay = minimumTransmissionDelay;

        Task? messageReceiver = null;
        Task? messageTransmitter = null;
        Task? deviceInitializer = null;

        var mre = new AsyncManualResetEvent();
        if (_device is IDeviceDefinitionInitialize)
        {
            mre.Reset();
            deviceInitializer = Initializer(mre);
        }
        else
        {
            //Assumed to start in set state but just be sure anyway
            mre.Set();
        }

        if (_device is IDeviceDefinitionReceiver<TMessage> receiver)
        {
            _decoder = receiver.Decoder;
            _segmentDefintion = receiver.SegmentDefintion;
            messageReceiver = Receiver(mre);
        }
        if (_device is IDeviceDefinitionTransmitter<TMessage> transmitter)
        {
            _encoder = transmitter.Encoder;
            messageTransmitter = Transmitter(mre);
        }

        Runner = Task.WhenAll(
            deviceInitializer ?? Task.FromResult(0),
            messageReceiver ?? Task.FromResult(0),
            messageTransmitter ?? Task.FromResult(0)
            );
    }

    /// <summary>
    /// Gets the task representing the running device operations (initialization, receiving, and transmitting).
    /// </summary>
    public Task Runner { get; }

    /// <summary>
    /// Occurs when a message is received from the device.
    /// </summary>
    public event EventHandler<TMessage>? MessageReceived;

    /// <summary>
    /// Occurs when the device status changes.
    /// </summary>
    public event EventHandler<StreamDeviceStatus>? DeviceStatus;

    /// <summary>
    /// Occurs when an error happens during message receiving.
    /// </summary>
    public event EventHandler<DeviceErrorEventArgs>? MessageReceivedError;

    /// <summary>
    /// Occurs when an error happens during message transmission.
    /// </summary>
    public event EventHandler<DeviceErrorEventArgs>? MessageTransmitterError;

    /// <summary>
    /// Queues a message for transmission to the device.
    /// </summary>
    /// <param name="message">The message to transmit.</param>
    /// <returns>A task that resolves to true if the message was successfully queued, false otherwise.</returns>
    public Task<bool> Transmit(TMessage message) => Task.FromResult(_transmissionQueue.TryAdd(message));

    private Task OnMessageReceived(TMessage message)
    {
        MessageReceived?.Invoke(this, message);
        return Task.FromResult(0);
    }
    private Task ReportDeviceStatus(StreamDeviceStatus status)
    {
        DeviceStatus?.Invoke(this, status);
        return Task.FromResult(0);
    }

    private async Task Initializer(AsyncManualResetEvent mre)
    {
        if (!_token.IsCancellationRequested && _device is IDeviceDefinitionInitialize initializer)
        {
            await ReportDeviceStatus(StreamDeviceStatus.Initializing);
            await initializer.InitializeAsync(_adapter, _token).ConfigureAwait(false);
        }
        await ReportDeviceStatus(StreamDeviceStatus.Initialized);
        mre.Set();
    }

    private Task Receiver(AsyncManualResetEvent mre) => Task.Run(async () =>
    {
        while (!_token.IsCancellationRequested)
        {
            await mre.WaitAsync();
            try
            {
                await ReportDeviceStatus(StreamDeviceStatus.Receiving);
                await AdapterStream.Follow()
                             .With(_segmentDefintion.ThenAs(_decoder, OnMessageReceived))
                             .RunAsync(_token)
                             .ConfigureAwait(false);
                _tokenSource.Cancel(true);
                await ReportDeviceStatus(StreamDeviceStatus.Received);
            }
            catch (Exception ex)
            {
                var eventArg = new DeviceErrorEventArgs(exception: ex, errorHandling: ErrorHandling.Throw);
                MessageReceivedError?.Invoke(AdapterStream, eventArg);
                switch (eventArg.ErrorHandling)
                {
                    case ErrorHandling.Ignore:
                        break;

                    case ErrorHandling.Stop:
                        _tokenSource.Cancel(true);
                        break;

                    default:
                    case ErrorHandling.Throw:
                        throw new IOException(ex.Message, ex);
                }
            }
        }
    });

    private Task Transmitter(AsyncManualResetEvent mre) => Task.Run(async () =>
    {
        var encoder = _encoder;
        if (encoder == null) return;
        while (!_token.IsCancellationRequested)
        {
            await mre.WaitAsync();
            while (!_token.IsCancellationRequested && _transmissionQueue.TryTake(out var item))
            {
                try
                {
                    await ReportDeviceStatus(StreamDeviceStatus.Transmitting);
                    var requestBuffer = encoder.Encode(ref item);
                    await AdapterStream.WriteAsync(requestBuffer, _token)
                                 .ConfigureAwait(false);
                    await ReportDeviceStatus(StreamDeviceStatus.Transmitted);
                }
                catch (Exception ex)
                {
                    var eventArg = new DeviceErrorEventArgs(exception: ex, errorHandling: ErrorHandling.Throw);
                    MessageTransmitterError?.Invoke(AdapterStream, eventArg);
                    switch (eventArg.ErrorHandling)
                    {
                        case ErrorHandling.Ignore:
                            break;

                        case ErrorHandling.Stop:
                            _tokenSource.Cancel(true);
                            break;

                        case ErrorHandling.Throw:
                            throw new IOException(ex.Message, ex);
                    }
                }

                if (!_token.IsCancellationRequested && _minimumTransmissionDelay > 0)
                {
                    await Task.Delay(_minimumTransmissionDelay);
                }
            }
        }
    });

    /// <summary>
    /// Disposes the device, waiting for all operations to complete and cleaning up resources.
    /// </summary>
    public void Dispose()
    {
        Runner.GetAwaiter().GetResult();
        _tokenSource.Cancel(false);
        AdapterStream.Dispose();
    }
}
