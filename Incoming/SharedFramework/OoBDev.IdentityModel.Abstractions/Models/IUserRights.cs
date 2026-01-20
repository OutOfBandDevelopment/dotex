using Newtonsoft.Json;
using System.Collections.Generic;

namespace OoBDev.IdentityModel.Abstractions.Models
{
    public interface IUserRights : IEnumerable<string>
    {
        [JsonIgnore]
        bool this[string rightName] { get; }
    }
}
