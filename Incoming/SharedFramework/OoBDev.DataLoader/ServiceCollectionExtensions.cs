using OoBDev.DataLoader.DataReaders;
using OoBDev.DataLoader.JsonSerialization;
using OoBDev.DataLoader.PipeLine;
using OoBDev.DataLoader.Providers;
using OoBDev.IdentityModel.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace OoBDev.DataLoader
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataloaderServices(this IServiceCollection services)
        {
            services.TryAddTransient<IDataloaderConfig, DataloaderConfig>();
            services.TryAddTransient<IDbContextProvider, DbContextProvider>();
            services.TryAddTransient(typeof(IDbContextProvider<>), typeof(DbContextProvider<>));
            services.TryAddTransient<IDataEntityProvider, DataEntityProvider>();
            services.TryAddTransient<IMasterDataProvider, MasterDataProvider>();
            services.TryAddTransient<IDataFileProvider, DataFileProvider>();
            services.TryAddTransient<IDataFileReaderProvider, DataFileReaderProvider>();
            services.TryAddTransient<IDataPipelineProcessor, DataPipelineProcessor>();
            services.TryAddTransient<IDatabaseDeploymentTemplateFactory, DatabaseDeploymentTemplateFactory>();
            services.TryAddTransient<ISourceDataPipeline, SourceDataPipeline>();
            services.TryAddTransient<ISourceDataValidationProvider, SourceDataValidationProvider>();

            services.TryAddSingleton(sp => sp.GetRequiredService<IDatabaseDeploymentTemplateFactory>().GetTemplate());
            services.TryAddSingleton<IUserSession>(sp => sp.GetRequiredService<IDatabaseDeploymentTemplate>().UserSession);

            services.AddTransient<IDataFileReader, JsonDataFileReader>();
            services.AddTransient<IDataFileReader, CsvDataFileReader>();

            services.AddTransient<IDataPipelineHandler, GetTextData>();
            services.AddTransient<IDataPipelineHandler, DocumentUpload>();
            services.AddTransient<IDataPipelineHandler, LookupAlternativeKey>();
            services.AddTransient<IDataPipelineHandler, LookupDatabaseValuesByQuery>();
            services.AddTransient<IDataPipelineHandler, LookupDatabaseValuesByProperty>();

            services.AddTransient<JsonConverter, DateTimeOffsetConverter>();

            return services;
        }
    }
}
