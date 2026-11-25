using System.IO;
using System.Xml.XPath;
using Microsoft.SqlServer.Dac.Model;
using OoBDev.System.IO;
using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;

namespace OoBDev.Extensions.CodeAnalysis.DacFx;

/// <summary>Opens a <c>.dacpac</c> package as a navigable SQL Server model.</summary>
[FileExtension(".dacpac")]
public class DacPacNavigator : IToXPathNavigable
{
    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(string filePath) =>
        new DacPacElementNodeBuilder(new TSqlModel(filePath)).Build().ToNavigable();

    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(Stream stream)
    {
        using var temp = stream.AsTempFile();
        return ToNavigable(temp.FilePath);
    }
}
