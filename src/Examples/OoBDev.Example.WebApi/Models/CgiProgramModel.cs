namespace OoBDev.WebApi.Models;

/// <summary>A CGI program the host can start.</summary>
public class CgiProgramModel
{
    /// <summary>Gets or sets the file name, for example <c>greet.exe</c>.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the URL path that runs the program.</summary>
    public required string Path { get; set; }
}
