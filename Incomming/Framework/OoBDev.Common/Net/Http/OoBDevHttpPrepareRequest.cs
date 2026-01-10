using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace OoBDev.Common.Net.Http;

public partial class OoBDevHttpPrepareRequestFeature : IHttpPrepareRequestFeature
{
    private readonly IOptions<OoBDevClientOptions> _options;

    public OoBDevHttpPrepareRequestFeature(
        IOptions<OoBDevClientOptions> options
        )
    {
        _options = options;
    }

    public const string ApplicationKeyHeader = "APPKEY";
    public const string DefaultApiPattern = @$"^/api/(?<{ControllerGroupName}>[^/]+)/?.*$";
    public const string ControllerGroupName = "controllerName";

    [GeneratedRegex(DefaultApiPattern, RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ApiPatternRegex();

    public void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        if (_options == null) return;

        var apiPath = new Uri(url).AbsolutePath;

        var regex = ApiPatternRegex();

        var match = regex.Match(apiPath);
        var controllerName = match.Groups[ControllerGroupName].Value;

        if (!string.IsNullOrEmpty(controllerName) && _options.Value.ApiKeys.TryGetValue(controllerName, out var apiKey))
        {
            request.Headers.TryAddWithoutValidation(ApplicationKeyHeader, apiKey);
        }
    }
}
