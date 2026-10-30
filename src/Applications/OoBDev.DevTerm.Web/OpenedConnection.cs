using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm;
namespace OoBDev.DevTerm.Web;

/// <summary>
/// The extra connections the host opens from its <c>--project</c> file, each its own <see cref="SessionHub"/>.
/// Shared by the REST endpoints and the Blazor <c>/connections</c> page so both see the same set.
/// </summary>
/// <summary>An opened project connection; <see cref="ControlPort"/> and <see cref="ControlToken"/> are set only when its profile asked for <c>ControlHttp</c>.</summary>
public sealed record OpenedConnection(string Id, string Name, int? ControlPort = null, string? ControlToken = null);
