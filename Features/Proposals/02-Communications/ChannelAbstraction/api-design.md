# Channel Abstraction - API Design

**Epic:** 2 - Communications Platform
**Feature:** Channel Abstraction
**Last Updated:** 2026-01-22

---

## API Overview

The Channel Abstraction API provides four primary interfaces:
1. **IChannel** - Channel data model (Name + Protocol + Provider)
2. **IChannelProvider** - Send/receive via channel
3. **IChannelRegistry** - Discover and lookup providers
4. **IChannelRepository** - Store and retrieve channels

---

## Core Interfaces

### IChannel

**Purpose:** Data model for communication channels.

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Represents a communication channel with protocol + provider + name pattern.
/// </summary>
public interface IChannel
{
    /// <summary>
    /// Gets the unique channel name (e.g., "support-email", "alerts-sms").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the protocol type (e.g., "email", "sms", "slack").
    /// </summary>
    string Protocol { get; }

    /// <summary>
    /// Gets the provider name (e.g., "sendgrid", "twilio", "slack-api").
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Gets the provider-specific configuration.
    /// </summary>
    IDictionary<string, object> Configuration { get; }

    /// <summary>
    /// Gets or sets whether the channel is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Gets the channel creation timestamp.
    /// </summary>
    DateTimeOffset? CreatedAt { get; }

    /// <summary>
    /// Gets the channel last update timestamp.
    /// </summary>
    DateTimeOffset? UpdatedAt { get; set; }
}
```

---

### IChannelProvider

**Purpose:** Pluggable provider for sending/receiving messages via channels.

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Provides send/receive operations for a specific channel provider.
/// </summary>
public interface IChannelProvider
{
    /// <summary>
    /// Gets the provider name (e.g., "sendgrid", "twilio").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the protocols supported by this provider (e.g., ["email"], ["sms"]).
    /// </summary>
    string[] SupportedProtocols { get; }

    /// <summary>
    /// Gets whether this provider supports sending messages.
    /// </summary>
    bool SupportsSending { get; }

    /// <summary>
    /// Gets whether this provider supports receiving messages (polling).
    /// </summary>
    bool SupportsReceiving { get; }

    /// <summary>
    /// Gets whether this provider supports webhook registration.
    /// </summary>
    bool SupportsWebhooks { get; }

    /// <summary>
    /// Checks if this provider can send the message via the channel.
    /// </summary>
    /// <param name="channel">Target channel</param>
    /// <param name="message">Message to send</param>
    /// <returns>True if provider can send message</returns>
    Task<bool> CanSendAsync(IChannel channel, IMessage message);

    /// <summary>
    /// Sends message via the channel.
    /// </summary>
    /// <param name="channel">Target channel</param>
    /// <param name="message">Message to send</param>
    /// <returns>Send result with message ID and status</returns>
    /// <exception cref="ChannelException">Channel configuration invalid</exception>
    /// <exception cref="ProviderException">Provider API error</exception>
    Task<SendResult> SendAsync(IChannel channel, IMessage message);

    /// <summary>
    /// Receives message from channel (polling-based).
    /// </summary>
    /// <param name="channel">Source channel</param>
    /// <returns>Received message or null if no messages available</returns>
    /// <exception cref="NotSupportedException">Provider does not support polling</exception>
    Task<IMessage?> ReceiveAsync(IChannel channel);

    /// <summary>
    /// Registers webhook for inbound messages.
    /// </summary>
    /// <param name="channel">Target channel</param>
    /// <param name="webhookUrl">Webhook URL for inbound messages</param>
    /// <exception cref="NotSupportedException">Provider does not support webhooks</exception>
    Task RegisterWebhookAsync(IChannel channel, string webhookUrl);

    /// <summary>
    /// Unregisters webhook for inbound messages.
    /// </summary>
    /// <param name="channel">Target channel</param>
    Task UnregisterWebhookAsync(IChannel channel);
}
```

---

### IChannelRegistry

**Purpose:** Registry for discovering and looking up channel providers.

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Registry for channel providers.
/// </summary>
public interface IChannelRegistry
{
    /// <summary>
    /// Registers a channel provider.
    /// </summary>
    /// <param name="provider">Provider to register</param>
    void RegisterProvider(IChannelProvider provider);

