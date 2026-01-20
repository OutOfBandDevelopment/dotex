using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions
{
    /// <summary>
    /// Data enhancement allows for adding, changing and removing data in a consistent manner.
    /// This is intended to be used with the text template process and communication provider 
    /// to upgrade small request data into larger payloads required to fully populate messages.
    /// </summary>
    public interface IDataEnhancementProvider
    {
        /// <summary>
        /// using the input target person id, message type and data stub this provider allows
        /// for extending the data available for the template engine.
        /// </summary>
        /// <param name="targetPersonId"></param>
        /// <param name="messageType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<JObject> EnhanceAsync(Guid targetPersonId, string messageType, JObject data);
    }
}
