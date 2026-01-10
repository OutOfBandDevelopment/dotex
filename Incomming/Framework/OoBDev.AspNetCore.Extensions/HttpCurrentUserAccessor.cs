using OoBDev.Common;
using Microsoft.AspNetCore.Http;

namespace OoBDev.AspNetCore.Extensions;

public class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private IHttpContextAccessor _contextAccessor;

    public HttpCurrentUserAccessor(
        IHttpContextAccessor contextAccessor
        )
    {
        _contextAccessor = contextAccessor;
    }

    private string? _currentUser;

    public string? UserName
    {
        get => string.IsNullOrWhiteSpace(_currentUser) ? _contextAccessor.HttpContext?.Request.Headers["X-CurrentUser"] : _currentUser;
        set => _currentUser = value;
    }
}