    /// <summary>
    /// Gets provider by protocol and provider name.
    /// </summary>
    /// <param name="protocol">Protocol (e.g., "email")</param>
    /// <param name="providerName">Provider name (e.g., "sendgrid")</param>
    /// <returns>Provider instance or null if not found</returns>
    IChannelProvider? GetProvider(string protocol, string providerName);

    /// <summary>
    /// Gets all providers supporting the specified protocol.
    /// </summary>
    /// <param name="protocol">Protocol (e.g., "email")</param>
    /// <returns>Collection of providers</returns>
    IEnumerable<IChannelProvider> GetProvidersByProtocol(string protocol);

    /// <summary>
    /// Gets all supported protocols.
    /// </summary>
    /// <returns>Collection of protocol names</returns>
    IEnumerable<string> GetSupportedProtocols();
}
```

---

### IChannelRepository

**Purpose:** Repository for storing and retrieving channels.

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Repository for channel persistence.
/// </summary>
public interface IChannelRepository
{
    /// <summary>
    /// Gets channel by unique name.
    /// </summary>
    /// <param name="name">Channel name</param>
    /// <returns>Channel or null if not found</returns>
    Task<IChannel?> GetByNameAsync(string name);

    /// <summary>
    /// Gets all channels for a protocol.
    /// </summary>
    /// <param name="protocol">Protocol (e.g., "email")</param>
    /// <returns>Collection of channels</returns>
    Task<IEnumerable<IChannel>> GetByProtocolAsync(string protocol);

    /// <summary>
    /// Gets all channels.
    /// </summary>
    /// <returns>Collection of all channels</returns>
    Task<IEnumerable<IChannel>> GetAllAsync();

    /// <summary>
    /// Creates a new channel.
    /// </summary>
    /// <param name="channel">Channel to create</param>
    /// <returns>Created channel</returns>
    /// <exception cref="InvalidOperationException">Channel with name already exists</exception>
    Task<IChannel> CreateAsync(IChannel channel);

    /// <summary>
    /// Updates an existing channel.
    /// </summary>
    /// <param name="channel">Channel to update</param>
    /// <exception cref="InvalidOperationException">Channel not found</exception>
    Task UpdateAsync(IChannel channel);

    /// <summary>
    /// Deletes a channel (soft-delete/archive).
    /// </summary>
    /// <param name="name">Channel name</param>
    /// <exception cref="InvalidOperationException">Channel not found</exception>
    Task DeleteAsync(string name);
}
```

---

## Factory & Builder

### ChannelFactory

**Purpose:** Factory for creating channel instances.

```csharp
namespace OoBDev.Communications;

/// <summary>
/// Factory for creating IChannel instances.
/// </summary>
public static class ChannelFactory
{
    /// <summary>
    /// Creates email channel.
    /// </summary>
    /// <param name="name">Channel name</param>
    /// <param name="provider">Provider (e.g., "sendgrid", "smtp")</param>
    /// <param name="configuration">Provider configuration</param>
    public static IChannel CreateEmailChannel(
        string name,
        string provider,
        IDictionary<string, object> configuration)
    {
        return new Channel(name, "email", provider)
        {
            Configuration = configuration
        };
    }

    /// <summary>
    /// Creates SMS channel.
    /// </summary>
    /// <param name="name">Channel name</param>
    /// <param name="provider">Provider (e.g., "twilio", "aws-sns")</param>
    /// <param name="configuration">Provider configuration</param>
    public static IChannel CreateSmsChannel(
        string name,
        string provider,
        IDictionary<string, object> configuration)
    {
        return new Channel(name, "sms", provider)
        {
            Configuration = configuration
        };
    }

    /// <summary>
    /// Creates Slack channel.
    /// </summary>
    /// <param name="name">Channel name</param>
    /// <param name="webhookUrl">Slack webhook URL</param>
    /// <param name="channel">Slack channel (e.g., "#sales")</param>
    public static IChannel CreateSlackChannel(
        string name,
        string webhookUrl,
        string channel)
    {
        return new Channel(name, "slack", "slack-api")
        {
            Configuration = new Dictionary<string, object>
            {
                ["WebhookUrl"] = webhookUrl,
                ["Channel"] = channel
            }
        };
    }

    /// <summary>
    /// Creates custom channel.
    /// </summary>
    /// <param name="name">Channel name</param>
    /// <param name="protocol">Protocol</param>
    /// <param name="provider">Provider name</param>
    /// <param name="configuration">Configuration</param>
    public static IChannel Create(
        string name,
        string protocol,
        string provider,
        IDictionary<string, object>? configuration = null)
    {
        var channel = new Channel(name, protocol, provider);
        if (configuration != null)
        {
            channel.Configuration = configuration;
        }
        return channel;
    }
}
```

