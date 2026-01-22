# API Design: Communications Core Orchestration

**Feature:** Communications Core
**Epic:** Communications Platform
**Last Updated:** 2026-01-22

---

## Overview

This document defines the public API surface for the Communications Core. All interfaces use **framework-agnostic abstractions** to avoid coupling to third-party libraries.

**Key Design Principles:**
- **System.Text.Json** as default JSON implementation (NOT Newtonsoft.Json)
- **IMessageData abstraction** for flexible data structures (Dictionary, Dynamic, or JSON)
- **Async/await** throughout
- **Nullable reference types** enabled
- **Immutability** where appropriate

---

## Core Abstractions

### IMessageData - Message Payload Abstraction

**Purpose:** Framework-agnostic container for message data that supports enhancement, template injection, and serialization.

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Represents message data that can be enhanced and used for template variable substitution.
/// </summary>
public interface IMessageData
{
    /// <summary>
    /// Gets a strongly-typed value from the specified path.
    /// </summary>
    /// <typeparam name="T">Expected type</typeparam>
    /// <param name="path">Property path (e.g., "Order.Customer.Email")</param>
    /// <returns>Value or default(T) if not found</returns>
    T? GetValue<T>(string path);

    /// <summary>
    /// Sets a value at the specified path, creating intermediate objects as needed.
    /// </summary>
    /// <param name="path">Property path</param>
    /// <param name="value">Value to set (null to remove)</param>
    void SetValue(string path, object? value);

    /// <summary>
    /// Attempts to get a value at the specified path.
    /// </summary>
    bool TryGetValue<T>(string path, out T? value);

    /// <summary>
    /// Checks if a value exists at the specified path.
    /// </summary>
    bool ContainsPath(string path);

    /// <summary>
    /// Creates a deep copy of this message data.
    /// </summary>
    IMessageData Clone();

    /// <summary>
    /// Converts to a dictionary for serialization or logging.
    /// </summary>
    IDictionary<string, object?> ToDictionary();

    /// <summary>
    /// Serializes to JSON string.
    /// </summary>
    string ToJson();

    /// <summary>
    /// Gets all keys at the root level.
    /// </summary>
    IEnumerable<string> GetKeys();
}
```

**Usage Examples:**

```csharp
// Create from dictionary
var data = MessageDataFactory.Create(new Dictionary<string, object?>
{
    ["OrderId"] = 12345,
    ["Customer"] = new { Email = "user@example.com", Name = "John" }
});

// Get values with path navigation
var orderId = data.GetValue<int>("OrderId");           // 12345
var email = data.GetValue<string>("Customer.Email");   // "user@example.com"

// Set values (creates intermediate objects)
data.SetValue("Order.LineItems", new[] {
    new { ProductId = 1, Quantity = 2 },
    new { ProductId = 2, Quantity = 1 }
});

// Try get (safe)
if (data.TryGetValue<string>("Customer.Phone", out var phone))
{
    Console.WriteLine($"Phone: {phone}");
}

// Clone for immutability
var clone = data.Clone();
clone.SetValue("Modified", true);  // Original unchanged

// Serialize
var json = data.ToJson();
var dict = data.ToDictionary();
```

**Default Implementation:**

```csharp
namespace OoBDev.Communications;

/// <summary>
/// Default implementation using System.Text.Json.Nodes.JsonObject
/// </summary>
public class JsonMessageData : IMessageData
{
    private readonly JsonObject _data;

    public JsonMessageData()
    {
        _data = new JsonObject();
    }

    public JsonMessageData(JsonObject data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public JsonMessageData(IDictionary<string, object?> dictionary)
    {
        var json = JsonSerializer.Serialize(dictionary);
        _data = JsonNode.Parse(json)!.AsObject();
    }

    public JsonMessageData(string json)
    {
        _data = JsonNode.Parse(json)!.AsObject();
    }

    public T? GetValue<T>(string path)
    {
        var node = NavigatePath(path);
        if (node == null) return default;

        return node.Deserialize<T>();
    }

    public void SetValue(string path, object? value)
    {
        var parts = path.Split('.');
        var current = _data;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];
            if (!current.ContainsKey(part) || current[part] is not JsonObject)
            {
                current[part] = new JsonObject();
            }
            current = current[part]!.AsObject();
        }

