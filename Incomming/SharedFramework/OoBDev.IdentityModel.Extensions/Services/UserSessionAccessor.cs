using OoBDev.IdentityModel.Contracts;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.IdentityModel.Extensions.Services
{
    public class UserSessionAccessor : IUserSessionAccessor
    {
        private readonly IServiceProvider _services;

        public UserSessionAccessor(IServiceProvider services) => _services = services;

        private IUserSession? _userSession;
        public IUserSession? Value
        {
            get => _userSession ?? _services.GetService<IUserSession>();
            set => _userSession = value;
        }
    }
}
