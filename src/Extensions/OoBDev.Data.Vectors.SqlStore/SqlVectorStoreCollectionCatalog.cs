#pragma warning disable MA0004 // "await using" declarations cannot take ConfigureAwait
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using OoBDev.Search.Semantic;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.Vectors.SqlStore;

/// <summary>Lists the collections (the rows of <c>[embedding].[Sources]</c>) of the SQL vector store.</summary>
/// <param name="options">The store options.</param>
public sealed class SqlVectorStoreCollectionCatalog(IOptions<SqlVectorStoreOptions> options) : IVectorStoreCollectionCatalog
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var names = new List<string>();
        await using var connection = new SqlConnection(options.Value.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT [Name] FROM [embedding].[Sources] ORDER BY [Name]";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }
}
