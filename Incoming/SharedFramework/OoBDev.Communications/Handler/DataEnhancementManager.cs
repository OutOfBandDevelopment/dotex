using OoBDev.Communications.Contracts.Handler;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Handler
{
    public class DataEnhancementManager : IDataEnhancementManager
    {
        private readonly IDataEnhancementProviderFactory _providers;

        public DataEnhancementManager(
            IDataEnhancementProviderFactory providers
            ) => _providers = providers;

        public async Task<JObject> EnhanceAsync(Guid targetPersonId, string messageType, object? data)
        {
            try
            {
                var source = _providers.GetData(data);
                var providers = _providers.GetProviders(messageType);
                foreach (var provider in providers)
                {
                    source = await provider.EnhanceAsync(targetPersonId, messageType, source ?? new JObject());
                }
                return source;
            }
            catch (Exception ex)
            {
                throw new DataEnhancementException($"Enhancement failed for \"{messageType}\" to {targetPersonId}", ex);
            }
        }

        public JObject SeedData(object? data, params (string label, object data)[] items)
        {
            var source = _providers.GetData(data);
            foreach (var item in items)
            {
                if (!string.IsNullOrWhiteSpace(item.label) && item.data != null && source[item.label] == null)
                    source[item.label] = _providers.GetData(item.data);
            }
            return source;
        }
    }
}