        var lastPart = parts[^1];
        if (value == null)
        {
            current.Remove(lastPart);
        }
        else
        {
            current[lastPart] = JsonValue.Create(value);
        }
    }

    public bool TryGetValue<T>(string path, out T? value)
    {
        var node = NavigatePath(path);
        if (node == null)
        {
            value = default;
            return false;
        }

        try
        {
            value = node.Deserialize<T>();
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    public bool ContainsPath(string path)
    {
        return NavigatePath(path) != null;
    }

    public IMessageData Clone()
    {
        var json = _data.ToJsonString();
        return new JsonMessageData(json);
    }

    public IDictionary<string, object?> ToDictionary()
    {
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(_data.ToJsonString())!;
    }

    public string ToJson()
    {
        return _data.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    public IEnumerable<string> GetKeys()
    {
        return _data.Select(kvp => kvp.Key);
    }

    private JsonNode? NavigatePath(string path)
    {
        var parts = path.Split('.');
        JsonNode? current = _data;

        foreach (var part in parts)
        {
            if (current is not JsonObject obj || !obj.TryGetPropertyValue(part, out current))
            {
                return null;
            }
        }

        return current;
    }
}
```

**Factory:**

```csharp
namespace OoBDev.Communications;

/// <summary>
/// Factory for creating IMessageData instances.
/// </summary>
public static class MessageDataFactory
{
    /// <summary>
    /// Creates message data from a dictionary.
    /// </summary>
    public static IMessageData Create(IDictionary<string, object?> data)
    {
        return new JsonMessageData(data);
    }

    /// <summary>
    /// Creates message data from JSON string.
    /// </summary>
    public static IMessageData FromJson(string json)
    {
        return new JsonMessageData(json);
    }

    /// <summary>
    /// Creates empty message data.
    /// </summary>
    public static IMessageData CreateEmpty()
    {
        return new JsonMessageData();
    }

    /// <summary>
    /// Creates message data from an object.
    /// </summary>
    public static IMessageData FromObject(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new JsonMessageData(json);
    }
}
```

---

## Main Entry Points

### ICommunicationProvider - Facade

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Main entry point for sending communications.
/// </summary>
public interface ICommunicationProvider
{
    /// <summary>
    /// Sends a message immediately to the target person.
    /// </summary>
    /// <param name="request">Send request with message details</param>
    /// <param name="headers">Optional custom headers for tracking/routing</param>
    /// <returns>Correlation ID for tracking this message across channels</returns>
    Task<Guid> SendAsync(ISendRequest request, IDictionary<string, object?>? headers = null);

    /// <summary>
    /// Schedules a message for future delivery.
    /// </summary>
    /// <param name="request">Send request with message details</param>
    /// <param name="until">Delivery time (UTC)</param>
    /// <param name="headers">Optional custom headers</param>
    /// <returns>Correlation ID</returns>
    Task<Guid> DeferAsync(ISendRequest request, DateTimeOffset until, IDictionary<string, object?>? headers = null);
}
```

### ISendRequest - Request Model

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Represents a request to send a communication.
/// </summary>
public interface ISendRequest
{
    /// <summary>
    /// Target person identifier (user ID, customer ID, etc.)
    /// </summary>
    Guid TargetPersonId { get; }

    /// <summary>
    /// Message type identifier (e.g., "order.confirmation", "password.reset")
    /// Used to look up templates and enhancement providers.
    /// </summary>
    string MessageType { get; }

    /// <summary>
    /// Message data for template variable substitution and enhancement.
    /// </summary>
    IMessageData Data { get; }

    /// <summary>
    /// Request priority (Normal, High, Critical)
    /// </summary>
    RequestPriorities Priority { get; }
}
```

**Implementation:**

```csharp
namespace OoBDev.Communications;

public class SendRequest : ISendRequest
{
    public Guid TargetPersonId { get; set; }
    public string MessageType { get; set; } = "";
    public IMessageData Data { get; set; } = MessageDataFactory.CreateEmpty();
    public RequestPriorities Priority { get; set; } = RequestPriorities.Normal;

    // Fluent builder methods
    public static SendRequest Create(Guid targetPersonId, string messageType)
    {
        return new SendRequest
        {
            TargetPersonId = targetPersonId,
            MessageType = messageType
        };
    }

    public SendRequest WithData(IDictionary<string, object?> data)
    {
        Data = MessageDataFactory.Create(data);
        return this;
    }

    public SendRequest WithData(IMessageData data)
    {
        Data = data;
        return this;
    }

    public SendRequest WithPriority(RequestPriorities priority)
    {
        Priority = priority;
        return this;
    }
}
```

**Usage:**

```csharp
// Simple usage
var request = SendRequest.Create(userId, "order.confirmation")
    .WithData(new Dictionary<string, object?>
    {
        ["OrderId"] = 12345,
        ["Total"] = 99.99m
    })
    .WithPriority(RequestPriorities.High);

await _communicationProvider.SendAsync(request);

// Advanced usage with IMessageData
var data = MessageDataFactory.CreateEmpty();
data.SetValue("OrderId", 12345);
data.SetValue("Customer.Email", "user@example.com");
data.SetValue("LineItems", new[] { /* ... */ });

var request = new SendRequest
{
    TargetPersonId = userId,
    MessageType = "order.confirmation",
    Data = data,
    Priority = RequestPriorities.Normal
};

var correlationId = await _communicationProvider.SendAsync(request);
```

---

## Orchestration Interfaces

### ICommunicationCentralProcessor

```csharp
namespace OoBDev.Communications.Abstractions.Handler;

/// <summary>
/// Central processor that orchestrates multi-channel message delivery.
/// </summary>
public interface ICommunicationCentralProcessor
{
    /// <summary>
    /// Processes a send request by routing to appropriate channels.
    /// </summary>
    /// <param name="preference">User's channel preferences</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="request">Send request</param>
    /// <param name="headers">Custom headers</param>
    Task HandleRequestAsync(
        ITargetPreference preference,
        Guid correlationId,
        ISendRequest request,
        IDictionary<string, object?> headers);

    /// <summary>
    /// Defers a request for future delivery.
    /// </summary>
    Task DeferRequestAsync(
        ITargetPreference preference,
        Guid correlationId,
        ISendRequest request,
        DateTimeOffset until,
        IDictionary<string, object?> headers);
}
```

### IDataEnhancementManager

```csharp
namespace OoBDev.Communications.Abstractions.Handler;

/// <summary>
/// Manages data enhancement providers that enrich message data.
/// </summary>
public interface IDataEnhancementManager
{
    /// <summary>
    /// Seeds initial data with correlation metadata.
    /// </summary>
    /// <param name="data">Original message data</param>
    /// <param name="metadata">Metadata tuples (key, value)</param>
    /// <returns>Seeded data</returns>
    IMessageData SeedData(IMessageData data, params (string Key, object? Value)[] metadata);

    /// <summary>
    /// Enhances message data by invoking registered enhancement providers.
    /// </summary>
    /// <param name="targetPersonId">Target person identifier</param>
    /// <param name="messageType">Message type</param>
    /// <param name="data">Data to enhance</param>
    /// <returns>Enhanced data</returns>
    Task<IMessageData> EnhanceAsync(Guid targetPersonId, string messageType, IMessageData data);
}
```

### IDataEnhancementProvider

```csharp
namespace OoBDev.Communications.Abstractions.Handler;

/// <summary>
/// Provides domain-specific data enhancement for messages.
/// Implementations should be decorated with [Communication(MessageType)] attribute.
/// </summary>
public interface IDataEnhancementProvider
{
    /// <summary>
    /// Enhances message data with additional context.
    /// </summary>
    /// <param name="targetPersonId">Target person identifier</param>
    /// <param name="messageType">Message type being enhanced</param>
    /// <param name="data">Current message data (may be modified)</param>
    /// <returns>Enhanced data (same instance or new)</returns>
    Task<IMessageData> EnhanceAsync(Guid targetPersonId, string messageType, IMessageData data);
}
```

**Example Enhancement Provider:**

```csharp
[Communication(MessageType = "order.confirmation", Priority = RequestPriorities.High)]
public class OrderConfirmationEnhancementProvider : IDataEnhancementProvider
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderConfirmationEnhancementProvider> _logger;

    public OrderConfirmationEnhancementProvider(
        IOrderRepository orderRepository,
        ILogger<OrderConfirmationEnhancementProvider> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<IMessageData> EnhanceAsync(Guid targetPersonId, string messageType, IMessageData data)
    {
        // Get OrderId from incoming data
        var orderId = data.GetValue<int>("OrderId");

        _logger.LogInformation("Enhancing order confirmation for OrderId {OrderId}", orderId);

        // Load full order details from repository
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        // Enrich data with full order details
        data.SetValue("Order.Id", order.Id);
        data.SetValue("Order.OrderNumber", order.OrderNumber);
        data.SetValue("Order.Total", order.Total);
        data.SetValue("Order.TaxAmount", order.TaxAmount);
        data.SetValue("Order.ShippingCost", order.ShippingCost);
        data.SetValue("Order.CreatedAt", order.CreatedAt);
        data.SetValue("Order.EstimatedDelivery", order.EstimatedDeliveryDate);

        // Add line items
        data.SetValue("Order.LineItems", order.LineItems.Select(li => new
        {
            li.ProductName,
            li.Quantity,
            li.UnitPrice,
            Total = li.Quantity * li.UnitPrice
        }).ToArray());

        // Add shipping address
        data.SetValue("ShippingAddress.Street", order.ShippingAddress.Street);
        data.SetValue("ShippingAddress.City", order.ShippingAddress.City);
        data.SetValue("ShippingAddress.State", order.ShippingAddress.State);
        data.SetValue("ShippingAddress.ZipCode", order.ShippingAddress.ZipCode);

        // Add customer details
        data.SetValue("Customer.Email", order.Customer.Email);
        data.SetValue("Customer.FirstName", order.Customer.FirstName);
        data.SetValue("Customer.LastName", order.Customer.LastName);

        return data;
    }
}
```

---

## Channel Interfaces

### IMessageComposer

```csharp
namespace OoBDev.Communications.Abstractions.Channels;

/// <summary>
/// Composes and sends messages for a specific channel (Email, SMS, Push, etc.)
/// </summary>
public interface IMessageComposer
{
    /// <summary>
    /// Composes a message from template and sends via channel provider.
    /// </summary>
    /// <param name="targetPersonId">Target person identifier</param>
    /// <param name="messageType">Message type (template lookup key)</param>
    /// <param name="culture">User's culture for template selection</param>
    /// <param name="data">Enhanced message data</param>
    /// <param name="requestGroupId">Correlation ID</param>
    /// <param name="headers">Custom headers</param>
    Task ComposeAndSendAsync(
        Guid targetPersonId,
        string messageType,
        CultureInfo? culture,
        IMessageData data,
        Guid requestGroupId,
        IDictionary<string, object?> headers);
}
```

### ISendEmailProvider

```csharp
namespace OoBDev.Communications.Abstractions.Channels;

/// <summary>
/// Sends email messages via a provider (SendGrid, SMTP, etc.)
/// </summary>
public interface ISendEmailProvider
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="message">Email message to send</param>
    Task SendAsync(IEmailMessage message);
}
```

### IEmailMessage

```csharp
namespace OoBDev.Communications.Abstractions.Channels;

/// <summary>
/// Represents an email message to be sent.
/// </summary>
public interface IEmailMessage
{
    /// <summary>
    /// Message type identifier (for tracking)
    /// </summary>
    string? MessageType { get; }

    /// <summary>
    /// Correlation/request ID for tracking
    /// </summary>
    Guid RequestId { get; }

    /// <summary>
    /// From email address
    /// </summary>
    string? FromAddress { get; }

    /// <summary>
    /// From display name
    /// </summary>
    string? FromName { get; }

    /// <summary>
    /// To email addresses (required, at least one)
    /// </summary>
    ICollection<string> ToAddresses { get; }

    /// <summary>
    /// CC email addresses (optional)
    /// </summary>
    ICollection<string> CcAddresses { get; }

    /// <summary>
    /// BCC email addresses (optional)
    /// </summary>
    ICollection<string> BccAddresses { get; }

    /// <summary>
    /// Email subject line
    /// </summary>
    string? Subject { get; }

    /// <summary>
    /// HTML email content (optional, but HtmlContent or TextContent required)
    /// </summary>
    string? HtmlContent { get; }

    /// <summary>
    /// Plain text email content (optional, but HtmlContent or TextContent required)
    /// </summary>
    string? TextContent { get; }

    /// <summary>
    /// Custom headers for tracking/routing
    /// </summary>
    IDictionary<string, object?> Headers { get; }

    /// <summary>
    /// Email attachments (optional)
    /// </summary>
    ICollection<IEmailAttachment>? Attachments { get; }
}
```

### ISendSmsProvider

```csharp
namespace OoBDev.Communications.Abstractions.Channels;

/// <summary>
/// Sends SMS messages via a provider (Twilio, etc.)
/// </summary>
public interface ISendSmsProvider
{
    /// <summary>
    /// Sends an SMS message.
    /// </summary>
    /// <param name="message">SMS message to send</param>
    Task SendAsync(ISmsMessage message);
}
```

### ISmsMessage

```csharp
namespace OoBDev.Communications.Abstractions.Channels;

/// <summary>
/// Represents an SMS message to be sent.
/// </summary>
public interface ISmsMessage
{
    /// <summary>
    /// Message type identifier (for tracking)
    /// </summary>
    string? MessageType { get; }

    /// <summary>
    /// Correlation/request ID for tracking
    /// </summary>
    Guid RequestId { get; }

    /// <summary>
    /// From phone number (E.164 format recommended: +12345678900)
    /// </summary>
    string? FromNumber { get; }

    /// <summary>
    /// To phone number (E.164 format recommended: +12345678900)
    /// </summary>
    string? ToNumber { get; }

    /// <summary>
    /// SMS content (160 chars standard, 1600 chars extended)
    /// </summary>
    string? Content { get; }

    /// <summary>
    /// Custom headers for tracking/routing
    /// </summary>
    IDictionary<string, object?> Headers { get; }
}
```

---

## Preference Management

### ITargetPreferenceManager

```csharp
namespace OoBDev.Communications.Abstractions.Handler;

/// <summary>
/// Manages lookup of user communication preferences.
/// </summary>
public interface ITargetPreferenceManager
{
    /// <summary>
    /// Gets communication preferences for a target person.
    /// </summary>
    /// <param name="targetPersonId">Target person identifier</param>
    /// <returns>Preferences or null if person not found</returns>
    Task<ITargetPreference?> GetPreferenceAsync(Guid targetPersonId);
}
```

### ITargetPreference

```csharp
namespace OoBDev.Communications.Abstractions.Models;

/// <summary>
/// Represents a person's communication channel preferences.
/// </summary>
public interface ITargetPreference
{
    /// <summary>
    /// Person identifier
    /// </summary>
    Guid TargetPersonId { get; }

    /// <summary>
    /// Enabled communication channels (e.g., ["Email", "SMS", "Push"])
    /// </summary>
    string[] Channels { get; }

    /// <summary>
    /// Preferred culture for message templates
    /// </summary>
    CultureInfo Culture { get; }

    /// <summary>
    /// User's timezone (for quiet hours and scheduled delivery)
    /// </summary>
    TimeZoneInfo Timezone { get; }

    /// <summary>
    /// Minimum priority level for messages (Normal, High, Critical)
    /// Only messages at or above this priority will be sent.
    /// </summary>
    RequestPriorities MinimumPriority { get; }

    /// <summary>
    /// Quiet hours start time (local time, e.g., 22:00 = 10 PM)
    /// Messages during quiet hours will be deferred.
    /// </summary>
    TimeOnly? QuietHoursStart { get; }

    /// <summary>
    /// Quiet hours end time (local time, e.g., 08:00 = 8 AM)
    /// </summary>
    TimeOnly? QuietHoursEnd { get; }
}
```

---

## Template Management

### ITemplateProvider

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Provides message templates for composition.
/// </summary>
public interface ITemplateProvider
{
    /// <summary>
    /// Gets an email template (HTML and/or Text).
    /// </summary>
    /// <param name="messageType">Message type identifier</param>
    /// <param name="culture">Culture for localization</param>
    /// <returns>Email template or null if not found</returns>
    Task<IEmailTemplate?> GetEmailTemplateAsync(string messageType, CultureInfo culture);

    /// <summary>
    /// Gets an SMS template.
    /// </summary>
    /// <param name="messageType">Message type identifier</param>
    /// <param name="culture">Culture for localization</param>
    /// <returns>SMS template or null if not found</returns>
    Task<ISmsTemplate?> GetSmsTemplateAsync(string messageType, CultureInfo culture);
}
```

### IEmailTemplate

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Represents an email message template.
/// </summary>
public interface IEmailTemplate
{
    /// <summary>
    /// Message type identifier
    /// </summary>
    string MessageType { get; }

    /// <summary>
    /// Culture for this template
    /// </summary>
    CultureInfo Culture { get; }

    /// <summary>
    /// Email subject template (with {{variables}})
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// HTML content template (optional)
    /// </summary>
    string? HtmlContent { get; }

    /// <summary>
    /// Plain text content template (optional)
    /// At least one of HtmlContent or TextContent must be present.
    /// </summary>
    string? TextContent { get; }

    /// <summary>
    /// Default from address (optional, can be overridden)
    /// </summary>
    string? FromAddress { get; }

    /// <summary>
    /// Default from name (optional)
    /// </summary>
    string? FromName { get; }
}
```

### ISmsTemplate

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Represents an SMS message template.
/// </summary>
public interface ISmsTemplate
{
    /// <summary>
    /// Message type identifier
    /// </summary>
    string MessageType { get; }

    /// <summary>
    /// Culture for this template
    /// </summary>
    CultureInfo Culture { get; }

    /// <summary>
    /// SMS content template (with {{variables}})
    /// Should be ≤ 160 chars for single SMS, ≤ 1600 for extended.
    /// </summary>
    string Content { get; }
}
```

---

## Deferral Management

### IDeferralManager

```csharp
namespace OoBDev.Communications.Abstractions.Handler;

/// <summary>
/// Manages deferred message delivery.
/// </summary>
public interface IDeferralManager
{
    /// <summary>
    /// Posts a deferred request for future delivery.
    /// </summary>
    /// <param name="request">Deferral request with delivery time</param>
    Task PostAsync(DeferralRequestModel request);
}
```

### DeferralRequestModel

```csharp
namespace OoBDev.Communications.Abstractions.Models;

/// <summary>
/// Represents a deferred message request.
/// </summary>
public class DeferralRequestModel
{
    /// <summary>
    /// Correlation ID (preserved from original request)
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Target person identifier
    /// </summary>
    public Guid TargetPersonId { get; set; }

    /// <summary>
    /// Message type identifier
    /// </summary>
    public string MessageType { get; set; } = "";

    /// <summary>
    /// Serialized message data (JSON)
    /// </summary>
    public string ExtendedData { get; set; } = "";

    /// <summary>
    /// Delivery time (UTC)
    /// </summary>
    public DateTimeOffset HoldUntil { get; set; }

    /// <summary>
    /// Request priority
    /// </summary>
    public RequestPriorities Priority { get; set; } = RequestPriorities.Normal;
}
```

---

## Enumerations

### RequestPriorities

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Message priority levels.
/// </summary>
public enum RequestPriorities
{
    /// <summary>
    /// Normal priority (default) - promotional, informational messages
    /// </summary>
    Normal = 0,

    /// <summary>
    /// High priority - important notifications, password resets
    /// </summary>
    High = 1,

    /// <summary>
    /// Critical priority - security alerts, account issues
    /// </summary>
    Critical = 2
}
```

### NotificationCommunicationTypes

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Communication channel types.
/// </summary>
public enum NotificationCommunicationTypes
{
    /// <summary>
    /// Email channel
    /// </summary>
    Email = 0,

    /// <summary>
    /// SMS/Text message channel
    /// </summary>
    Sms = 1,

    /// <summary>
    /// Push notification channel (mobile/web)
    /// </summary>
    Push = 2,

    /// <summary>
    /// In-app notification
    /// </summary>
    InApp = 3,

    /// <summary>
    /// WhatsApp message (future)
    /// </summary>
    WhatsApp = 4,

    /// <summary>
    /// Telegram message (future)
    /// </summary>
    Telegram = 5
}
```

---

## Attributes

### CommunicationAttribute

```csharp
namespace OoBDev.Communications.Abstractions;

/// <summary>
/// Marks an IDataEnhancementProvider for automatic discovery.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class CommunicationAttribute : Attribute
{
    /// <summary>
    /// Message type this provider handles (e.g., "order.confirmation")
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    /// Priority level for this message type
    /// </summary>
    public RequestPriorities Priority { get; set; } = RequestPriorities.Normal;
}
```

---

## Dependency Injection Extensions

### ServiceCollectionExtensions

```csharp
namespace OoBDev.Communications;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core communication services.
    /// </summary>
    public static IServiceCollection TryAddCommunications(this IServiceCollection services)
    {
        // Core services
        services.TryAddSingleton<ICommunicationProvider, CommunicationProvider>();
        services.TryAddSingleton<ICommunicationCentralProcessor, CommunicationCentralProcessor>();
        services.TryAddSingleton<IDataEnhancementManager, DataEnhancementManager>();
        services.TryAddSingleton<IMessageComposerFactory, MessageComposerFactory>();

        // Channel composers
        services.TryAddSingleton<IMessageComposer, EmailMessageComposer>("Email");
        services.TryAddSingleton<IMessageComposer, SmsMessageComposer>("SMS");

        // Support services
        services.TryAddSingleton<ITemplateProvider, FileTemplateProvider>();

        // Options
        services.AddOptions<CommunicationsOptions>()
            .BindConfiguration("OoBDev:Communications")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Registers a data enhancement provider.
    /// </summary>
    public static IServiceCollection AddEnhancementProvider<T>(this IServiceCollection services)
        where T : class, IDataEnhancementProvider
    {
        services.TryAddTransient<IDataEnhancementProvider, T>();
        return services;
    }
}
```

---

## Usage Examples

### Example 1: Simple Send

```csharp
public class OrderService
{
    private readonly ICommunicationProvider _communications;

    public async Task CompleteOrderAsync(Order order)
    {
        // ... complete order ...

        // Send confirmation email + SMS
        var request = SendRequest.Create(order.CustomerId, "order.confirmation")
            .WithData(new Dictionary<string, object?>
            {
                ["OrderId"] = order.Id
            })
            .WithPriority(RequestPriorities.High);

        var correlationId = await _communications.SendAsync(request);

        _logger.LogInformation("Order confirmation sent with correlation {CorrelationId}", correlationId);
    }
}
```

### Example 2: Scheduled Delivery

```csharp
public class MarketingService
{
    public async Task ScheduleCampaignAsync(Guid userId, DateTimeOffset deliveryTime)
    {
        var request = SendRequest.Create(userId, "campaign.spring-sale")
            .WithData(new Dictionary<string, object?>
            {
                ["CampaignCode"] = "SPRING2026",
                ["DiscountPercent"] = 20
            });

        // Schedule for future delivery (e.g., 9 AM local time)
        await _communications.DeferAsync(request, deliveryTime);
    }
}
```

### Example 3: Data Enhancement

```csharp
// Enhancement provider (registered at startup)
[Communication(MessageType = "order.confirmation")]
public class OrderEnhancementProvider : IDataEnhancementProvider
{
    private readonly IOrderRepository _orders;

    public async Task<IMessageData> EnhanceAsync(Guid targetPersonId, string messageType, IMessageData data)
    {
        var orderId = data.GetValue<int>("OrderId");
        var order = await _orders.GetByIdAsync(orderId);

        // Enrich with full order details
        data.SetValue("Order.Total", order.Total);
        data.SetValue("Order.LineItems", order.LineItems);
        data.SetValue("Customer.Email", order.CustomerEmail);

        return data;
    }
}

// Startup registration
services.AddEnhancementProvider<OrderEnhancementProvider>();
```

### Example 4: Advanced IMessageData Usage

```csharp
public async Task SendComplexMessageAsync(Guid userId, Order order)
{
    // Build message data programmatically
    var data = MessageDataFactory.CreateEmpty();

    // Simple values
    data.SetValue("OrderId", order.Id);
    data.SetValue("OrderNumber", order.OrderNumber);

    // Nested objects (creates intermediate objects automatically)
    data.SetValue("Customer.Email", order.Customer.Email);
    data.SetValue("Customer.Name", $"{order.Customer.FirstName} {order.Customer.LastName}");
    data.SetValue("Customer.Phone", order.Customer.PhoneNumber);

    // Arrays
    data.SetValue("LineItems", order.LineItems.Select(li => new
    {
        li.ProductName,
        li.Quantity,
        li.UnitPrice,
        Total = li.Quantity * li.UnitPrice
    }));

    // Complex nested structure
    data.SetValue("ShippingAddress.Street", order.ShippingAddress.Street);
    data.SetValue("ShippingAddress.City", order.ShippingAddress.City);

    var request = new SendRequest
    {
        TargetPersonId = userId,
        MessageType = "order.confirmation",
        Data = data,
        Priority = RequestPriorities.High
    };

    await _communications.SendAsync(request);
}
```

---

## Related Documentation

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Business Rules](./business-rules.md)
- [Configuration](./configuration.md)
- [Testing Strategy](./testing-strategy.md)
