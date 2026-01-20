using System.Collections.Generic;

namespace OoBDev.ComplexEvents.Abstractions
{
    public class ComplexEventData
    {
        public string ClassType { get; set; } = typeof(object).FullName;
        public object? Data { get; set; }
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
