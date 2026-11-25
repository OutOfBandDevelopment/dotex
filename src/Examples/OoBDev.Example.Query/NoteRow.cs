using System;
using OoBDev.Query.Metadata;

namespace OoBDev.Example.Query;

/// <summary>A row class for a table <c>notes(id, title, body, created, lat, lon)</c>; the attributes are everything a client may ask.</summary>
[QueryKind("notes", Title = "Notes", Base = "notes n", Key = "n.id", DefaultSort = "-created")]
[Space("geo", SpaceMetric.Haversine, A = "lat", B = "lon")]
public sealed class NoteRow
{
    [Facet("n.id", Sortable = false)]
    public long Id { get; set; }

    [Facet("n.title", Search = true, Role = "title")]
    public string Title { get; set; } = "";

    [Facet("n.created", Format = "date")]
    public DateTime? Created { get; set; }

    [Facet("n.lat")]
    public double? Lat { get; set; }

    [Facet("n.lon")]
    public double? Lon { get; set; }

    public double? Distance { get; set; }
}
