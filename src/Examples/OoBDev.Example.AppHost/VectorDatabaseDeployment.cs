using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace OoBDev.Example.AppHost;

/// <summary>
/// Prepares the SQL Server 2019 container for the vector types: allows SQL CLR, then publishes the
/// OoBDev.Data.Vectors.DB dacpac (with the CLR vector types merged in) to the Aspire-created database.
/// </summary>
internal static class VectorDatabaseDeployment
{
    public static IResourceBuilder<SqlServerDatabaseResource> WithVectorTypes(this IResourceBuilder<SqlServerDatabaseResource> database, string dacpacPath)
    {
        database.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(database.Resource, async (@event, cancellationToken) =>
        {
            var server = database.Resource.Parent;
            var serverConnection = await server.ConnectionStringExpression.GetValueAsync(cancellationToken)
                ?? throw new InvalidOperationException($"No connection string for {server.Name}.");

            await EnableClrAsync(serverConnection, cancellationToken);
            Deploy(serverConnection, database.Resource.DatabaseName, dacpacPath);
        });

        return database;
    }

    private static async Task EnableClrAsync(string serverConnection, CancellationToken cancellationToken)
    {
        // The vector assembly is not signed for strict security, so this development server turns it off (see README.Data.Vectors.DB.md).
        await using var connection = new SqlConnection(serverConnection);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            """
            EXEC sp_configure 'show advanced options', 1; RECONFIGURE;
            EXEC sp_configure 'clr enabled', 1;
            EXEC sp_configure 'clr strict security', 0; RECONFIGURE;
            """, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void Deploy(string serverConnection, string databaseName, string dacpacPath)
    {
        if (!File.Exists(dacpacPath))
            throw new FileNotFoundException($"Vector database dacpac not found. Build OoBDev.Data.Vectors.DB first.", dacpacPath);

        var services = new DacServices(serverConnection);
        using var package = DacPackage.Load(dacpacPath);
        services.Deploy(package, databaseName, upgradeExisting: true, new DacDeployOptions { IncludeCompositeObjects = true });
    }
}
