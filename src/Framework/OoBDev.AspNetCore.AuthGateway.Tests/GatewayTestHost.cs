using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.AuthGateway.Tests;

internal sealed class GatewayTestHost
{
    public const string ApiKey = "k-123456";

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 23, 10, 0, 0, TimeSpan.Zero));

    public StubTokenEndpoint Endpoint { get; } = new();

    public CapturingLoggerProvider Logs { get; } = new();

    public AuthGatewayOptions Options { get; } = new();

    public GatewayTestHost(OutboundGrant grant = OutboundGrant.ClientCredentials)
    {
        Options.ApiKeys.Add(new ApiKeyEntry { Id = "one", Subject = "caller-one", Sha512 = Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(ApiKey))) });
        Options.Targets["orders"] = new AuthGatewayTarget
        {
            Inbound = [InboundScheme.ApiKey],
            Outbound = new OutboundOptions { Grant = grant, TokenEndpoint = "https://idp.test/token", ClientId = "gateway", ClientSecret = "super-secret", Audience = "orders-api" },
        };
    }

    public AuthGatewayAuthorizer BuildAuthorizer(IEnumerable<IInboundAuthenticator>? extra = null)
    {
        var options = Microsoft.Extensions.Options.Options.Create(Options);
        var tokens = new OAuthTokenProvider(new StubHttpClientFactory(Endpoint), options, Time);
        var authenticators = new List<IInboundAuthenticator> { new ApiKeyAuthenticator(options), new ClientCertificateAuthenticator(options) };
        if (extra is not null)
        {
            authenticators.AddRange(extra);
        }

        var logger = new LoggerFactory([Logs]).CreateLogger<AuthGatewayAuthorizer>();
        return new AuthGatewayAuthorizer(options, authenticators, tokens, logger);
    }

    public static DefaultHttpContext Request(string? apiKey = null, string? bearer = null)
    {
        var context = new DefaultHttpContext();
        if (apiKey is not null)
        {
            context.Request.Headers["X-Api-Key"] = apiKey;
        }

        if (bearer is not null)
        {
            context.Request.Headers.Authorization = "Bearer " + bearer;
        }

        return context;
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }
}

internal sealed class StubTokenEndpoint : HttpMessageHandler
{
    private int _calls;

    public int Calls => _calls;

    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    public int ExpiresIn { get; set; } = 3600;

    public List<string> Bodies { get; } = [];

    public TimeSpan Delay { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var number = Interlocked.Increment(ref _calls);
        lock (Bodies)
        {
            Bodies.Add(request.Content is null ? string.Empty : request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult());
        }

        if (Delay > TimeSpan.Zero)
        {
            await Task.Delay(Delay, cancellationToken);
        }

        return new HttpResponseMessage(Status)
        {
            Content = new StringContent($"{{\"access_token\":\"token-{number}\",\"expires_in\":{ExpiresIn}}}", Encoding.UTF8, "application/json"),
        };
    }
}

internal sealed class CapturingLoggerProvider : ILoggerProvider, ILogger
{
    public List<string> Lines { get; } = [];

    public ILogger CreateLogger(string categoryName) => this;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (Lines)
        {
            Lines.Add(formatter(state, exception));
        }
    }

    public void Dispose()
    {
    }
}
