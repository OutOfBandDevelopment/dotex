namespace OoBDev.Query.Metadata.Tests;

[QueryKind("hinted", Base = "files f", Key = "f.id", DefaultSort = "name")]
[Hint("copies", "EXISTS (SELECT 1 FROM files c WHERE c.hash = f.hash AND c.id <> {k})")]
[Hint("burst", "f.burst_id IS NOT NULL")]
public sealed class HintedRow
{
    [Facet("f.name")]
    public string Name { get; set; } = "";

    public HintFlags? Has { get; set; }
}
