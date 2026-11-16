using OoBDev.Query.Metadata;

namespace OoBDev.Search.Parity.Tests;

[QueryKind("docs", Base = "docs d", Key = "d.id", DefaultSort = "id")]
public sealed class ParityDoc
{
    [Facet("d.id")]
    public long Id { get; set; }

    [Facet("d.name", Search = true)]
    public string Name { get; set; } = "";

    [Facet("d.ext")]
    public string Ext { get; set; } = "";

    [Facet("d.size")]
    public int Size { get; set; }
}