---

## Usage Examples

### Example 1: Create Email Channel (SendGrid)

```csharp
using OoBDev.Communications;
using OoBDev.Communications.Abstractions;

// Create SendGrid email channel
var emailChannel = ChannelFactory.CreateEmailChannel(
    name: "support-email",
    provider: "sendgrid",
    configuration: new Dictionary<string, object>
    {
        ["ApiKey"] = "SG.xxxxxxxxxxxxxxxxxxxxxxxx",
        ["FromEmail"] = "support@company.com",
        ["FromName"] = "Company Support",
        ["ReplyToEmail"] = "noreply@company.com"
    }
);

// Save channel to repository
await _channelRepository.CreateAsync(emailChannel);

Console.WriteLine($"Created channel: {emailChannel.Name}");
Console.WriteLine($"Protocol: {emailChannel.Protocol}");
Console.WriteLine($"Provider: {emailChannel.Provider}");

// Output:
// Created channel: support-email
// Protocol: email
// Provider: sendgrid
```

---

### Example 2: Create SMS Channel (Twilio)

```csharp
// Create Twilio SMS channel
var smsChannel = ChannelFactory.CreateSmsChannel(
    name: "alerts-sms",
    provider: "twilio",
    configuration: new Dictionary<string, object>
    {
        ["AccountSid"] = "AC123456789abcdef",
        ["AuthToken"] = "your_auth_token",
        ["FromPhoneNumber"] = "+15551234567",
        ["EnableDeliveryReceipts"] = true
    }
);

// Save channel
await _channelRepository.CreateAsync(smsChannel);

Console.WriteLine($"Created SMS channel: {smsChannel.Name}");
```

---

### Example 3: Create Slack Channel

```csharp
// Create Slack channel for sales team
var slackChannel = ChannelFactory.CreateSlackChannel(
    name: "sales-team-slack",
    webhookUrl: "https://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXXXXXXXXXX",
    channel: "#sales"
);

// Save channel
await _channelRepository.CreateAsync(slackChannel);

// Send message via Slack
var message = new TextMessage
{
    Content = "New lead: John Doe - john@example.com"
};

var provider = _channelRegistry.GetProvider("slack", "slack-api");
var result = await provider.SendAsync(slackChannel, message);

if (result.Success)
{
    Console.WriteLine($"Message sent to Slack: {result.MessageId}");
}
```

---

### Example 4: Discover Channels by Protocol

```csharp
// Get all email channels
var emailChannels = await _channelRepository.GetByProtocolAsync("email");

Console.WriteLine($"Found {emailChannels.Count()} email channels:");
foreach (var channel in emailChannels)
{
    Console.WriteLine($"  - {channel.Name} ({channel.Provider})");
}

// Output:
// Found 3 email channels:
//   - support-email (sendgrid)
//   - marketing-email (mailchimp)
//   - transactional-email (smtp)

// Get all SMS channels
var smsChannels = await _channelRepository.GetByProtocolAsync("sms");

Console.WriteLine($"\nFound {smsChannels.Count()} SMS channels:");
foreach (var channel in smsChannels)
{
    Console.WriteLine($"  - {channel.Name} ({channel.Provider})");
}

// Output:
// Found 2 SMS channels:
//   - alerts-sms (twilio)
//   - otp-sms (aws-sns)
```

---

### Example 5: Send Message via Channel

