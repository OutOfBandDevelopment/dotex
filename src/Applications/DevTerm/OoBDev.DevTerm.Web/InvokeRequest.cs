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
namespace OoBDev.DevTerm.Web;

/// <summary>Body of <c>POST /api/invoke</c>: a control panel command id and its string value (see <see cref="OoBDev.UiDefinitions.IControlSurface"/>).</summary>
public sealed record InvokeRequest(string CommandId, string? Value);
