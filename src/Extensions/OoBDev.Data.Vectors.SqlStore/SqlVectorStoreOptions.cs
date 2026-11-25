using System.ComponentModel.DataAnnotations;

namespace OoBDev.Data.Vectors.SqlStore;

/// <summary>Settings for the SQL Server vector store.</summary>
public class SqlVectorStoreOptions
{
    /// <summary>Connection string of the database deployed from <c>OoBDev.Data.Vectors.DB</c>.</summary>
    [Required]
    public string ConnectionString { get; set; } = "";

    /// <summary>Distance metric used to rank neighbors, as accepted by <c>VectorF.Distance</c> (cosine, euclidean, dotproduct, manhattan).</summary>
    public string Metric { get; set; } = "cosine";

    /// <summary>Maximum rows returned by list and neighbor queries.</summary>
    [Range(1, 100000)]
    public int Limit { get; set; } = 100;
}
