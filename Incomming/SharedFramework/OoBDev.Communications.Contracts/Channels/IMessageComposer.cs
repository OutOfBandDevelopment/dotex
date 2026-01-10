using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OoBDev.Communications.Contracts.Channels
{
    public interface IMessageComposer
    {
        Task ComposeAndSendAsync(Guid targetPersonId, string messageType, CultureInfo? culture, JObject data, Guid requestGroupId, IDictionary<string, object> headers);
    }
}
