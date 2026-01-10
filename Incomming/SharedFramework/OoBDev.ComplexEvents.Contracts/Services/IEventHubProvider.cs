using OoBDev.Toolkit.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Contracts.Services
{
    [Obsolete("Use IEventHubProvider<TChannel>")]
    public interface IEventHubProvider
    {
        Task SendAsync<TEvent>(
            TEvent item,
            IDictionary<string, object> properties
            ) where TEvent : IEventData;
    }
    [ContractConfig(
        AllowDefault = true,
        ConfigKey = "OoBDev:EventHubProvider:Type"
        )]
#pragma warning disable CS0618 // Type or member is obsolete
    public interface IEventHubProvider<TChannel> : IEventHubProvider
#pragma warning restore CS0618 // Type or member is obsolete
    {
    }
}
