using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Services;
using System;
using System.Linq;
using System.Reflection;

namespace OoBDev.Communications.Services
{
    public class AttributeResolver : IAttributeResolver
    {
        private readonly string[] _removeThese = new[]
        {
            "oobdev", "persistence", "contracts", "models"
        };

        public string GetMessageType<T>()
        {
            var messageType = typeof(T).GetCustomAttribute<CommunicationAttribute>()?.MessageType;
            if (string.IsNullOrWhiteSpace(messageType))
            {
                messageType = string.Join("_", from segment in typeof(T).FullName.Split('.')
                                               where !_removeThese.Contains(segment, StringComparer.InvariantCultureIgnoreCase)
                                               select segment);
            }
            return messageType;
        }

        public RequestPriorities GetPriority<T>() =>
            typeof(T).GetCustomAttribute<CommunicationAttribute>()?.Priority ?? RequestPriorities.Normal;
    }
}
