using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;

namespace OoBDev.DataLoader.Providers
{
    public interface IDbContextProvider
    {
        DbContext GetContext(Type dbContextType, string? databaseConnectionString);
        IEnumerable<DbContext> GetDbContexts(IDatabaseDeploymentTemplate template);
    }
    public interface IDbContextProvider<TContext>
        where TContext : DbContext
    {
        TContext GetContext(string? databaseConnectionString);
        IDesignTimeDbContextFactory<TContext> GetContextFactory(Type dbContextDesignFactoryType);
    }
}
