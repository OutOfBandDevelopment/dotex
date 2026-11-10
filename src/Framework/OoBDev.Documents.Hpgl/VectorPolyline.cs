using System.Collections.Generic;

namespace OoBDev.Documents.Hpgl;

/// <summary>One stroked line through <see cref="Points"/>.</summary>
/// <param name="Pen">The pen number in effect when the line started.</param>
/// <param name="Color">Stroke colour as <c>#RRGGBB</c>.</param>
/// <param name="Points">The vertices, in scene units.</param>
public sealed record VectorPolyline(int Pen, string Color, IReadOnlyList<VectorPoint> Points);
