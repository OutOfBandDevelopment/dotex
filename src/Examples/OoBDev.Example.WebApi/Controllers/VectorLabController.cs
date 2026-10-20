using OoBDev.System.Math;
using OoBDev.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.WebApi.Controllers;

/// <summary>Vector arithmetic over a set of vectors: pairwise distances, magnitudes and the centroid.</summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class VectorLabController : ControllerBase
{
    private const int MaxVectors = 50;
    private const int MaxLength = 4096;

    /// <summary>Computes the pairwise distance matrix for the vectors.</summary>
    /// <param name="model">The vectors (one per row) and the metric.</param>
    /// <returns>The matrix, magnitudes and centroid.</returns>
    [HttpPost("matrix")]
    public ActionResult<VectorMatrixResponseModel> Matrix([FromBody] VectorMatrixRequestModel model)
    {
        if (model.Vectors.Count is 0 or > MaxVectors)
            return Problem(title: "Invalid vectors", detail: $"Give 1 to {MaxVectors} vectors.", statusCode: 400);
        var length = model.Vectors[0].Count;
        if (length is 0 or > MaxLength || model.Vectors.Any(v => v.Count != length))
            return Problem(title: "Invalid vectors", detail: "All vectors must have the same non-zero length.", statusCode: 400);
        if (!Enum.TryParse<VectorDistanceMetrics>(model.Metric, ignoreCase: true, out var metric))
            return Problem(title: "Unknown metric", detail: $"Use one of {string.Join(", ", Enum.GetNames<VectorDistanceMetrics>())}.", statusCode: 400);

        var vectors = model.Vectors.Select(v => Vector.Create(v)).ToList();
        var matrix = vectors.Select(a => vectors.Select(b => a.Distance(b, metric) ?? double.NaN).ToList()).ToList();
        var centroid = Enumerable.Range(0, length).Select(i => vectors.Average(v => v.Value[i])).ToList();
        return new VectorMatrixResponseModel
        {
            Matrix = matrix,
            Magnitudes = vectors.Select(v => v.Magnitude).ToList(),
            Centroid = centroid,
        };
    }
}
