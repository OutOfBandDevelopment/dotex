using System;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.IdentityModel.Contracts.Models
{
    public interface IExtendedProperty
    {
        string Module { get; }
        string Name { get; }
        string Value { get; }
    }
}