```csharp
// Get channel by name
var channel = await _channelRepository.GetByNameAsync("support-email");

if (channel == null)
{
    Console.WriteLine("Channel not found");
    return;
}

// Get provider from registry
var provider = _channelRegistry.GetProvider(channel.Protocol, channel.Provider);

if (provider == null)
{
    Console.WriteLine($"Provider '{channel.Provider}' not found for protocol '{channel.Protocol}'");
    return;
}

// Create email message
var emailMessage = new EmailMessage
{
    To = new[] { "customer@example.com" },
    Subject = "Order Confirmation",
    HtmlContent = "<h1>Thank you for your order!</h1>",
    TextContent = "Thank you for your order!"
};

// Check if provider can send
if (await provider.CanSendAsync(channel, emailMessage))
{
    // Send message
    var result = await provider.SendAsync(channel, emailMessage);

    if (result.Success)
    {
        Console.WriteLine($"Email sent successfully: {result.MessageId}");
    }
    else
    {
        Console.WriteLine($"Failed to send email: {result.ErrorMessage}");
    }
}
else
{
    Console.WriteLine("Provider cannot send this message");
}
```

---

### Example 6: Provider Registry Discovery

```csharp
// Get all supported protocols
var protocols = _channelRegistry.GetSupportedProtocols();

Console.WriteLine("Supported protocols:");
foreach (var protocol in protocols)
{
    Console.WriteLine($"  - {protocol}");

    // Get providers for this protocol
    var providers = _channelRegistry.GetProvidersByProtocol(protocol);
    foreach (var provider in providers)
    {
        Console.WriteLine($"    - {provider.ProviderName}");
    }
}

// Output:
// Supported protocols:
//   - email
//     - sendgrid
//     - smtp
//     - mailkit
//   - sms
//     - twilio
//     - aws-sns
//   - slack
//     - slack-api
//   - teams
//     - microsoft-teams
//   - push
//     - firebase
//     - apns
```

---

### Example 7: Custom Channel Provider Implementation

```csharp
/// <summary>
/// Custom email provider using SMTP.
/// </summary>
public class SmtpEmailProvider : IChannelProvider
{
    private readonly ILogger<SmtpEmailProvider> _logger;

    public string ProviderName => "smtp";
    public string[] SupportedProtocols => new[] { "email" };
    public bool SupportsSending => true;
    public bool SupportsReceiving => false;
    public bool SupportsWebhooks => false;

    public SmtpEmailProvider(ILogger<SmtpEmailProvider> logger)
    {
        _logger = logger;
    }

    public Task<bool> CanSendAsync(IChannel channel, IMessage message)
    {
        // Validate channel configuration
        if (!channel.Configuration.ContainsKey("SmtpHost"))
            return Task.FromResult(false);

        if (!channel.Configuration.ContainsKey("SmtpPort"))
            return Task.FromResult(false);

        // Validate message type
        if (message is not IEmailMessage)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }

    public async Task<SendResult> SendAsync(IChannel channel, IMessage message)
    {
        if (message is not IEmailMessage emailMessage)
        {
            throw new ArgumentException("Message must be IEmailMessage", nameof(message));
        }

        // Extract SMTP configuration
        var smtpHost = channel.Configuration["SmtpHost"].ToString();
        var smtpPort = Convert.ToInt32(channel.Configuration["SmtpPort"]);
        var username = channel.Configuration.GetValueOrDefault("Username", "")?.ToString();
        var password = channel.Configuration.GetValueOrDefault("Password", "")?.ToString();

        _logger.LogDebug("Sending email via SMTP: {Host}:{Port}", smtpHost, smtpPort);

        try
        {
            // Send email using MailKit or SmtpClient
            using var client = new SmtpClient(smtpHost, smtpPort);

            if (!string.IsNullOrEmpty(username))
            {
                await client.AuthenticateAsync(username, password);
            }

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(
                channel.Configuration["FromName"]?.ToString(),
                channel.Configuration["FromEmail"]?.ToString()));
            mimeMessage.To.Add(new MailboxAddress("", emailMessage.To.First()));
            mimeMessage.Subject = emailMessage.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = emailMessage.HtmlContent,
                TextBody = emailMessage.TextContent
            };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully via SMTP");

            return new SendResult
            {
                Success = true,
                MessageId = mimeMessage.MessageId,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP");

            return new SendResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }

    public Task<IMessage?> ReceiveAsync(IChannel channel)
    {
        throw new NotSupportedException("SMTP does not support receiving messages");
    }

    public Task RegisterWebhookAsync(IChannel channel, string webhookUrl)
    {
        throw new NotSupportedException("SMTP does not support webhooks");
    }

    public Task UnregisterWebhookAsync(IChannel channel)
    {
        throw new NotSupportedException("SMTP does not support webhooks");
    }
}

// Register custom provider
services.AddSingleton<IChannelProvider, SmtpEmailProvider>();

// Use custom provider
var channel = ChannelFactory.CreateEmailChannel(
    name: "smtp-email",
    provider: "smtp",
    configuration: new Dictionary<string, object>
    {
        ["SmtpHost"] = "smtp.company.com",
        ["SmtpPort"] = 587,
        ["Username"] = "noreply@company.com",
        ["Password"] = "password123",
        ["FromEmail"] = "noreply@company.com",
        ["FromName"] = "Company Name"
    }
);

await _channelRepository.CreateAsync(channel);
```

