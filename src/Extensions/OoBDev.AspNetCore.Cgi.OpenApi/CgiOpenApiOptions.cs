using System.Collections.Generic;

namespace OoBDev.AspNetCore.Cgi.OpenApi;

/// <summary>
/// Options for describing CGI programs in the OpenAPI documents.
/// </summary>
public class CgiOpenApiOptions
{
    /// <summary>
    /// The suffix of the description file that sits next to a program: <c>greet.exe</c> and <c>greet.cgi</c> are both described by <c>greet.openapi.json</c>.
    /// </summary>
    public string FileSuffix { get; set; } = ".openapi.json";

    /// <summary>
    /// The names of the OpenAPI documents that list the CGI programs.
    /// </summary>
    public IList<string> Documents { get; set; } = ["all"];
}
