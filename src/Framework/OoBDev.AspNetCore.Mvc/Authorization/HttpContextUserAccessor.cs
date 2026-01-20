using OoBDev.System.Security;
using System.Security.Principal;

namespace OoBDev.AspNetCore.Mvc.Authorization;
public class HttpContextUserAccessor : ICurrentUserAccessor
{
    private IIdentity _identity;

    public HttpContextUserAccessor(
        IIdentity identity
        )
    {
        _identity = identity;
    }

    public string? UserName => _identity?.Name;
}
