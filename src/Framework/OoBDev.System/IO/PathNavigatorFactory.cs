using OoBDev.System.Xml.XPath;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.IO;

/// <summary>
/// Factory methods for creating XPath navigators and nodes from file system directory structures.
/// Converts DirectoryInfo objects into XPath-queryable representations with file and directory metadata as attributes.
/// </summary>
public static class PathNavigatorFactory
{
    /// <summary>
    /// Converts a DirectoryInfo object into an XPath-navigable representation.
    /// </summary>
    /// <param name="dir">The directory to convert.</param>
    /// <param name="rootName">The XML name to use for the root element. Defaults to "Directory".</param>
    /// <param name="baseUri">The base URI for the navigator. Optional.</param>
    /// <returns>An IXPathNavigable object representing the directory structure.</returns>
    public static IXPathNavigable ToNavigable(this DirectoryInfo dir, XName? rootName = null, string? baseUri = null) =>
        new ExtensibleNavigator(dir.AsNode(rootName, baseUri));

    /// <summary>
    /// Converts a DirectoryInfo object into an INode representation suitable for XPath navigation.
    /// Creates a tree structure with directories and files as child nodes, and file system metadata as attributes.
    /// </summary>
    /// <param name="dir">The directory to convert.</param>
    /// <param name="rootName">The XML name to use for the root element. Defaults to "Directory".</param>
    /// <param name="baseUri">The base URI for the node. Optional.</param>
    /// <returns>An INode representing the directory structure with metadata.</returns>
    public static INode AsNode(this DirectoryInfo dir, XName? rootName = null, string? baseUri = null)
    {
        if (rootName == null || string.IsNullOrWhiteSpace(rootName.LocalName))
            rootName = "Directory";

        var trimLen = dir.FullName.Length;

        return new ExtensibleElementNode(
            rootName,
            dir,

              //valueSelector: v => v switch
              //{
              //    FileInfo file => file.Name,
              //    DirectoryInfo directory => directory.Name,
              //    _ => throw new NotSupportedException(),
              //},

              attributeSelector: a => a switch
              {
                  DirectoryInfo directory => new[]
                  {
                      (XName.Get(nameof(directory.Attributes), rootName.NamespaceName), directory.Attributes.ToString()),
                      (XName.Get(nameof(directory.CreationTime), rootName.NamespaceName), directory.CreationTime.ToString()),
                      (XName.Get(nameof(directory.CreationTimeUtc), rootName.NamespaceName), directory.CreationTimeUtc.ToString()),
                      (XName.Get(nameof(directory.Extension), rootName.NamespaceName), directory.Extension),
                      (XName.Get(nameof(directory.FullName), rootName.NamespaceName), directory.FullName),
                      (XName.Get("RelativeName", rootName.NamespaceName), directory.FullName[trimLen..]),
                      (XName.Get(nameof(directory.LastAccessTime), rootName.NamespaceName), directory.LastAccessTime.ToString()),
                      (XName.Get(nameof(directory.LastAccessTimeUtc), rootName.NamespaceName), directory.LastAccessTimeUtc.ToString()),
                      (XName.Get(nameof(directory.LastWriteTime), rootName.NamespaceName), directory.LastWriteTime.ToString()),
                      (XName.Get(nameof(directory.LastWriteTimeUtc), rootName.NamespaceName), directory.LastWriteTimeUtc.ToString()),
                      (XName.Get(nameof(directory.Name), rootName.NamespaceName), directory.Name),
                      (XName.Get("WithoutExtension", rootName.NamespaceName), Path.GetFileNameWithoutExtension( directory.Name)),
                  }.Select(i => (i.Item1, (string?)i.Item2)),

                  FileInfo file => new[]
                  {
                      (XName.Get(nameof(file.Attributes), rootName.NamespaceName), file.Attributes.ToString()),
                      (XName.Get(nameof(file.CreationTime), rootName.NamespaceName), file.CreationTime.ToString()),
                      (XName.Get(nameof(file.CreationTimeUtc), rootName.NamespaceName), file.CreationTimeUtc.ToString()),
                      (XName.Get(nameof(file.Extension), rootName.NamespaceName), file.Extension),
                      (XName.Get(nameof(file.FullName), rootName.NamespaceName), file.FullName),
                      (XName.Get("RelativeName", rootName.NamespaceName), file.FullName[trimLen..]),
                      (XName.Get(nameof(file.IsReadOnly), rootName.NamespaceName), file.IsReadOnly.ToString()),
                      (XName.Get(nameof(file.LastAccessTime), rootName.NamespaceName), file.LastAccessTime.ToString()),
                      (XName.Get(nameof(file.LastAccessTimeUtc), rootName.NamespaceName), file.LastAccessTimeUtc.ToString()),
                      (XName.Get(nameof(file.LastWriteTime), rootName.NamespaceName), file.LastWriteTime.ToString()),
                      (XName.Get(nameof(file.LastWriteTimeUtc), rootName.NamespaceName), file.LastWriteTimeUtc.ToString()),
                      (XName.Get(nameof(file.Length), rootName.NamespaceName), file.Length.ToString()),
                      (XName.Get(nameof(file.Name), rootName.NamespaceName), file.Name),
                      (XName.Get("WithoutExtension", rootName.NamespaceName), Path.GetFileNameWithoutExtension( file.Name)),
                  }.Select(i => (i.Item1, (string?)i.Item2)),

                  _ => throw new NotSupportedException(),
              },

             childSelector: c => c switch
             {
                 DirectoryInfo directory => directory.GetDirectories().Select(fsi => (XName.Get("Directory", rootName.NamespaceName), (object)fsi))
                                    .Concat(directory.GetFiles().Select(fsi => (XName.Get("File", rootName.NamespaceName), (object)fsi))),

                 FileInfo file => null,

                 _ => throw new NotSupportedException()
             }
        );
    }
}
