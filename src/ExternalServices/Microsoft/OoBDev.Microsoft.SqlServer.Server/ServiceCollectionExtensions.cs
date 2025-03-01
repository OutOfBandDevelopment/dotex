using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Data.Common;

namespace OoBDev.Microsoft.SqlServer.Server;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddMicrosoftSqlServerExtensions(this IServiceCollection services)
    {
        services.TryAddTransient<IDatabaseMapper, SqlDatabaseMapper>();
        services.TryAddKeyedTransient<IDatabaseMapper, SqlDatabaseMapper>("MSSQL");
        return services;
    }
}
