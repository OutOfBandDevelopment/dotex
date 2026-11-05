using OoBDev.System.IO;
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
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm;
using OoBDev.DevTerm.Logging;
namespace OoBDev.DevTerm.Web;

/// <summary>A broker-to-device message awaiting the viewer's decision.</summary>
public sealed class PendingConfirm(int id, string topic, string text)
{
    public int Id { get; } = id;

    public string Topic { get; } = topic;

    public string Text { get; } = text;

    internal TaskCompletionSource<RoutingConfirmChoice> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}
