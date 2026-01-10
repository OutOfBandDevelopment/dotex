using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace OoBDev.Communications.Contracts.Handler
{
    public interface IDataEnhancementProviderFactory
    {
        JObject GetData(object? data);
        IEnumerable<IDataEnhancementProvider> GetProviders(string messageType);
        int TotalProviderCount { get; }
    }
}