using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Handler;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace OoBDev.Communications.Handler
{
    public class MessageComposerFactory : IMessageComposerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageComposerFactory(
            IServiceProvider serviceProvider
            )
        {
            _serviceProvider = serviceProvider;
        }

        public IMessageComposer GetComposer(string? channel) =>
            (from provider in _serviceProvider.GetServices<IMessageComposer>()
             let attribute = provider.GetType().GetCustomAttribute<ComposerAttribute>()
             let deliveryChannel = attribute?.DeliveryChannel?.Trim()
             where !string.IsNullOrEmpty(deliveryChannel)
             where string.Equals(channel?.Trim(), deliveryChannel, StringComparison.InvariantCultureIgnoreCase)
             select provider
            ).FirstOrDefault();
    }
}