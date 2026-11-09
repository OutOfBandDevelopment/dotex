namespace OoBDev.Documents.Guides;

/// <summary>One run of a command-line tool: what was typed and what came back.</summary>
/// <param name="Title">Heading of the example.</param>
/// <param name="Description">One or two sentences about what the example shows; may be empty.</param>
/// <param name="CommandLine">The command line as a reader would type it (tool name and arguments).</param>
/// <param name="Output">Captured standard output and error, in the order written.</param>
/// <param name="ExitCode">Process exit code.</param>
public sealed record CliExample(string Title, string Description, string CommandLine, string Output, int ExitCode);
