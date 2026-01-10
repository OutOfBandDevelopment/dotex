using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.IdentityModel.Contracts;
using OoBDev.Toolkit.Contracts.Common;
using OoBDev.Toolkit.Contracts.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Services
{
    public class EventHubSource<TChannel> : IEventHubSource<TChannel>
    {
        private readonly ISelectedService<IEventHubProvider<TChannel>> _provider;
        private readonly IEventResolver _resolver;
        private readonly IUserSessionAccessor _user;
        private readonly IDateTools _date;
        private readonly IGuidTools _guid;

        public EventHubSource(
            ISelectedService<IEventHubProvider<TChannel>> provider,
            IEventResolver resolver,
            IUserSessionAccessor user,
            IDateTools date,
            IGuidTools guid
            )
        {
            _provider = provider;
            _resolver = resolver;
            _user = user;
            _date = date;
            _guid = guid;
        }

        internal IEventHubProvider<TChannel> Provider => _provider.Value;

        public async Task SendAsync(
            IEventData item,
            Guid? targetUser = null,
            [CallerMemberName] string? caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string? callerPath = null
            )
        {
            var messageType = _resolver.GetMessageType(item);
            var properties = new Dictionary<string, object>()
            {
                { "ContentType", messageType},
                { "X-Type", item.GetType().FullName},
                { "X-CallerMemberName", caller ?? "UNKNOWN CALLER"},
                { "X-CallerLineNumber", lineNumber},
                { "X-CallerFilePath", callerPath ?? "UNKNOWN CALLER PATH"},

                { "X-UserName",  _user.Value?.Username ?? Environment.UserName},
                { "X-MachineName", Environment.MachineName},

                { "X-SentAt",  _date.Now()},
            };

            if (!_guid.IsNullOrEmpty(_user.Value?.UserId))
                properties.Add("X-CurrentUserId", _user.Value?.UserId ?? Guid.Empty);

            if (targetUser.HasValue && targetUser.Value != Guid.Empty)
                properties.Add("X-TargetUserId", targetUser.Value);

            await Provider.SendAsync(item, properties).ConfigureAwait(false);
        }
    }
}
