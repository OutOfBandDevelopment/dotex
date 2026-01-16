using OoBDev.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.DataLoader.Providers
{
    public class DbContextProvider : IDbContextProvider
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public DbContextProvider(
            IServiceProvider serviceProvider,
            ILogger<DbContextProvider> logger
            )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public DbContext GetContext(Type dbContextType, string? databaseConnectionString)
        {
            var targetType = typeof(IDbContextProvider<>).MakeGenericType(dbContextType);
            var provider = (IDbContextProvider)_serviceProvider.GetRequiredService(targetType);

            return provider.GetContext(dbContextType, databaseConnectionString);
        }

        public IEnumerable<DbContext> GetDbContexts(IDatabaseDeploymentTemplate template)
        {
            (string TypeName, Type Type)[]? contextTypes = default;
            using (new AssemblyResolver(template.AssemblySearchPath ?? "", _logger))
            {
                contextTypes = (
                    from typeName in template.Contexts
                    select (
                        TypeName: typeName,
                        Type: Type.GetType(typeName)
                    )).ToArray();

                if (!contextTypes.Any())
                {
                    _logger.LogWarning($"Nothing to do: {{template}}", template);
                }

                var missingContextTypes = contextTypes.Where(c => c.Type == null).ToArray();
                if (missingContextTypes.Any())
                {
                    foreach (var contextType in missingContextTypes)
                        _logger.LogInformation($"{nameof(contextType)}: \"{{contextType}}\" was not loaded", contextType.TypeName);

                    throw new ApplicationException($"Unable to resolve types: " + string.Join(";", missingContextTypes.Select(ct => $"\"{ct.TypeName}\"")));
                }
            }

            if (contextTypes != null)
                foreach (var context in contextTypes)
                {
                    _logger.LogInformation($"{nameof(context)}: {{context}}", context);
                    var dbContext = GetContext(context.Type, template.ConnectionString);
                    _logger.LogInformation($"{nameof(dbContext)}: {{dbContext}}", dbContext);
                    yield return dbContext;
                }
        }
    }

    public class DbContextProvider<TContext> : IDbContextProvider<TContext>, IDbContextProvider
        where TContext : DbContext
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public DbContextProvider(
            ILogger<DbContextProvider> logger,
            IServiceProvider serviceProvider
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider; ;
        }

        DbContext IDbContextProvider.GetContext(Type dbContextType, string? databaseConnectionString) => GetContext(databaseConnectionString);

        public TContext GetContext(string? databaseConnectionString)
        {
            var dbContextDesignFactoryTypeInterface = typeof(IDesignTimeDbContextFactory<>).MakeGenericType(typeof(TContext));
            _logger.LogInformation($"{nameof(dbContextDesignFactoryTypeInterface)}: {{dbContextDesignFactoryTypeInterface}}", dbContextDesignFactoryTypeInterface);

            var dbContextDesignFactoryTypeQuery = from t in typeof(TContext).Assembly.GetTypes()
                                                  where !t.IsInterface
                                                  where !t.IsAbstract
                                                  where dbContextDesignFactoryTypeInterface.IsAssignableFrom(t)
                                                  select t;

            var dbContextDesignFactoryType = dbContextDesignFactoryTypeQuery.FirstOrDefault();
            _logger.LogInformation($"{nameof(dbContextDesignFactoryType)}: {{dbContextDesignFactoryType}}", dbContextDesignFactoryType);

            var contextFactory = GetContextFactory(dbContextDesignFactoryType);
            var context = contextFactory.CreateDbContext(new[] { databaseConnectionString }.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray());
            return context;
        }


        public IDesignTimeDbContextFactory<TContext> GetContextFactory(Type dbContextDesignFactoryType) =>
            (IDesignTimeDbContextFactory<TContext>)ActivatorUtilities.CreateInstance(
                _serviceProvider,
                true ? typeof(ExtendedDesignTimeDbContextFactory<TContext>) : dbContextDesignFactoryType
                );

        public IEnumerable<DbContext> GetDbContexts(IDatabaseDeploymentTemplate template) =>
            throw new NotImplementedException();
    }

    public class ExtendedDesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : DbContext
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public ExtendedDesignTimeDbContextFactory(
            ILogger<ExtendedDesignTimeDbContextFactory<TContext>> logger,
            IServiceProvider serviceProvider
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider; ;
        }

        public TContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseExtendedSqlServer(args[0]);
            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, typeof(TContext), optionsBuilder.Options);
            return (TContext)instance;
        }
    }
}
