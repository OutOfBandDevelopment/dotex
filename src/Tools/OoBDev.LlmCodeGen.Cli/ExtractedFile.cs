namespace OoBDev.LlmCodeGen.Cli;

/// <summary>
/// A file found in a model response.
/// </summary>
/// <param name="RelativePath">Path relative to the output folder of the prompt; never rooted and never contains <c>..</c>.</param>
/// <param name="Content">The file content.</param>
public sealed record ExtractedFile(string RelativePath, string Content);