---

### Example 8: Update Channel Configuration

```csharp
// Get existing channel
var channel = await _channelRepository.GetByNameAsync("support-email");

if (channel != null)
{
    // Update API key (e.g., after rotation)
    channel.Configuration["ApiKey"] = "SG.new_api_key_here";

    // Update from email
    channel.Configuration["FromEmail"] = "newsupport@company.com";

    // Save changes
    await _channelRepository.UpdateAsync(channel);

    Console.WriteLine($"Updated channel: {channel.Name}");
}
```

---

### Example 9: Enable/Disable Channel

```csharp
// Temporarily disable channel (e.g., for maintenance)
var channel = await _channelRepository.GetByNameAsync("alerts-sms");

if (channel != null)
{
    channel.IsEnabled = false;
    await _channelRepository.UpdateAsync(channel);

    Console.WriteLine($"Disabled channel: {channel.Name}");
}

// Later: Re-enable channel
channel = await _channelRepository.GetByNameAsync("alerts-sms");
if (channel != null)
{
    channel.IsEnabled = true;
    await _channelRepository.UpdateAsync(channel);

    Console.WriteLine($"Enabled channel: {channel.Name}");
}

// Check if channel is enabled before sending
if (channel.IsEnabled)
{
    var provider = _channelRegistry.GetProvider(channel.Protocol, channel.Provider);
    await provider.SendAsync(channel, message);
}
else
{
    Console.WriteLine("Channel is disabled");
}
```

---

## Extension Methods

### Channel Extensions

```csharp
namespace OoBDev.Communications.Extensions;

public static class ChannelExtensions
{
    /// <summary>
    /// Gets configuration value with default.
    /// </summary>
    public static T? GetConfigurationValue<T>(
        this IChannel channel,
        string key,
        T? defaultValue = default)
    {
        if (channel.Configuration.TryGetValue(key, out var value))
        {
            return (T?)value;
        }
        return defaultValue;
    }

    /// <summary>
    /// Checks if channel has required configuration keys.
    /// </summary>
    public static bool HasRequiredConfiguration(
        this IChannel channel,
        params string[] requiredKeys)
    {
        foreach (var key in requiredKeys)
        {
            if (!channel.Configuration.ContainsKey(key))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Validates channel configuration.
    /// </summary>
    public static bool ValidateConfiguration(this IChannel channel)
    {
        switch (channel.Protocol)
        {
            case "email":
                return channel.HasRequiredConfiguration("ApiKey", "FromEmail");

            case "sms":
                return channel.HasRequiredConfiguration("AccountSid", "AuthToken", "FromPhoneNumber");

            case "slack":
                return channel.HasRequiredConfiguration("WebhookUrl", "Channel");

            default:
                return true;  // Custom protocols validated by provider
        }
    }
}
```

---

## Error Handling

### Exception Types

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Base exception for channel errors.
/// </summary>
public class ChannelException : Exception
{
    public string? ChannelName { get; }
    public string? Protocol { get; }

