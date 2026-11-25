using System;
using System.Collections.Generic;
using System.Linq;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>Finds a registered <see cref="IWebhookProcessor"/> by provider key.</summary>
public sealed class WebhookProcessorFactory(IEnumerable<IWebhookProcessor> processors) : IWebhookProcessorFactory
{
    /// <inheritdoc />
    public IWebhookProcessor? GetProcessor(string provider)
        => processors.FirstOrDefault(p => string.Equals(p.Provider, provider, StringComparison.OrdinalIgnoreCase));
}
