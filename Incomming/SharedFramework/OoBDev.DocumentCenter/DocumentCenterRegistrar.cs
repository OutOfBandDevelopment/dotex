using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using OoBDev.DocumentCenter.Contracts.Providers;
using OoBDev.DocumentCenter.Contracts.Storage;
using OoBDev.DocumentCenter.Conversion;
using OoBDev.DocumentCenter.Packaging;
using OoBDev.DocumentCenter.Resolvers;
using OoBDev.DocumentCenter.Storage;
using OoBDev.Toolkit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.DocumentCenter
{
    public class DocumentCenterRegistrar : IRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.TryAddTransient<IDocumentTypeResolver, DocumentTypeResolver>();

            services.TryAddTransient<IDocumentConversionProvider, DocumentConversionProvider>();
            services.TryAddTransient<IDocumentConversionHandlerResolver, DocumentConversionHandlerResolver>();
            services.TryAddTransient<IDocumentConversionInputProvider, DocumentConversionInputProvider>();
            services.TryAddTransient<IDocumentConverter, DocumentConverter>();

            services.TryAddTransient<IBlobContainerResolver, BlobContainerResolver>();
            services.TryAddTransient<IDocumentStore, DocumentStore>();
            services.TryAddTransient<IDocumentKeyGenerator, DocumentKeyGenerator>();

            services.TryAddTransient<IDocumentPackageProvider, DocumentPackageProvider>();
            services.AddTransient<IDocumentPackageHandler, ZipFileDocumentPackageHandler>();
            services.TryAddTransient<IDocumentPackager, DocumentPackager>();
            services.TryAddTransient<IDocumentPackageHandlerResolver, DocumentPackageHandlerResolver>();

            services.TryAddTransient<IValidateContent, BypassValidateContent>();

            return services;
        }
    }
}