    public ChannelException(string message, string? channelName = null)
        : base(message)
    {
        ChannelName = channelName;
    }

    public ChannelException(string message, Exception innerException, string? channelName = null)
        : base(message, innerException)
    {
        ChannelName = channelName;
    }
}

/// <summary>
/// Exception thrown when channel not found.
/// </summary>
public class ChannelNotFoundException : ChannelException
{
    public ChannelNotFoundException(string channelName)
        : base($"Channel '{channelName}' not found", channelName)
    {
    }
}

/// <summary>
/// Exception thrown when provider not found.
/// </summary>
public class ProviderNotFoundException : ChannelException
{
    public string? ProviderName { get; }

    public ProviderNotFoundException(string protocol, string providerName)
        : base($"Provider '{providerName}' not found for protocol '{protocol}'")
    {
        Protocol = protocol;
        ProviderName = providerName;
    }
}

/// <summary>
/// Exception thrown when provider operation fails.
/// </summary>
public class ProviderException : ChannelException
{
    public ProviderException(string message, Exception innerException, string? channelName = null)
        : base(message, innerException, channelName)
    {
    }
}

/// <summary>
/// Exception thrown when channel configuration is invalid.
/// </summary>
public class ChannelConfigurationException : ChannelException
{
    public string? ConfigurationKey { get; }

    public ChannelConfigurationException(string message, string channelName, string? configurationKey = null)
        : base(message, channelName)
    {
        ConfigurationKey = configurationKey;
    }
}
```

### Error Handling Example

```csharp
try
{
    // Get channel
    var channel = await _channelRepository.GetByNameAsync("support-email");

    if (channel == null)
    {
        throw new ChannelNotFoundException("support-email");
    }

    // Validate configuration
    if (!channel.ValidateConfiguration())
    {
        throw new ChannelConfigurationException(
            "Missing required configuration",
            channel.Name);
    }

    // Get provider
    var provider = _channelRegistry.GetProvider(channel.Protocol, channel.Provider);

    if (provider == null)
    {
        throw new ProviderNotFoundException(channel.Protocol, channel.Provider);
    }

    // Send message
    var result = await provider.SendAsync(channel, message);

    if (!result.Success)
    {
        throw new ProviderException(
            $"Failed to send message: {result.ErrorMessage}",
            new Exception(result.ErrorMessage),
            channel.Name);
    }
}
catch (ChannelNotFoundException ex)
{
    _logger.LogError(ex, "Channel not found: {ChannelName}", ex.ChannelName);
}
catch (ProviderNotFoundException ex)
{
    _logger.LogError(ex, "Provider not found: {Protocol}/{Provider}", ex.Protocol, ex.ProviderName);
}
catch (ChannelConfigurationException ex)
{
    _logger.LogError(ex, "Invalid channel configuration: {ChannelName}", ex.ChannelName);
}
catch (ProviderException ex)
{
    _logger.LogError(ex, "Provider error: {ChannelName}", ex.ChannelName);
}
```

---

## Best Practices

### 1. Channel Naming Convention
```csharp
// ✅ GOOD: Descriptive, purpose-based names
"support-email"
"alerts-sms"
"sales-team-slack"
"transactional-email"

// ❌ BAD: Generic, ambiguous names
"email1"
"sms"
"channel"
```

### 2. Configuration Security
```csharp
// ✅ GOOD: Encrypt sensitive data before storing
var apiKey = _encryption.Encrypt("SG.xxx");
channel.Configuration["ApiKey"] = apiKey;

// ❌ BAD: Store API keys in plain text
channel.Configuration["ApiKey"] = "SG.xxx";  // Security risk!
```

### 3. Provider Selection
```csharp
// ✅ GOOD: Check if provider can send before sending
if (await provider.CanSendAsync(channel, message))
{
    await provider.SendAsync(channel, message);
}

// ❌ BAD: Send without checking
await provider.SendAsync(channel, message);  // May fail!
```

### 4. Channel Repository Caching
```csharp
// ✅ GOOD: Repository handles caching automatically
var channel = await _channelRepository.GetByNameAsync("support-email");

// ❌ BAD: Manually caching channels
// Don't do this - repository already caches!
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 2 Overview](../README-REVISED.md)
