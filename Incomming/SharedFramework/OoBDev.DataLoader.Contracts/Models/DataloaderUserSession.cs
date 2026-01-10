using OoBDev.IdentityModel.Contracts;
using OoBDev.IdentityModel.Contracts.Models;
using System;

namespace OoBDev.DataLoader.Models
{
    public class DataloaderUserSession : IUserSession
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public Guid PersonId { get; set; }
        public string Culture { get; set; }
        public IUserRights Rights { get; set; }
        public IExtendedProperties ExtendedProperties { get; set; }
    }

}
