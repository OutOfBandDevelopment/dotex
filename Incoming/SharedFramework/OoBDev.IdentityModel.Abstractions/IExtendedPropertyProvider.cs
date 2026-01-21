using OoBDev.IdentityModel.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Abstractions
{
    public interface IExtendedPropertyProvider
    {
        Task<IEnumerable<IExtendedProperty>> GetPropertiesForUserIdAsync(Guid userId);
    }
}
