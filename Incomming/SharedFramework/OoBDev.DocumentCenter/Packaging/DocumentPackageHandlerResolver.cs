using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.DocumentCenter.Packaging
{
    public class DocumentPackageHandlerResolver : IDocumentPackageHandlerResolver
    {
        private readonly IServiceProvider _provider;
        public DocumentPackageHandlerResolver(
            IServiceProvider provider
            )
        {
            _provider = provider;
        }

        internal IEnumerable<IDocumentPackageHandler> GetInputHandlers(PackageTypes packageType) =>
            from handler in _provider.GetServices<IDocumentPackageHandler>()
            from attribute in handler.GetType().GetCustomAttributes<PackageHandlerAttribute>() ?? Enumerable.Empty<PackageHandlerAttribute>()
            where attribute.PackageType == packageType
            orderby attribute.Priority descending
            select handler;

        public IDocumentPackageHandler GetHandler(PackageTypes packageType) => GetInputHandlers(packageType).FirstOrDefault();
    }
}
