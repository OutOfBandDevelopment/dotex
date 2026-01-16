using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.IdentityModel.Contracts.Models
{
    public interface IExtendedProperties : IEnumerable<IExtendedProperty> //, IEnumerable<string>
    {
        [JsonIgnore]
        IDictionary<string, string> this[string module] { get; }
        [JsonIgnore]
        string this[string module, string name] { get; }
    }
}
