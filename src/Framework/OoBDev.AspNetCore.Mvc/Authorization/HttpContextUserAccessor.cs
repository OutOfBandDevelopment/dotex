using Microsoft.AspNetCore.Http;
using OoBDev.System.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

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
