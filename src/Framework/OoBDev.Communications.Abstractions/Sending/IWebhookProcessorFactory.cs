namespace OoBDev.Communications.Abstractions;

/// <summary>Finds the webhook processor of a provider.</summary>
public interface IWebhookProcessorFactory
{
    /// <summary>Gets the processor for the provider key, or <c>null</c>.</summary>
    IWebhookProcessor? GetProcessor(string provider);
}
