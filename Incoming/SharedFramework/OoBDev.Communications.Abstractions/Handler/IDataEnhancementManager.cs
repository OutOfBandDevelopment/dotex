using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions.Handler
{
    public interface IDataEnhancementManager
    {
        Task<JObject> EnhanceAsync(Guid targetPersonId, string messageType, object? data);

        JObject SeedData(object? data, params (string label, object data)[] items);
    }
}