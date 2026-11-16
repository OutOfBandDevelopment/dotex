using System.ComponentModel.DataAnnotations;

namespace OoBDev.Data.Vectors.Postgres;

/// <summary>
/// Settings of the pgvector store.
/// </summary>
public sealed class PgVectorOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "Vectors:Postgres";

    /// <summary>The database connection string.</summary>
    [Required]
    public string ConnectionString { get; set; } = "";

    /// <summary>The schema holding the table. Letters, digits and underscore only.</summary>
    [Required, RegularExpression("^[A-Za-z_][A-Za-z0-9_]*$")]
    public string Schema { get; set; } = "public";

    /// <summary>The table name. Letters, digits and underscore only.</summary>
    [Required, RegularExpression("^[A-Za-z_][A-Za-z0-9_]*$")]
    public string Table { get; set; } = "vectors";

    /// <summary>The vector length.</summary>
    [Range(1, 2000)]
    public int Dimensions { get; set; }

    /// <summary>The distance the search orders by.</summary>
    public VectorMetric Metric { get; set; } = VectorMetric.Cosine;

    /// <summary>Stores 16-bit floats (halfvec) instead of 32-bit; halves the size and allows indexes up to 4000 dimensions.</summary>
    public bool UseHalfVector { get; set; }

    /// <summary>How many neighbours a search returns.</summary>
    [Range(1, 1000)]
    public int DefaultLimit { get; set; } = 10;

    /// <summary>The command timeout in seconds.</summary>
    [Range(1, 600)]
    public int CommandTimeoutSeconds { get; set; } = 30;
}
