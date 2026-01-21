using System;

namespace OoBDev.System.MetaData;

/// <summary>
/// Attribute used to associate a class with one or more file extensions.
/// </summary>
/// <param name="fileExtension">The file extension (e.g., ".txt", ".pdf").</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FileExtensionAttribute(string fileExtension) : Attribute
{
    /// <summary>
    /// Gets the file extension associated with the decorated class.
    /// </summary>
    public string FileExtension { get; } = fileExtension;
}
