using OoBDev.Documents;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika;

/// <summary>
/// Represents options for configuring the Apache Tika client.
/// </summary>
[ExcludeFromCodeCoverage]
public class ApacheTikaClientOptions
{
    /// <summary>
    /// Gets or sets the URL of the Apache Tika server.
    /// </summary>
    public string Url { get; set; }
}
