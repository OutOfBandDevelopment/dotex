using OoBDev.IdentityModel.Contracts.Models;
using System;

namespace OoBDev.IdentityModel.Contracts
{
    public interface IUserSession
    {
        Guid UserId { get; }
        string Username { get; }
        Guid PersonId { get; }
        string Culture { get; }

        IUserRights Rights { get; }
        IExtendedProperties ExtendedProperties { get; }
    }
}
